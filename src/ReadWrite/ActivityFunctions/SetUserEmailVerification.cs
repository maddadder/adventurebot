using AdventureBot.Models;
using AdventureBot.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace AdventureBot.ActivityFunctions
{
    public class SetUserEmailVerification
    {
        private readonly ICosmosApiService _cosmosApiService;

        public SetUserEmailVerification(ICosmosApiService cosmosApiService)
        {
            _cosmosApiService = cosmosApiService;
        }

        [FunctionName(nameof(SetUserEmailVerification))]
        public async Task Run(
            [ActivityTrigger] IDurableActivityContext context,
            ILogger log)
        {
            var emailAddress = context.GetInput<string>();
            await _cosmosApiService.SetUserEmailVerification(emailAddress);
        }
    }
}
