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
                GameState = input.InitialGameState
            };

            await context.CallActivityAsync(nameof(GameStateLoopActivity), sendReceiveGameStateInput);

            // 2. Setup timer and wait for external event to be executed. Whatever comes first continues            
            using (var cts = new CancellationTokenSource())
            {
                var expiredAt = context.CurrentUtcDateTime.Add(TimeSpan.FromDays(1));
                var timeout = context.CreateTimer(expiredAt, cts.Token);

                var customStatus = new GameLoopOrchestatorStatus { Text = "Waiting for user response", ExpireAt = expiredAt };
                context.SetCustomStatus(customStatus);

                var confirmationButtonClicked = context.WaitForExternalEvent<string>("GameStateAdvanced");
                
                var winner = await Task.WhenAny(confirmationButtonClicked, timeout);
                
                if (winner == confirmationButtonClicked)
                {
                    input.InitialGameState = confirmationButtonClicked.Result;
                    log.LogInformation(input.InitialGameState);
                    
                    context.ContinueAsNew(input);
                }
                else
                {
                    context.SetCustomStatus(new GameLoopOrchestatorStatus { Text = "Game timed out" });
                }
                
                if (!timeout.IsCompleted)
                {
                    // All pending timers must be complete or canceled before the function exits.
                    cts.Cancel();
                }
                return true;
            }
            
        }
    }
}