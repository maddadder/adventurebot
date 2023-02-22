using AdventureBot.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using AdventureBot.Models;

namespace AdventureBot.ActivityFunctions
{
    public class GetActiveUserProfileList
    {
        private readonly ICosmosApiService _cosmosApiService;
        
        public GetActiveUserProfileList(
            
            ICosmosApiService cosmosApiService
        )
        {
            _cosmosApiService = cosmosApiService;
        }

        [FunctionName(nameof(GetActiveUserProfileList))]
        public async Task<List<UserProfile>> Run(
            [ActivityTrigger] IDurableActivityContext context,
            ILogger log)
        {
            
            return await _cosmosApiService.GetActiveUserProfileList();
        }
    }
}
