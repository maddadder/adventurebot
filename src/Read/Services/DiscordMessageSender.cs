using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
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
        int attempts = 0;
        int maxAttempts = 10;
        client.Ready += async () =>
        {
            try
            {
                _logger.LogInformation("DiscordMessageSender client Ready");
                var channel = client.GetChannel(_targetChannelId) as ISocketMessageChannel;

                if (channel != null)
                {
                    _logger.LogInformation("DiscordMessageSender channel.SendMessageAsync");
                    // Split the message into words and Markdown links
                    var wordsAndLinks = Regex.Split(messageText, @"(\[.*?\]\(.*?\))|\s+");
                    var chunks = new List<string>();
                    var currentChunk = new StringBuilder();

                    foreach (var wordOrLink in wordsAndLinks)
                    {
                        if (currentChunk.Length + wordOrLink.Length <= 1999)
                        {
                            currentChunk.Append(wordOrLink);
                        }
                        else
                        {
                            chunks.Add(currentChunk.ToString());
                            currentChunk.Clear();
                            currentChunk.Append(wordOrLink);
                        }
                    }

                    if (currentChunk.Length > 0)
                    {
                        chunks.Add(currentChunk.ToString());
                    }

                    foreach (var chunk in chunks)
                    {
                        await channel.SendMessageAsync(chunk);
                    }
                }
                else
                {
                    _logger.LogInformation("Target channel not found.");
                }

                _logger.LogInformation("Shutting down the bot");
                await client.StopAsync();
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Error in Ready event handler: {ex.Message}");
            }
        };

        // Check connection state with a limited number of attempts
        while (attempts < maxAttempts && client.ConnectionState != ConnectionState.Connected)
        {
            _logger.LogInformation("client.ConnectionState != ConnectionState.Connected");
            await Task.Delay(1000);
            attempts++;
        }

        if (attempts >= maxAttempts)
        {
            _logger.LogInformation("Bot could not connect to Discord within the specified number of attempts.");
        }
    }

    private Task LogAsync(LogMessage log)
    {
        _logger.LogInformation(log.Message);
        return Task.CompletedTask;
    }
}
