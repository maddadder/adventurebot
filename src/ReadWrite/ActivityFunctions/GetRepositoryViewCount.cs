using DurableFunctionDemoConfig.Models;
using DurableFunctionDemoConfig.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace DurableFunctionDemoConfig.ActivityFunctions
{
    public class GetRepositoryViewCount
    {
        private readonly IGitHubApiService _gitHubApiService;

        public GetRepositoryViewCount(IGitHubApiService gitHubApiService)
        {
            _gitHubApiService = gitHubApiService;
        }

        [FunctionName(nameof(GetRepositoryViewCount))]
        public async Task<RepoViewCount> Run(
            [ActivityTrigger] IDurableActivityContext context,
            ILogger log)
        {
            var repoName = context.GetInput<string>();
            return await _gitHubApiService.GetRepositoryViewCount(repoName);
        }
    }
}
