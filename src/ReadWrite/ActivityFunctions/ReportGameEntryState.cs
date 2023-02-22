using AdventureBot.Models;
using AdventureBot.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace AdventureBot.ActivityFunctions
{
    public class ReportGameEntryState
    {
        private readonly IAwsSesApiService _awsSesApiService;

        public ReportGameEntryState(IAwsSesApiService awsSesApiService)
        {
            _awsSesApiService = awsSesApiService;
        }

        [FunctionName(nameof(ReportGameEntryState))]
        public async Task Run(
            [ActivityTrigger] IDurableActivityContext context,
            ILogger log)
        {
            UserProfileGameEntry entry = context.GetInput<UserProfileGameEntry>();
            log.LogInformation($"User {entry.userProfile?.PreferredUsername}'s game state is {entry.gameEntry?.name}");
            var emailMessage = await _awsSesApiService.RenderUserProfileGameEntry(entry);
            if(!string.IsNullOrEmpty(emailMessage))
            {
                await _awsSesApiService.SendEmail(entry.userProfile, "Adventure Bot", emailMessage);
            }
        }
    }
}
