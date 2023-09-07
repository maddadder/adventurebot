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
            // 1. Send confirmation email and store a mapping so users can't cheat as easily.
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
            await context.CallActivityAsync(nameof(DiscordStateLoopActivity), sendReceiveGameStateInput);
            // 2. Initialize an entity and set the prior vote
            var entityId = new EntityId(EntityTriggerDiscordVotingCounter.Name.Vote, $"{EntityTriggerDiscordVotingCounter.Name.Vote},{input.InstanceId}");
            context.SignalEntity(entityId, DiscordVotingCounterOperationNames.DiscordSetPriorVote, input.InitialGameState);

            // 3. Setup timer for 1 day and wait for external event to be executed. Whatever comes first is the winner (timer vs input)
            using (var ctsGameTimeout = new CancellationTokenSource())
            {
                var expiredAt = context.CurrentUtcDateTime.Add(TimeSpan.FromDays(1));
                var gameTimeout = context.CreateTimer(expiredAt, ctsGameTimeout.Token);

                var customStatus = new DiscordLoopOrchestatorStatus { Text = $"Waiting for user response", ExpireAt = expiredAt };
                context.SetCustomStatus(customStatus);

                var gameAdvanceButtonClicked = context.WaitForExternalEvent<DiscordLoopInput>(EventNames.DiscordStateAdvanced);
                
                // Whatever comes first is the winner (timer vs input)
                var winner = await Task.WhenAny(gameAdvanceButtonClicked, gameTimeout);
                
                if (winner == gameAdvanceButtonClicked)
                {
                    var gameLoopInput = gameAdvanceButtonClicked.Result;
                    // 3. Setup another timer based on the input.GameDelay. Wait until the timeout expires
                    using (var ctsGameDelayTimeout = new CancellationTokenSource())
                    {
                        if(mapping.Select(x => x.SubscriberId).Contains(gameLoopInput.SubscriberId))
                        {
                            // 4. Add the vote to the tally
                            context.SignalEntity(entityId, DiscordVotingCounterOperationNames.DiscordVote, gameLoopInput);
                        }
                        // 5. Initialize the WaitForExternalEvent and then loop on the await.
                        //    Only break out when the timeout expires
                        var gameDelayExpiredAt = context.CurrentUtcDateTime.Add(input.GameDelay);
                        var gameDelayTimeout = context.CreateTimer(gameDelayExpiredAt, ctsGameDelayTimeout.Token);
                        var gameAdvanceButtonClickedBeforeTimeout = context.WaitForExternalEvent<DiscordLoopInput>(EventNames.DiscordStateAdvanced);
                        while(await Task.WhenAny(gameAdvanceButtonClickedBeforeTimeout, gameDelayTimeout) != gameDelayTimeout)
                        {
                            // 6. Add the vote to the tally
                            gameLoopInput = gameAdvanceButtonClickedBeforeTimeout.Result;
                            if(mapping.Select(x => x.SubscriberId).Contains(gameLoopInput.SubscriberId))
                            {
                               context.SignalEntity(entityId, DiscordVotingCounterOperationNames.DiscordVote, gameLoopInput);
                            }
                            gameAdvanceButtonClickedBeforeTimeout = context.WaitForExternalEvent<DiscordLoopInput>(EventNames.DiscordStateAdvanced);
                        }
                        // 7. Get the current state of the votingCounter
                        DiscordVotingCounter votingCounter = await context.CallEntityAsync<DiscordVotingCounter>(entityId, DiscordVotingCounterOperationNames.DiscordGet);
                        Dictionary<string, int> votes = votingCounter.VoteCount;
                        // 8. Tally up the votes, and get the new game state
                        var newGameState = await context.CallActivityAsync<string>(nameof(TallyVoteActivity), votes);
                        if(!string.IsNullOrEmpty(newGameState)) // a null newGameState is a tie 
                        {
                            input.InitialGameState = newGameState;
                        }
                        // 9. Delete the entity because we are done with it
                        context.SignalEntity(entityId, DiscordVotingCounterOperationNames.DiscordDelete);
                        log.LogInformation($"New Game State: {input.InitialGameState}");
                        // 10. restart the workflow with new input
                        context.ContinueAsNew(input, false);
                    }
                }
                else
                {
                    context.SetCustomStatus(new DiscordLoopOrchestatorStatus { Text = "Game timed out" });
                }
                
                if (!gameTimeout.IsCompleted)
                {
                    // All pending timers must be complete or canceled before the function exits.
                    ctsGameTimeout.Cancel();
                }
                return true;
            }
            
        }
    }
}