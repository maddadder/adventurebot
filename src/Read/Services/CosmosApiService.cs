using AdventureBot.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
namespace AdventureBot.Services
{
    public class CosmosApiService : ICosmosApiService
    {
        private readonly CosmosClient cosmosClient;

        public CosmosApiService(CosmosClient cosmosClient)
        {
            this.cosmosClient = cosmosClient;
        }

        public async Task RegisterUser(UserRegistrationInput input)
        {
            var container = cosmosClient.GetContainer(DbStrings.CosmosDBDatabaseName, DbStrings.CosmosDBContainerName);
            if(!string.IsNullOrEmpty(input.Email) && input.Email.Contains("@"))
            {
                var userPrefix = input.Email.Split("@")[0];
                var azureAdUserName = $"{userPrefix}@{AzureAd.TennantName}";
                
                // Build query definition
                var parameterizedQuery = new QueryDefinition(
                    query: "SELECT * FROM userProfiles up WHERE up.preferredUsername = @preferredUsername and up.__T = @partitionKey"
                )
                    .WithParameter("@preferredUsername", azureAdUserName)
                    .WithParameter("@partitionKey", "up");

                // Query multiple items from container
                using FeedIterator<UserProfile> filteredFeed = container.GetItemQueryIterator<UserProfile>(
                    queryDefinition: parameterizedQuery
                );

                List<UserProfile> results = new List<UserProfile>();
                // Iterate query result pages
                while (filteredFeed.HasMoreResults)
                {
                    FeedResponse<UserProfile> response = await filteredFeed.ReadNextAsync();

                    // Iterate query results
                    foreach (UserProfile item in response)
                    {
                        results.Add(item);
                    }
                }
                if(!results.Any())
                {
                    var names = input.Name.Split(" ");
                    var firstName = names.Count() >= 2 ? names[0] : input.Name;
                    var lastname = names.Count() >= 2 ? names[1] : input.Name;
                
                    var newUser = new UserProfile(){
                        __T = "up",
                        Created = DateTime.UtcNow,
                        Modified = DateTime.UtcNow,
                        Email = input.Email,
                        EmailIsVerified = true,
                        FirstName = firstName,
                        LastName = lastname,
                        GameEntryState = "begin",
                        id = Guid.NewGuid(),
                        PreferredUsername = azureAdUserName,
                        ReceiveGameAdvanceEmail = true,
                        CreatedBy = azureAdUserName,
                        ModifiedBy = azureAdUserName
                    };

                    await container.UpsertItemAsync<UserProfile>(
                            item: newUser,
                            partitionKey: new PartitionKey("up")
                        );
                }
            }
        }
    }
}
