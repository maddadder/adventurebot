using AdventureBot.ActivityFunctions;
using AdventureBot.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using AdventureBot.EntityTriggers;

namespace AdventureBot.Orchestrators
{
    public class DiscordLoopOrchestration
    {

        [FunctionName(nameof(DiscordLoopOrchestration))]
        public async Task<bool> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger log)
        {
            var input = context.GetInput<InitializeDiscordLoopInput>();
            if(string.IsNullOrEmpty(input.TargetChannelId))
            {
                return false;
            }
            log.LogInformation("1. Send confirmation email and store a mapping so users can't cheat as easily.");
            List<SendReceiveDiscordStateInput> mapping = new List<SendReceiveDiscordStateInput>();
            var sendReceiveGameStateInput = new SendReceiveDiscordStateInput
            {
                RegistrationConfirmationURL = $"{input.BaseUri}",
                InstanceId = input.InstanceId,
                SubscriberId = context.NewGuid().ToString(),
                GameState = input.InitialGameState,
                TargetChannelId = input.TargetChannelId
            };
            mapping.Add(sendReceiveGameStateInput);
             log.LogInformation("1.5. Call DiscordStateLoopActivity to send out game state and return the current list of options.");
            var gameOptions = await context.CallActivityAsync<List<GameOption>>(nameof(DiscordStateLoopActivity), sendReceiveGameStateInput);
            log.LogInformation("2. Initialize an entity and set the priorVote, voteInstanceId, targetChannelId, and gameOptions");
            var entityId = new EntityId(EntityTriggerDiscordVotingCounter.Name.Vote, $"{EntityTriggerDiscordVotingCounter.Name.Vote},{input.InstanceId}");
            context.SignalEntity(entityId, DiscordVotingCounterOperationNames.SetPriorVote, input.InitialGameState);
            context.SignalEntity(entityId, DiscordVotingCounterOperationNames.SetVoteInstanceId, sendReceiveGameStateInput.SubscriberId);
            context.SignalEntity(entityId, DiscordVotingCounterOperationNames.SetTargetChannelId, sendReceiveGameStateInput.TargetChannelId);
            context.SignalEntity(entityId, DiscordVotingCounterOperationNames.SetGameOptions, gameOptions);

            log.LogInformation("3. Setup timer for 1 day and wait for external event to be executed. Whatever comes first is the winner (timer vs input)");
            using (var ctsGameTimeout = new CancellationTokenSource())
            {
                var expiredAt = context.CurrentUtcDateTime.Add(TimeSpan.FromDays(1));
                var gameTimeout = context.CreateTimer(expiredAt, ctsGameTimeout.Token);

                var customStatus = new DiscordLoopOrchestatorStatus { Text = $"Waiting for user response", ExpireAt = expiredAt };
                context.SetCustomStatus(customStatus);

                var gameAdvanceButtonClicked = context.WaitForExternalEvent<DiscordLoopInput>(EventNames.DiscordStateAdvanced);
                
                log.LogInformation("Whatever comes first is the winner (timer vs input)");
                var winner = await Task.WhenAny(gameAdvanceButtonClicked, gameTimeout);
                
                if (winner == gameAdvanceButtonClicked)
                {
                    var gameLoopInput = gameAdvanceButtonClicked.Result;
                    log.LogInformation("3. Setup another timer based on the input.GameDelay. Wait until the timeout expires");
                    using (var ctsGameDelayTimeout = new CancellationTokenSource())
                    {
                        if(mapping.Select(x => x.SubscriberId).Contains(gameLoopInput.SubscriberId))
                        {
                            log.LogInformation("4. Add the vote to the tally");
                            context.SignalEntity(entityId, DiscordVotingCounterOperationNames.Vote, gameLoopInput);
                        }
                        log.LogInformation("5. Initialize the WaitForExternalEvent and then loop on the await.");
                        log.LogInformation("Only break out when the timeout expires");
                        var gameDelayExpiredAt = context.CurrentUtcDateTime.Add(input.GameDelay);
                        var gameDelayTimeout = context.CreateTimer(gameDelayExpiredAt, ctsGameDelayTimeout.Token);
                        var gameAdvanceButtonClickedBeforeTimeout = context.WaitForExternalEvent<DiscordLoopInput>(EventNames.DiscordStateAdvanced);
                        while(await Task.WhenAny(gameAdvanceButtonClickedBeforeTimeout, gameDelayTimeout) != gameDelayTimeout)
                        {
                            log.LogInformation("6. Add the vote to the tally");
                            gameLoopInput = gameAdvanceButtonClickedBeforeTimeout.Result;
                            if(mapping.Select(x => x.SubscriberId).Contains(gameLoopInput.SubscriberId))
                            {
                               context.SignalEntity(entityId, DiscordVotingCounterOperationNames.Vote, gameLoopInput);
                            }
                            gameAdvanceButtonClickedBeforeTimeout = context.WaitForExternalEvent<DiscordLoopInput>(EventNames.DiscordStateAdvanced);
                        }
                        log.LogInformation("7. Get the current state of the votingCounter");
                        DiscordVotingCounter votingCounter = await context.CallEntityAsync<DiscordVotingCounter>(entityId, DiscordVotingCounterOperationNames.Get);
                        Dictionary<string, int> votes = votingCounter.VoteCount;
                        log.LogInformation("8. Tally up the votes, and get the new game state");
                        var newGameState = await context.CallActivityAsync<string>(nameof(TallyVoteActivity), votes);
                        if(!string.IsNullOrEmpty(newGameState)) //a null newGameState is a tie
                        {
                            log.LogInformation($"The Gamestate {newGameState} won the vote");
                            input.InitialGameState = newGameState;
                        }
                        log.LogInformation("9. Delete the entity because we are done with it");
                        context.SignalEntity(entityId, DiscordVotingCounterOperationNames.Delete);
                        log.LogInformation($"Saved Game State: {input.InitialGameState}");
                        log.LogInformation("10. restart the workflow with new input");
                        context.ContinueAsNew(input, false);
                    }
                }
                else
                {
                    context.SetCustomStatus(new DiscordLoopOrchestatorStatus { Text = "Game timed out" });
                }
                
                if (!gameTimeout.IsCompleted)
                {
                    log.LogInformation("All pending timers must be complete or canceled before the function exits.");
                    ctsGameTimeout.Cancel();
                }
                return true;
            }
            
        }
    }
}