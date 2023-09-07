using AdventureBot.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AdventureBot.Services
{
    public interface IDiscordBotService
    {
        Task<string> RenderGameStateGameEntry(SendReceiveDiscordStateInput gameState, GameEntry entry);
        Task SendMessage(string TargetChannelId, string Body);
    }
}
