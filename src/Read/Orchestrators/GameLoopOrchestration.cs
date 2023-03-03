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
                    RegistrationConfirmationURL = $"{input.BaseUri}/gameloop/{input.InstanceId}",
                    Email = subscriber,
                    Name = subscriber,
                    GameState = input.InitialGameState,
                    Subscribers = input.Subscribers
                };
                
                await context.CallActivityAsync(nameof(GameStateLoopActivity), sendReceiveGameStateInput);
            }
            // 2. Setup timer and wait for external event to be executed. Whatever comes first continues            
            using (var ctsGameTimeout = new CancellationTokenSource())
            {
                var expiredAt = context.CurrentUtcDateTime.Add(TimeSpan.FromDays(1));
                var gameTimeout = context.CreateTimer(expiredAt, ctsGameTimeout.Token);

                var customStatus = new GameLoopOrchestatorStatus { Text = $"Waiting for user response", ExpireAt = expiredAt };
                context.SetCustomStatus(customStatus);

                var gameAdvanceButtonClicked = context.WaitForExternalEvent<GameLoopInput>(EventNames.GameStateAdvanced);
                
                var winner = await Task.WhenAny(gameAdvanceButtonClicked, gameTimeout);
                
                if (winner == gameAdvanceButtonClicked)
                {
                    var gameLoopInput = gameAdvanceButtonClicked.Result;
                    using (var ctsGameDelayTimeout = new CancellationTokenSource())
                    {
                        var entityId = new EntityId(EntityTriggerVotingCounter.Name.Vote, $"{EntityTriggerVotingCounter.Name.Vote},{input.InstanceId}");
                        if(input.Subscribers.Contains(gameLoopInput.Subscriber))
                        {
                            context.SignalEntity(entityId, "add", gameLoopInput);
                        }
                        var gameDelayExpiredAt = context.CurrentUtcDateTime.Add(input.GameDelay);
                        var gameDelayTimeout = context.CreateTimer(gameDelayExpiredAt, ctsGameDelayTimeout.Token);
                        var gameAdvanceButtonClickedBeforeTimeout = context.WaitForExternalEvent<GameLoopInput>(EventNames.GameStateAdvanced);
                        while(await Task.WhenAny(gameAdvanceButtonClickedBeforeTimeout, gameDelayTimeout) != gameDelayTimeout)
                        {
                            gameLoopInput = gameAdvanceButtonClickedBeforeTimeout.Result;
                            if(input.Subscribers.Contains(gameLoopInput.Subscriber))
                            {
                                context.SignalEntity(entityId, "add", gameLoopInput);
                            }
                            gameAdvanceButtonClickedBeforeTimeout = context.WaitForExternalEvent<GameLoopInput>(EventNames.GameStateAdvanced);
                        }
                        VotingCounter votingCounter = await context.CallEntityAsync<VotingCounter>(entityId, "get");
                        Dictionary<string, int> votes = votingCounter.Get();
                        var newGameState = await context.CallActivityAsync<string>(nameof(TallyVoteActivity), votes);
                        if(!string.IsNullOrEmpty(newGameState)) // a null newGameState is a tie 
                        {
                            input.InitialGameState = newGameState;
                        }
                        context.SignalEntity(entityId, "delete");
                        log.LogInformation($"New Game State: {input.InitialGameState}");
                        // restart the workflow with new input
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