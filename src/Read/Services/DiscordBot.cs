using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

public class DiscordBot
{
    private DiscordSocketClient _client;
    public async Task RunBotAsync(string _botToken, ulong _targetChannelId, string messageToSend)
    {

        if (string.IsNullOrEmpty(_botToken))
        {
            Console.WriteLine("Bot token is missing or invalid.");
            return;
        }

        _client = new DiscordSocketClient();
        _client.Log += LogAsync;
        _client.Ready += async () => await ReadyAsync(_targetChannelId, messageToSend); // Pass messageToSend to ReadyAsync

        await _client.LoginAsync(TokenType.Bot, _botToken);
        await _client.StartAsync();

        // Block the program until it is closed.
        await Task.Delay(-1);
    }


    public async Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log);
    }

    public async Task ReadyAsync(ulong _targetChannelId, string messageToSend)
    {
        Console.WriteLine("Bot is connected and ready.");

        if (_targetChannelId == 0)
        {
            Console.WriteLine("Target channel ID is missing or invalid.");
            await _client.StopAsync();
            return;
        }

        var channel = _client.GetChannel(_targetChannelId) as ISocketMessageChannel;

        if (channel != null)
        {
            // Send the custom message
            await channel.SendMessageAsync(messageToSend);
        }
        else
        {
            Console.WriteLine("Target channel not found.");
        }

        // Shut down the bot
        await _client.StopAsync();
    }

}