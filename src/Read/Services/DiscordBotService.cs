using AdventureBot.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AdventureBot.Services
{
    public class DiscordBotService : IDiscordBotService
    {
        private readonly string BaseUrl;
        private readonly string BotToken;
        private readonly ILogger _logger;
        public DiscordBotService(
            ILogger<DiscordBotService> logger,
            IOptions<DiscordConfig> discordConfig,
            IOptions<ApplicationConfig> applicationConfig)
        {
            _logger = logger;
            var discordConfigValue = discordConfig.Value;
            var applicationConfigValue = applicationConfig.Value;
            BotToken = discordConfigValue.BotToken;
            BaseUrl = applicationConfigValue.BaseUrl;
        }
        
        public async Task<string> RenderGameStateGameEntry(SendReceiveDiscordStateInput gameState, GameEntry gameEntry){
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if(gameState == null || string.IsNullOrEmpty(gameState.TargetChannelId))
            {
                return "";
            }

            foreach(var desc in gameEntry.description)
            {
                var paragraphs = desc.Split("\n");
                foreach(var paragraph in paragraphs){
                    sb.Append($"{paragraph}\n\n");
                }
            }
            sb.Append($"\nOptions:\n\n");
            //[Custom Text](URL)
            if(!gameEntry.options.Any())
            {
                sb.Append($"[Start Over]({gameState.RegistrationConfirmationURL}/discordloop/{gameState.InstanceId}/{gameState.TargetChannelId}/{gameState.SubscriberId}/begin/)\n\n");
            }
            else
            {
                foreach(var option in gameEntry.options){
                    sb.Append($"[{option.description}]({gameState.RegistrationConfirmationURL}/discordloop/{gameState.InstanceId}/{gameState.TargetChannelId}/{gameState.SubscriberId}/{option.next}/)\n\n");
                }
            }
            sb.Append($@"

You received the above message because you or a party member has responded to the game within 24 hours.

To end the game, you and your party members must not respond for 24 hours and the game will end.");
            return sb.ToString();
        }
        public async Task SendMessage(string TargetChannelId, string Body)
        {
            _logger.LogInformation("Creating DiscordMessageSender");
            DiscordMessageSender sender = new DiscordMessageSender(_logger, BotToken, ulong.Parse(TargetChannelId));
            _logger.LogInformation("awaiting sender.SendMessageAsync(Body);");
            await sender.SendMessageAsync(Body);
        }
    }
}