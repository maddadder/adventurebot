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
        
        public async Task<IEnumerable<string>> RenderGameStateGameEntry(SendReceiveDiscordStateInput gameState, GameEntry gameEntry){
            
            if(gameState == null || string.IsNullOrEmpty(gameState.TargetChannelId))
            {
                return new List<string>();
            }
            List<StringBuilder> messages = new List<StringBuilder>();
            System.Text.StringBuilder message1 = new System.Text.StringBuilder();
            System.Text.StringBuilder message2 = new System.Text.StringBuilder();
            messages.Add(message1);
            messages.Add(message2);
            foreach(var desc in gameEntry.description)
            {
                var paragraphs = desc.Split("\n");
                foreach(var paragraph in paragraphs){
                    message1.Append($"{paragraph}\n\n");
                }
            }
            message1.Append($"\nOptions:\n\n");
            //[Custom Text](URL)
            if(!gameEntry.options.Any())
            {
                message1.Append($"[Start Over]({gameState.RegistrationConfirmationURL}/discordloop/{gameState.InstanceId}/{gameState.TargetChannelId}/{gameState.SubscriberId}/begin/)\n\n");
            }
            else
            {
                foreach(var option in gameEntry.options){
                    message1.Append($"[{option.description}]({gameState.RegistrationConfirmationURL}/discordloop/{gameState.InstanceId}/{gameState.TargetChannelId}/{gameState.SubscriberId}/{option.next}/)\n\n");
                }
            }
            message1.Append($@"

You received the above message because you or a party member has responded to the game within 24 hours.

To end the game, you and your party members must not respond for 24 hours and the game will end.

To get the game status, use the following command:

");
message2.Append($"/status instanceid:{gameState.InstanceId}");
            return messages.Select(x => x.ToString());
        }
        public async Task SendMessages(string TargetChannelId, IEnumerable<string> messages)
        {
            _logger.LogInformation("Creating DiscordMessageSender");
            DiscordMessageSender sender = new DiscordMessageSender(_logger, BotToken, ulong.Parse(TargetChannelId));
            _logger.LogInformation("awaiting sender.SendMessageAsync(Body);");
            await sender.SendMessageAsync(messages);
        }
    }
}