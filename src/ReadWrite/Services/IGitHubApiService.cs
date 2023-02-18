using DurableFunctionDemoConfig.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DurableFunctionDemoConfig.Services
{
    public interface IGitHubApiService
    {
        Task<List<string>> GetUserRepositoryList();
        Task<RepoViewCount> GetRepositoryViewCount(string repoName);
    }
}
