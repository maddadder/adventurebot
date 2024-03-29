using AdventureBot.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AdventureBot.Services
{
    public interface IDiscordBotService
    {
        Task<IEnumerable<string>> RenderGameStateGameEntry(SendReceiveDiscordStateInput gameState, GameEntry entry);
        Task SendMessages(string TargetChannelId, IEnumerable<string> messages);
    }
}
