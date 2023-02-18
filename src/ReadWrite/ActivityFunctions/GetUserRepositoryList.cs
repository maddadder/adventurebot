using DurableFunctionDemoConfig.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DurableFunctionDemoConfig.ActivityFunctions
{
    public class GetUserRepositoryList
    {
        private readonly IGitHubApiService _gitHubApiService;

        public GetUserRepositoryList(IGitHubApiService gitHubApiService)
        {
            _gitHubApiService = gitHubApiService;
        }

        [FunctionName(nameof(GetUserRepositoryList))]
        public async Task<List<string>> Run(
            [ActivityTrigger] IDurableActivityContext context,
            ILogger log)
        {
            return await _gitHubApiService.GetUserRepositoryList();
        }
    }
}
