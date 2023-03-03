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
    public class GameLoopOrchestration
    {

        [FunctionName(nameof(GameLoopOrchestration))]
        public async Task<bool> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger log)
        {
            var input = context.GetInput<InitializeGameLoopInput>();
            if(input.Subscribers == null || !input.Subscribers.Any())
            {
                return false;
            }
            // 1. Send confirmation email
            foreach(var subscriber in input.Subscribers)
            {
                var sendReceiveGameStateInput = new SendReceiveGameStateInput
                {
                    RegistrationConfirmationURL = $"{input.BaseUri}",
                    InstanceId = input.InstanceId,
                    Email = subscriber,
                    Name = subscriber,
                    GameState = input.InitialGameState,
                    Subscribers = input.Subscribers
                };
                
                await context.CallActivityAsync(nameof(GameStateLoopActivity), sendReceiveGameStateInput);
            }
            // 2. Initialize an entity and set the prior vote
            var entityId = new EntityId(EntityTriggerVotingCounter.Name.Vote, $"{EntityTriggerVotingCounter.Name.Vote},{input.InstanceId}");
            context.SignalEntity(entityId, VotingCounterOperationNames.SetPriorVote, input.InitialGameState);

            // 3. Setup timer for 1 day and wait for external event to be executed. Whatever comes first is the winner (timer vs input)
            using (var ctsGameTimeout = new CancellationTokenSource())
            {
                var expiredAt = context.CurrentUtcDateTime.Add(TimeSpan.FromDays(1));
                var gameTimeout = context.CreateTimer(expiredAt, ctsGameTimeout.Token);

                var customStatus = new GameLoopOrchestatorStatus { Text = $"Waiting for user response", ExpireAt = expiredAt };
                context.SetCustomStatus(customStatus);

                var gameAdvanceButtonClicked = context.WaitForExternalEvent<GameLoopInput>(EventNames.GameStateAdvanced);
                
                // Whatever comes first is the winner (timer vs input)
                var winner = await Task.WhenAny(gameAdvanceButtonClicked, gameTimeout);
                
                if (winner == gameAdvanceButtonClicked)
                {
                    var gameLoopInput = gameAdvanceButtonClicked.Result;
                    // 3. Setup another timer based on the input.GameDelay. Wait until the timeout expires
                    using (var ctsGameDelayTimeout = new CancellationTokenSource())
                    {
                        if(input.Subscribers.Contains(gameLoopInput.Subscriber))
                        {
                            // 4. Add the vote to the Tally
                            context.SignalEntity(entityId, VotingCounterOperationNames.Vote, gameLoopInput);
                        }
                        // 5. Initialize the WaitForExternalEvent and then loop on the await.
                        //    Only break out when the timeout expires
                        var gameDelayExpiredAt = context.CurrentUtcDateTime.Add(input.GameDelay);
                        var gameDelayTimeout = context.CreateTimer(gameDelayExpiredAt, ctsGameDelayTimeout.Token);
                        var gameAdvanceButtonClickedBeforeTimeout = context.WaitForExternalEvent<GameLoopInput>(EventNames.GameStateAdvanced);
                        while(await Task.WhenAny(gameAdvanceButtonClickedBeforeTimeout, gameDelayTimeout) != gameDelayTimeout)
                        {
                            gameLoopInput = gameAdvanceButtonClickedBeforeTimeout.Result;
                            if(input.Subscribers.Contains(gameLoopInput.Subscriber))
                            {
                                context.SignalEntity(entityId, VotingCounterOperationNames.Vote, gameLoopInput);
                            }
                            gameAdvanceButtonClickedBeforeTimeout = context.WaitForExternalEvent<GameLoopInput>(EventNames.GameStateAdvanced);
                        }
                        // 6. Get the current state of the votingCounter
                        VotingCounter votingCounter = await context.CallEntityAsync<VotingCounter>(entityId, VotingCounterOperationNames.Get);
                        Dictionary<string, int> votes = votingCounter.VoteCount;
                        // 7. Tally up the votes, and get the new game state
                        var newGameState = await context.CallActivityAsync<string>(nameof(TallyVoteActivity), votes);
                        if(!string.IsNullOrEmpty(newGameState)) // a null newGameState is a tie 
                        {
                            input.InitialGameState = newGameState;
                        }
                        // 8. Delete the entity because we are done with it
                        context.SignalEntity(entityId, VotingCounterOperationNames.Delete);
                        log.LogInformation($"New Game State: {input.InitialGameState}");
                        // 9. restart the workflow with new input
                        context.ContinueAsNew(input, false);
                    }
                }
                else
                {
                    context.SetCustomStatus(new GameLoopOrchestatorStatus { Text = "Game timed out" });
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