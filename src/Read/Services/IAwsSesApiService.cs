using AdventureBot.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AdventureBot.Services
{
    public interface IAwsSesApiService
    {
        Task<string> RenderGameStateGameEntry(SendReceiveGameStateInput gameState, GameEntry entry);
        Task SendEmail(string ToEmailAddress, string Subject, string Body);
    }
}
