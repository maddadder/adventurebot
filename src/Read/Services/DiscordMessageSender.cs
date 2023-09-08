using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

public class DiscordMessageSender
{
    private string _botToken;
    private ulong _targetChannelId;

    public DiscordMessageSender(string botToken, ulong targetChannelId)
    {
        _botToken = botToken;
        _targetChannelId = targetChannelId;
    }

    public async Task SendMessageAsync(string messageText)
    {
        var client = new DiscordSocketClient();
        client.Log += LogAsync;

        await client.LoginAsync(TokenType.Bot, _botToken);
        await client.StartAsync();

        client.Ready += async () =>
        {
            var channel = client.GetChannel(_targetChannelId) as ISocketMessageChannel;

            if (channel != null)
            {
                // Send the custom message
                await channel.SendMessageAsync(messageText);
            }
            else
            {
                Console.WriteLine("Target channel not found.");
            }

            // Shut down the bot
            await client.StopAsync();
        };

        // Block until the bot is ready
        while (client.ConnectionState != ConnectionState.Connected)
        {
            await Task.Delay(1000);
        }
    }

    private Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log);
        return Task.CompletedTask;
    }
}
