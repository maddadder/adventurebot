using AdventureBot.Models;
using AdventureBot.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdventureBot.ActivityFunctions
{
    public class RegisterUser
    {
        private readonly IGraphClientService _graphClientService;
        private readonly ICosmosApiService _cosmosApiService;
        
        public RegisterUser(
            IGraphClientService graphClientService,
            ICosmosApiService cosmosApiService
        )
        {
            _graphClientService = graphClientService;
            _cosmosApiService = cosmosApiService;
        }

        [FunctionName(nameof(RegisterUser))]
        public async Task Run(
            [ActivityTrigger] IDurableActivityContext context,
            ILogger log)
        {
            var graphapiClient = _graphClientService.GetAppGraphClient();
            var input = context.GetInput<UserRegistrationInput>();
            if(!string.IsNullOrEmpty(input.Email) && input.Email.Contains("@"))
            {
                var userPrefix = input.Email.Split("@")[0];
                var azureAdUserName = $"{userPrefix}@{AzureAd.TennantName}";
                
                var queryOptions = new List<QueryOption>()
                {
                    new QueryOption("$count", "true")
                };

                log.LogInformation("Attempting to search the graph api for user");
                var users = await graphapiClient.Users
                    .Request( queryOptions )
                    .Header("ConsistencyLevel","eventual")
                    .Filter($"startswith(userPrincipalName,'{azureAdUserName}')")
                    .OrderBy("userPrincipalName")
                    .GetAsync();
                
                if(users.Count == 0)
                {
                    log.LogInformation($"User account doesn't exist. Creating new user {azureAdUserName}");
                    var user = new User
                    {
                        AccountEnabled = true,
                        DisplayName = input.Name,
                        MailNickname = userPrefix,
                        UserPrincipalName = azureAdUserName,
                        PasswordProfile = new PasswordProfile
                        {
                            ForceChangePasswordNextSignIn = false,
                            Password = input.Password
                        }
                    };
                    await graphapiClient.Users
                        .Request()
                        .AddAsync(user);
                }
                await _cosmosApiService.RegisterUser(input);
            }
        }
    }
}
