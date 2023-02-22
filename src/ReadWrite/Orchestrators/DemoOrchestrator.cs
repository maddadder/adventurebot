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
namespace AdventureBot.Orchestrators
{
    public class DemoOrchestrator
    {
        [FunctionName(nameof(DemoOrchestrator))]
        public async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger log)
        {
            var UserProfiles = await context.CallActivityAsync<List<UserProfile>>(nameof(GetActiveUserProfileList), null);
            var list = string.Join(',', UserProfiles.Select(x => x.PreferredUsername));
            log.LogInformation($"Users: {list}");

            var queryTasks = new List<Task<UserProfileGameEntry>>();

            // fan-out
            foreach (var userProfile in UserProfiles)
            {
                var task = context.CallActivityAsync<UserProfileGameEntry>(nameof(GetGameStateFromUser), userProfile);
                queryTasks.Add(task);
            }

            // fan-in
            UserProfileGameEntry[] entries = await Task.WhenAll(queryTasks);
            var emailTasks = new List<Task>();

            // fan-out
            foreach (var entry in entries)
            {
                var task = context.CallActivityAsync(nameof(ReportGameEntryState), entry);
                emailTasks.Add(task);
            }
            // fan-in
            await Task.WhenAll(emailTasks);
        }
    }
}
