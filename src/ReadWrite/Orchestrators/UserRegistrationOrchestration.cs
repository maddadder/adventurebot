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
    public class UserRegistrationOrchestration
    {
        
        [FunctionName(nameof(UserRegistrationOrchestration))]
        public async Task<bool> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger log)
        {
            var input = context.GetInput<UserRegistrationInput>();

            // 1. Send confirmation email
            var sendConfirmationEmailInput = new SendConfirmationEmailInput
            {
                RegistrationConfirmationURL = $"{input.BaseUri}/UserRegistration/get/{input.InstanceId}",
                Email = input.Email,
                Name = input.Name
            };

            await context.CallActivityAsync(nameof(SendConfirmationEmailActivity), sendConfirmationEmailInput);

            // 2. Setup timer and wait for external event to be executed. Whatever comes first continues            
            using (var cts = new CancellationTokenSource())
            {
                var expiredAt = context.CurrentUtcDateTime.Add(TimeSpan.FromDays(7));
                var customStatus = new UserRegistrationOrchestatorStatus { Text = "Waiting for email confirmation", ExpireAt = expiredAt };

                context.SetCustomStatus(customStatus);

                var confirmationButtonClicked = context.WaitForExternalEvent<bool>("EmailConfirmationReceived");
                var timeout = context.CreateTimer(expiredAt, cts.Token);

                var winner = await Task.WhenAny(confirmationButtonClicked, timeout);

                // Cancel the timer if it has not yet been completed
                if (!timeout.IsCompleted)
                    cts.Cancel();

                if (winner == confirmationButtonClicked)
                {
                    await context.CallActivityAsync(nameof(SetUserEmailVerification), sendConfirmationEmailInput.Email);
                    context.SetCustomStatus(new UserRegistrationOrchestatorStatus { Text = "Email activation succeeded" });                    
                    return true;
                }
                else
                {
                    context.SetCustomStatus(new UserRegistrationOrchestatorStatus { Text = "Email activation timed out" });
                    return false;
                }
            }                         
        }
    }
}