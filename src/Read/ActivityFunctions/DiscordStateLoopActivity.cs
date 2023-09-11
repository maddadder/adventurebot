using System.Text;
using System.Threading.Tasks;
using AdventureBot.Models;
using AdventureBot.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Collections.Generic;
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
        public async Task<List<GameOption>> Run(          
            [ActivityTrigger] SendReceiveDiscordStateInput input)
        {
            _logger.LogInformation("DiscordStateLoopActivity has started");
            ulong temp = 0;
            if(ulong.TryParse(input.TargetChannelId, out temp) && !string.IsNullOrEmpty(input.GameState))
            {
                _logger.LogInformation("Getting GetGameStatesFromOption from GameState");
                var gameEntries = await _cosmosApiService.GetGameStatesFromOption(input.GameState);
                var gameEntry = gameEntries.FirstOrDefault();
                _logger.LogInformation("Getting Message to send from GameState");
                var messages = await _discordBotService.RenderGameStateGameEntry(input, gameEntry);
                if(messages.Any())
                {
                    _logger.LogInformation("Sending Message using DiscordBotService");
                    await _discordBotService.SendMessages(input.TargetChannelId, messages);
                    _logger.LogInformation($"Message sent to {input.TargetChannelId} with game state URL {input.RegistrationConfirmationURL}/ with instanceid: {input.InstanceId}");
                }
                else
                {
                    _logger.LogInformation($"Message not sent to {input.TargetChannelId}");
                }
                if(gameEntry != null)
                {
                    return gameEntry.options;
                }
                else
                {
                    return new List<GameOption>();
                }
            }
            else
            {
                _logger.LogInformation($"TargetChannelId or game state is invalid");
                return new List<GameOption>();
            }
        }

    }
}