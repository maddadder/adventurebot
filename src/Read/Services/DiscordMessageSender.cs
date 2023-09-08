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
                    // Tokenize the message into words, Markdown links, and other characters
                    var tokens = TokenizeMessage(messageText);

                    var currentChunk = new StringBuilder();
                    var currentChunkLength = 0;

                    foreach (var token in tokens)
                    {
                        if (currentChunkLength + token.Length <= 1999)
                        {
                            currentChunk.Append(token);
                            currentChunkLength += token.Length;
                        }
                        else
                        {
                            await channel.SendMessageAsync(currentChunk.ToString());
                            currentChunk.Clear();
                            currentChunk.Append(token);
                            currentChunkLength = token.Length;
                        }
                    }

                    if (currentChunkLength > 0)
                    {
                        await channel.SendMessageAsync(currentChunk.ToString());
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
    private List<string> TokenizeMessage(string messageText)
    {
        var tokens = new List<string>();
        var currentToken = new StringBuilder();

        for (int i = 0; i < messageText.Length; i++)
        {
            if (messageText[i] == '[')
            {
                // Check for Markdown link start
                var linkEndIndex = messageText.IndexOf(']', i + 1);
                if (linkEndIndex != -1)
                {
                    var linkText = messageText.Substring(i, linkEndIndex - i + 1);
                    tokens.Add(linkText);
                    i = linkEndIndex;
                    continue;
                }
            }
            
            if (messageText[i] == '(')
            {
                // Check for Markdown link end
                var linkEndIndex = messageText.IndexOf(')', i + 1);
                if (linkEndIndex != -1)
                {
                    var linkHref = messageText.Substring(i, linkEndIndex - i + 1);
                    tokens.Add(linkHref);
                    i = linkEndIndex;
                    continue;
                }
            }

            // Handle other characters and spaces
            if (char.IsWhiteSpace(messageText[i]))
            {
                // Preserve spaces and line breaks
                if (currentToken.Length > 0)
                {
                    tokens.Add(currentToken.ToString());
                    currentToken.Clear();
                }
                tokens.Add(messageText[i].ToString());
            }
            else
            {
                currentToken.Append(messageText[i]);
            }
        }

        // Add any remaining token
        if (currentToken.Length > 0)
        {
            tokens.Add(currentToken.ToString());
        }

        return tokens;
    }
    private Task LogAsync(LogMessage log)
    {
        _logger.LogInformation(log.Message);
        return Task.CompletedTask;
    }
}
