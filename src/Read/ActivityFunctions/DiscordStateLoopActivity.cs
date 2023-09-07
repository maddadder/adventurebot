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
    public class DiscordStateLoopActivity
    {
        private readonly IDiscordBotService _discordBotService;
        private readonly ICosmosApiService _cosmosApiService;

        public DiscordStateLoopActivity(
            IDiscordBotService discordBotService,
            ICosmosApiService cosmosApiService)
        {
            _discordBotService = discordBotService;
            _cosmosApiService = cosmosApiService;
        }

        [FunctionName(nameof(DiscordStateLoopActivity))]
        public async Task Run(          
            [ActivityTrigger] SendReceiveDiscordStateInput input, ILogger log)
        {
            ulong temp = 0;
            if(ulong.TryParse(input.TargetChannelId, out temp) && !string.IsNullOrEmpty(input.GameState))
            {
                var gameEntry = await _cosmosApiService.GetGameStatesFromOption(input.GameState);
                string message = await _discordBotService.RenderGameStateGameEntry(input, gameEntry.FirstOrDefault());
                if(!string.IsNullOrEmpty(message)){
                    await _discordBotService.SendMessage(input.TargetChannelId, message);
                    log.LogInformation($"Message sent to {input.TargetChannelId} with game state URL {input.RegistrationConfirmationURL}/ with instanceid: {input.InstanceId}");
                }
                else
                {
                    log.LogInformation($"Message not sent to {input.TargetChannelId}");
                }
            }
            else
            {
                log.LogInformation($"TargetChannelId or game state is invalid");
            }
        }

    }
}