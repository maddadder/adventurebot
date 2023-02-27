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
            
            // 1. Send confirmation email
            var sendReceiveGameStateInput = new SendReceiveGameStateInput
            {
                RegistrationConfirmationURL = $"{input.BaseUri}/gameloop/{input.InstanceId}",
                Email = input.Email,
                Name = input.Name,
                GameState = input.InitialGameState,
            };
            input.PriorState.Enqueue(input.InitialGameState);
            if(input.PriorState.Count >= 3)
            {
                input.PriorState.Dequeue();
            }
            if(input.PriorState.Where(x => x == input.InitialGameState).Count() <= 1)
            {
                await context.CallActivityAsync(nameof(GameStateLoopActivity), sendReceiveGameStateInput);
            }
            // 2. Setup timer and wait for external event to be executed. Whatever comes first continues            
            using (var ctsGameTimeout = new CancellationTokenSource())
            {
                var expiredAt = context.CurrentUtcDateTime.Add(TimeSpan.FromDays(1));
                var gameTimeout = context.CreateTimer(expiredAt, ctsGameTimeout.Token);

                var customStatus = new GameLoopOrchestatorStatus { Text = $"Waiting for user response, prior states: {string.Join(",",input.PriorState)}", ExpireAt = expiredAt };
                context.SetCustomStatus(customStatus);

                var gameAdvanceButtonClicked = context.WaitForExternalEvent<string>("GameStateAdvanced");
                
                var winner = await Task.WhenAny(gameAdvanceButtonClicked, gameTimeout);
                
                if (winner == gameAdvanceButtonClicked)
                {
                    input.InitialGameState = gameAdvanceButtonClicked.Result;
                    using (var ctsGameDelayTimeout = new CancellationTokenSource())
                    {
                        var gameDelayExpiredAt = context.CurrentUtcDateTime.Add(input.GameDelay);
                        var gameDelayTimeout = context.CreateTimer(gameDelayExpiredAt, ctsGameDelayTimeout.Token);
                        var gameAdvanceButtonClickedBeforeTimeout = context.WaitForExternalEvent<string>("GameStateAdvanced");
                        while(await Task.WhenAny(gameAdvanceButtonClickedBeforeTimeout, gameDelayTimeout) != gameDelayTimeout)
                        {
                            input.InitialGameState = gameAdvanceButtonClickedBeforeTimeout.Result;
                            gameAdvanceButtonClickedBeforeTimeout = context.WaitForExternalEvent<string>("GameStateAdvanced");
                        }
                        
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