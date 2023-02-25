using System.Text;
using System.Threading.Tasks;
using AdventureBot.Models;
using AdventureBot.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace AdventureBot.ActivityFunctions
{
    public class SendConfirmationEmailActivity
    {
        private readonly IAwsSesApiService _awsSesApiService;

        public SendConfirmationEmailActivity(IAwsSesApiService awsSesApiService)
        {
            _awsSesApiService = awsSesApiService;
        }

        [FunctionName(nameof(SendConfirmationEmailActivity))]
        public async Task Run(          
            [ActivityTrigger] SendConfirmationEmailInput input, ILogger log)
        {
            if(!string.IsNullOrEmpty(input.Email) && input.Email.Contains("@"))
            {
                var userPrefix = input.Email.Split("@")[0];
                var azureAdUserName = $"{userPrefix}@{AzureAd.TennantName}";
                var htmlContent = new StringBuilder();
                htmlContent
                    .AppendLine("<html>")
                    .AppendLine($"<head><meta name=\"viewport\" content=\"width=device-width\" /><meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\" /><title>AdventureBot Email confirmation</title>")
                    .AppendLine("<body>")
                    .AppendLine($"<p>Hello {input.Name}</p>")
                    .AppendLine($"<p>To activate your account open this page: <a href=\"{input.RegistrationConfirmationURL}\">{input.RegistrationConfirmationURL}</a></p>")
                    .AppendLine($"<p>Remember that your username to login is {azureAdUserName}</p>")
                    .AppendLine("</body></html>");

                await _awsSesApiService.SendEmail(input.Email, "AdventureBot Email Confirmation", htmlContent.ToString());
                log.LogInformation($"Email sent to {input.Email} with confirmation URL {input.RegistrationConfirmationURL}");
            }
        }

    }
}