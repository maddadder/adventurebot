using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AdventureBot.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace AdventureBot.TriggerFunctions
{
    public partial class HttpTriggerUserProfile
    {
        private readonly ILogger<HttpTriggerUserProfile> _logger;

        public HttpTriggerUserProfile(ILogger<HttpTriggerUserProfile> log)
        {
            _logger = log;
        }

        [FunctionName(Name.List)]
        [OpenApiOperation(operationId: $"{Resource.Name}-List", tags: new[] { Resource.Name }, Summary = Summary.List)]
        [OpenApiParameter(name: Parameter.partitionKey, In = Parameter.In, Required = true, Type = typeof(string), Description = "The **partitionKey** parameter")]
        [OpenApiParameter(name: Parameter.Name, In = Parameter.In, Required = true, Type = typeof(string), Description = "The **name** parameter")]
        [OpenApiResponseWithBody(statusCode: ResponseBody.StatusCode, contentType: ResponseBody.ContentType, bodyType: typeof(UserProfile[]), Description = Description.List)]
        public async Task<IActionResult> List(
            [HttpTrigger(AuthorizationLevel.Anonymous, Method.Get, Route = Route.List)] HttpRequest req,
            string partitionKey,
            string PreferredUsername,
            [CosmosDB(
                databaseName: DbStrings.CosmosDBDatabaseName, 
                containerName: DbStrings.CosmosDBContainerName, 
                Connection = DbStrings.CosmosDBConnection,
                SqlQuery = "select * from userprofile up where up.__T = {partitionKey} and up.preferredUsername = {PreferredUsername}")] 
                IEnumerable<UserProfile> gameEntries)
        {
            _logger.LogInformation($"List name: {PreferredUsername}");
            return new OkObjectResult(gameEntries);
        }

        [FunctionName(Name.Get)]
        [OpenApiOperation($"{Resource.Name}-Get", tags: new[] { Resource.Name }, Summary = Summary.Get)]
        [OpenApiParameter(name: Parameter.partitionKey, In = Parameter.In, Required = true, Type = typeof(string), Description = "The **partitionKey** parameter")]
        [OpenApiParameter(name: Parameter.Id, In = Parameter.In, Required = true, Type = typeof(string), Description = "The **UserProfileId** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ResponseBody.ContentType, bodyType: typeof(UserProfile), Description = "The OK response")]
        public async Task<IActionResult> Get(
            [HttpTrigger(AuthorizationLevel.Anonymous, Method.Get, Route = Route.Get)] HttpRequest req,
            string partitionKey,
            Guid UserProfileId,
            [CosmosDB(
                databaseName: DbStrings.CosmosDBDatabaseName, 
                containerName: DbStrings.CosmosDBContainerName, 
                Connection = DbStrings.CosmosDBConnection,
                Id = "{UserProfileId}",
                PartitionKey = "{partitionKey}")] UserProfile userProfile)
        {
            _logger.LogInformation($"Get UserProfileId: {UserProfileId}");
            return new OkObjectResult(userProfile);
        }

        [FunctionName(Name.Post)]
        [OpenApiOperation($"{Resource.Name}-Post", tags: new[] { Resource.Name }, Summary = Summary.Post)]
        [OpenApiParameter(name: Parameter.partitionKey, In = Parameter.In, Required = true, Type = typeof(string), Description = "The **partitionKey** parameter")]
        [OpenApiRequestBody(contentType: ResponseBody.ContentType, bodyType: typeof(UserProfile), Required = true, Description = "The **UserProfile** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Created, contentType: ResponseBody.ContentType, bodyType: typeof(UserProfile), Description = "The Created response")]
        public async Task<IActionResult> Post
        (
            [HttpTrigger(AuthorizationLevel.Anonymous, Method.Post, Route = Route.Post)] HttpRequest req,
            string partitionKey,
            [CosmosDB(
                databaseName: DbStrings.CosmosDBDatabaseName, 
                containerName: DbStrings.CosmosDBContainerName, 
                Connection = DbStrings.CosmosDBConnection)] 
                IAsyncCollector<dynamic> documentsOut)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic userProfile = JsonConvert.DeserializeObject<dynamic>(requestBody);
            
            if(userProfile.id == Guid.Empty)
            {
                userProfile.id = Guid.NewGuid();
            }
            _logger.LogInformation($"Post UserProfileId: {userProfile.id}");
            userProfile.Created = DateTime.UtcNow;
            userProfile.Modified = DateTime.UtcNow;
            userProfile.__T = partitionKey;
            await documentsOut.AddAsync(userProfile);
            return new CreatedResult($"/api/{Resource.Name}/get/{partitionKey}/{userProfile.id}", userProfile);
        }
        
        [FunctionName(Name.Put)]
        [OpenApiOperation($"{Resource.Name}-Put", tags: new[] { Resource.Name }, Summary = Summary.Put)]
        [OpenApiParameter(name: Parameter.partitionKey, In = Parameter.In, Required = true, Type = typeof(string), Description = "The **partitionKey** parameter")]
        [OpenApiParameter(name: Parameter.Id, In = Parameter.In, Required = true, Type = typeof(string), Description = "The **UserProfileId** parameter")]
        [OpenApiRequestBody(contentType: ResponseBody.ContentType, bodyType: typeof(UserProfile), Required = true, Description = "The **UserProfile** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ResponseBody.ContentType, bodyType: typeof(UserProfile), Description = "The OK response")]
        public async Task<IActionResult> Put
        (
            [HttpTrigger(AuthorizationLevel.Anonymous, Method.Put, Route = Route.Put)] HttpRequest req,
            string partitionKey,
            Guid UserProfileId,
            [CosmosDB(
                databaseName: DbStrings.CosmosDBDatabaseName, 
                containerName: DbStrings.CosmosDBContainerName, 
                Connection = DbStrings.CosmosDBConnection)] 
                CosmosClient client)
        {
            var container = client.GetContainer(DbStrings.CosmosDBDatabaseName, DbStrings.CosmosDBContainerName);

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic userProfileInput = JsonConvert.DeserializeObject(requestBody);
            userProfileInput.__T = partitionKey;
            userProfileInput.id = UserProfileId;
            userProfileInput.Modified = DateTime.UtcNow;
            _logger.LogInformation($"Put UserProfileId: {UserProfileId}");
            await container.UpsertItemAsync<dynamic>(
                item: userProfileInput,
                partitionKey: new PartitionKey(partitionKey)
            );

            return new OkObjectResult(userProfileInput);
        }

        [FunctionName(Name.Delete)]
        [OpenApiOperation($"{Resource.Name}-Delete", tags: new[] { Resource.Name }, Summary = Summary.Delete)]
        [OpenApiParameter(name: Parameter.partitionKey, In = Parameter.In, Required = true, Type = typeof(string), Description = "The **partitionKey** parameter")]
        [OpenApiParameter(name: Parameter.Id, In = Parameter.In, Required = true, Type = typeof(string), Description = "The **UserProfileId** parameter")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Description = "The OK response")]
        public async Task<IActionResult> Delete
        (
            [HttpTrigger(AuthorizationLevel.Anonymous, Method.Delete, Route = Route.Delete)] HttpRequest req,
            string partitionKey,
            Guid UserProfileId,
            [CosmosDB(
                databaseName: DbStrings.CosmosDBDatabaseName, 
                containerName: DbStrings.CosmosDBContainerName, 
                Connection = DbStrings.CosmosDBConnection)] 
                CosmosClient client)
        {
            var container = client.GetContainer(DbStrings.CosmosDBDatabaseName, DbStrings.CosmosDBContainerName);
            _logger.LogInformation($"Delete UserProfileId: {UserProfileId}");
            await container.DeleteItemAsync<dynamic>(
                id: UserProfileId.ToString(),
                partitionKey: new PartitionKey(partitionKey)
            );
            return new OkResult();
        }
    }
}

