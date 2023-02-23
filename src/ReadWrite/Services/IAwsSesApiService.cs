using AdventureBot.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AdventureBot.Services
{
    public interface IAwsSesApiService
    {
        Task<string> RenderUserProfileGameEntry(UserProfileGameEntry userProfileGameEntry);
        Task SendEmail(string ToEmailAddress, string Subject, string Body);
    }
}
