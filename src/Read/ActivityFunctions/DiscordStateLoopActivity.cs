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
        private readonly ILogger _logger;
        public DiscordStateLoopActivity(
            ILogger<DiscordStateLoopActivity> logger,
            IDiscordBotService discordBotService,
            ICosmosApiService cosmosApiService)
        {
            _logger = logger;
            _discordBotService = discordBotService;
            _cosmosApiService = cosmosApiService;
        }

        [FunctionName(nameof(DiscordStateLoopActivity))]
        public async Task Run(          
            [ActivityTrigger] SendReceiveDiscordStateInput input)
        {
            _logger.LogInformation("DiscordStateLoopActivity has started");
            ulong temp = 0;
            if(ulong.TryParse(input.TargetChannelId, out temp) && !string.IsNullOrEmpty(input.GameState))
            {
                _logger.LogInformation("Getting GetGameStatesFromOption from GameState");
                var gameEntry = await _cosmosApiService.GetGameStatesFromOption(input.GameState);
                _logger.LogInformation("Getting Message to send from GameState");
                string message = await _discordBotService.RenderGameStateGameEntry(input, gameEntry.FirstOrDefault());
                if(!string.IsNullOrEmpty(message)){
                    _logger.LogInformation("Sending Message using DiscordBotService");
                    await _discordBotService.SendMessage(input.TargetChannelId, message);
                    _logger.LogInformation($"Message sent to {input.TargetChannelId} with game state URL {input.RegistrationConfirmationURL}/ with instanceid: {input.InstanceId}");
                }
                else
                {
                    _logger.LogInformation($"Message not sent to {input.TargetChannelId}");
                }
            }
            else
            {
                _logger.LogInformation($"TargetChannelId or game state is invalid");
            }
        }

    }
}