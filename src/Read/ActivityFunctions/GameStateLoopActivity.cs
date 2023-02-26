using System.Text;
using System.Threading.Tasks;
using AdventureBot.Models;
using AdventureBot.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Linq;
namespace AdventureBot.ActivityFunctions
{
    public class GameStateLoopActivity
    {
        private readonly IAwsSesApiService _awsSesApiService;
        private readonly ICosmosApiService _cosmosApiService;

        public GameStateLoopActivity(
            IAwsSesApiService awsSesApiService,
            ICosmosApiService cosmosApiService)
        {
            _awsSesApiService = awsSesApiService;
            _cosmosApiService = cosmosApiService;
        }

        [FunctionName(nameof(GameStateLoopActivity))]
        public async Task Run(          
            [ActivityTrigger] SendReceiveGameStateInput input, ILogger log)
        {
            if(!string.IsNullOrEmpty(input.Email) && input.Email.Contains("@") && !string.IsNullOrEmpty(input.GameState))
            {
                var gameEntry = await _cosmosApiService.GetGameStatesFromOption(input.GameState);
                string emailMessage = await _awsSesApiService.RenderGameStateGameEntry(input, gameEntry.FirstOrDefault());
                if(!string.IsNullOrEmpty(emailMessage)){
                    await _awsSesApiService.SendEmail(input.Email, $"AdventureBot - {input.GameState}", emailMessage);
                    log.LogInformation($"Email sent to {input.Email} with game state URL {input.RegistrationConfirmationURL}");
                }
                else
                {
                    log.LogInformation($"Email not sent to {input.Email}");
                }
            }
            else
            {
                log.LogInformation($"Email address or game state is invalid");
            }
        }

    }
}