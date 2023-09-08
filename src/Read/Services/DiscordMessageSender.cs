using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

public class DiscordMessageSender
{
    private string _botToken;
    private ulong _targetChannelId;
    private readonly ILogger _logger;

    public DiscordMessageSender(ILogger logger, string botToken, ulong targetChannelId)
    {
        _logger = logger;
        _botToken = botToken;
        _targetChannelId = targetChannelId;
    }

    public async Task SendMessageAsync(string messageText)
    {
        var client = new DiscordSocketClient();
        client.Log += LogAsync;

        _logger.LogInformation("DiscordMessageSender LoginAsync");
        await client.LoginAsync(TokenType.Bot, _botToken);
        _logger.LogInformation("DiscordMessageSender StartAsync");
        await client.StartAsync();

        client.Ready += async () =>
        {
            _logger.LogInformation("DiscordMessageSender client Ready");
            var channel = client.GetChannel(_targetChannelId) as ISocketMessageChannel;

            if (channel != null)
            {
                 _logger.LogInformation("DiscordMessageSender channel.SendMessageAsync");
                // Send the custom message
                await channel.SendMessageAsync(messageText);
            }
            else
            {
                _logger.LogInformation("Target channel not found.");
            }

            _logger.LogInformation("Shutting down the bot");
            await client.StopAsync();
        };

        // Block until the bot is ready
        while (client.ConnectionState != ConnectionState.Connected)
        {
            _logger.LogInformation("client.ConnectionState != ConnectionState.Connected");
            await Task.Delay(1000);
        }
    }

    private Task LogAsync(LogMessage log)
    {
        _logger.LogInformation(log.Message);
        return Task.CompletedTask;
    }
}
