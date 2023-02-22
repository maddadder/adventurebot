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
    public class GetUserProfileList
    {
        private readonly ICosmosApiService _cosmosApiService;
        
        public GetUserProfileList(
            
            ICosmosApiService cosmosApiService
        )
        {
            _cosmosApiService = cosmosApiService;
        }

        [FunctionName(nameof(GetUserProfileList))]
        public async Task<List<UserProfile>> Run(
            [ActivityTrigger] IDurableActivityContext context,
            ILogger log)
        {
            
            return await _cosmosApiService.GetUserProfileList();
        }
    }
}
