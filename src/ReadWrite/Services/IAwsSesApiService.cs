using AdventureBot.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AdventureBot.Services
{
    public interface IAwsSesApiService
    {
        Task<string> RenderRepoViewCount(RepoViewCount[] repoViewCounts);
        Task SendEmail(string Subject, string Body);
    }
}
