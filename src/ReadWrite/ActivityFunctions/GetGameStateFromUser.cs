using AdventureBot.Models;
using AdventureBot.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace AdventureBot.ActivityFunctions
{
    public class GetGameStateFromUser
    {
        private readonly ICosmosApiService _cosmosApiService;

        public GetGameStateFromUser(ICosmosApiService cosmosApiService)
        {
            _cosmosApiService = cosmosApiService;
        }

        [FunctionName(nameof(GetGameStateFromUser))]
        public async Task<UserProfileGameEntry> Run(
            [ActivityTrigger] IDurableActivityContext context,
            ILogger log)
        {
            var userProfile = context.GetInput<UserProfile>();
            return await _cosmosApiService.GetGameStateFromUser(userProfile);
        }
    }
}
