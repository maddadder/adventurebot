using DurableFunctionDemoConfig.Models;
using DurableFunctionDemoConfig.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace DurableFunctionDemoConfig.ActivityFunctions
{
    public class ReportRepoViewCount
    {
        private readonly IAwsSesApiService _awsSesApiService;

        public ReportRepoViewCount(IAwsSesApiService awsSesApiService)
        {
            _awsSesApiService = awsSesApiService;
        }

        [FunctionName(nameof(ReportRepoViewCount))]
        public async Task Run(
            [ActivityTrigger] IDurableActivityContext context,
            ILogger log)
        {
            var repoViewCounts = context.GetInput<RepoViewCount[]>();
            foreach (var repoViewCount in repoViewCounts)
            {
                log.LogInformation($"Repository {repoViewCount.RepoName}'s view count is {repoViewCount.ViewCount}");
            }
            var emailMessage = await _awsSesApiService.RenderRepoViewCount(repoViewCounts);
            await _awsSesApiService.SendEmail("Repository view count", emailMessage);
        }
    }
}
