using AdventureBot.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AdventureBot.Services
{
    public interface ICosmosApiService
    {
        Task<List<UserProfile>> GetActiveUserProfileList();
        Task<UserProfileGameEntry> GetGameStateFromUser(UserProfile userProfile);
    }
}
