using System;
using System.Text;
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
                    var words = messageText.Split(' '); // Split the message into words

                    // Initialize a StringBuilder for each chunk
                    var chunkBuilder = new StringBuilder();

                    foreach (var word in words)
                    {
                        if (chunkBuilder.Length + word.Length + 1 <= 1500)
                        {
                            // If adding the word doesn't exceed the character limit, add it to the current chunk
                            if (chunkBuilder.Length > 0)
                            {
                                chunkBuilder.Append(' '); // Add a space between words
                            }
                            chunkBuilder.Append(word);
                        }
                        else
                        {
                            // If adding the word exceeds the character limit, send the current chunk
                            await channel.SendMessageAsync(chunkBuilder.ToString());

                            // Reset the chunkBuilder for the next chunk
                            chunkBuilder.Clear();
                            chunkBuilder.Append(word);
                        }
                    }

                    // Send any remaining part of the message
                    if (chunkBuilder.Length > 0)
                    {
                        await channel.SendMessageAsync(chunkBuilder.ToString());
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
