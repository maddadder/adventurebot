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

        public async Task<UserProfileGameEntry> GetGameStateFromUser(UserProfile userProfile)
        {
            UserProfileGameEntry result = new UserProfileGameEntry(){
                userProfile = userProfile
            };
            var container = cosmosClient.GetContainer(DbStrings.CosmosDBDatabaseName, DbStrings.CosmosDBContainerName);
            
            // Build query definition
            var parameterizedQuery = new QueryDefinition(
                query: "SELECT * FROM gameEntry ge WHERE ge.__T = @partitionKey and ge.name = @GameEntryState"
            )
                .WithParameter("@partitionKey", "ge")
                .WithParameter("@GameEntryState", userProfile.GameEntryState);

            // Query multiple items from container
            using FeedIterator<GameEntry> filteredFeed = container.GetItemQueryIterator<GameEntry>(
                queryDefinition: parameterizedQuery
            );

            List<GameEntry> queryResults = new List<GameEntry>();
            // Iterate query result pages
            while (filteredFeed.HasMoreResults)
            {
                FeedResponse<GameEntry> response = await filteredFeed.ReadNextAsync();

                // Iterate query results
                foreach (GameEntry item in response)
                {
                    queryResults.Add(item);
                }
            }
            result.gameEntry = queryResults.FirstOrDefault();
            return result;
        }

        public async Task<List<UserProfile>> GetActiveUserProfileList()
        {
            var container = cosmosClient.GetContainer(DbStrings.CosmosDBDatabaseName, DbStrings.CosmosDBContainerName);
            
            // Build query definition
            var parameterizedQuery = new QueryDefinition(
                query: "SELECT * FROM userProfiles up WHERE IS_DEFINED(up.email) and up.email != '' and up.receiveGameAdvanceEmail = true and up.__T = @partitionKey"
            )
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
            return results;
        }
        public async Task SetUserEmailVerification(string emailAddress)
        {
            var container = cosmosClient.GetContainer(DbStrings.CosmosDBDatabaseName, DbStrings.CosmosDBContainerName);
            
            // Build query definition
            var parameterizedQuery = new QueryDefinition(
                query: "SELECT * FROM userProfiles up WHERE up.email = @email and up.emailIsVerified = false and up.__T = @partitionKey"
            )
                .WithParameter("@email", emailAddress)
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
            foreach (UserProfile item in results)
            {
                item.EmailIsVerified = true;
                await container.UpsertItemAsync<UserProfile>(
                    item: item,
                    partitionKey: new PartitionKey("up")
                );
            }
        }
    }
}
