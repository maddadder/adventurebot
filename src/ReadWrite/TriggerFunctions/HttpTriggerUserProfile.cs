using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using AdventureBot.Models;
using AdventureBot.Services;
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
        [FunctionName(Name.Search)]
        [OpenApiOperation(operationId: $"{Resource.Name}-Search", tags: new[] { Resource.Name }, Summary = Summary.Search)]
        [OpenApiParameter(name: Parameter.partitionKey, In = Parameter.In, Required = true, Type = typeof(string), Description = "The **partitionKey** parameter")]
        [OpenApiParameter(name: Parameter.PreferredUsername, In = Parameter.In, Required = true, Type = typeof(string), Description = "The **PreferredUsername** parameter")]
        [OpenApiResponseWithBody(statusCode: ResponseBody.StatusCode, contentType: ResponseBody.Json, bodyType: typeof(UserProfile[]), Description = Description.Search)]
        public async Task<IActionResult> Search(
            [HttpTrigger(AuthorizationLevel.Anonymous, Method.Get, Route = Route.Search)] HttpRequest req,
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

        [FunctionName(Name.List)]
        [OpenApiOperation(operationId: $"{Resource.Name}-List", tags: new[] { Resource.Name }, Summary = Summary.List)]
        [OpenApiParameter(name: Parameter.partitionKey, In = Parameter.In, Required = true, Type = typeof(string), Description = "The **partitionKey** parameter")]
        [OpenApiResponseWithBody(statusCode: ResponseBody.StatusCode, contentType: ResponseBody.Json, bodyType: typeof(UserProfile[]), Description = Description.List)]
        public async Task<IActionResult> List(
            [HttpTrigger(AuthorizationLevel.Anonymous, Method.Get, Route = Route.List)] HttpRequest req,
            string partitionKey,
            [CosmosDB(
                databaseName: DbStrings.CosmosDBDatabaseName, 
                containerName: DbStrings.CosmosDBContainerName, 
                Connection = DbStrings.CosmosDBConnection,
                SqlQuery = "select * from userprofile up where up.__T = {partitionKey}")] 
                IEnumerable<UserProfile> gameEntries)
        {
            _logger.LogInformation($"List __T: {partitionKey}");
            return new OkObjectResult(gameEntries);
        }

        [FunctionName(Name.Get)]
        [OpenApiOperation($"{Resource.Name}-Get", tags: new[] { Resource.Name }, Summary = Summary.Get)]
        [OpenApiParameter(name: Parameter.partitionKey, In = Parameter.In, Required = true, Type = typeof(string), Description = "The **partitionKey** parameter")]
        [OpenApiParameter(name: Parameter.Id, In = Parameter.In, Required = true, Type = typeof(string), Description = "The **UserProfileId** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ResponseBody.Json, bodyType: typeof(UserProfile), Description = "The OK response")]
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
        [OpenApiRequestBody(contentType: ResponseBody.Json, bodyType: typeof(UserProfile), Required = true, Description = "The **UserProfile** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Created, contentType: ResponseBody.Json, bodyType: typeof(UserProfile), Description = "The Created response")]
        [OpenApiSecurity("oidc_auth", SecuritySchemeType.OAuth2, Flows = typeof(AzureADAuth))]
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
            var unique_name = AzureADHelper.GetUserName(req);
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            UserProfile userProfile = JsonConvert.DeserializeObject<UserProfile>(requestBody);
            if(!AzureADHelper.IsAuthorized(req))
            {
                if(unique_name != userProfile.PreferredUsername)
                    return new UnauthorizedObjectResult(Security.UnauthorizedAccessException);
            }
            if(userProfile.id == Guid.Empty)
            {
                userProfile.id = Guid.NewGuid();
            }
            _logger.LogInformation($"Post UserProfileId: {userProfile.id}");
            userProfile.Created = DateTime.UtcNow;
            userProfile.Modified = DateTime.UtcNow;
            userProfile.__T = partitionKey;
            userProfile.CreatedBy = unique_name;
            userProfile.ModifiedBy = unique_name;
            await documentsOut.AddAsync(userProfile);
            return new CreatedResult($"/api/{Resource.Name}/get/{partitionKey}/{userProfile.id}", userProfile);
        }
        
        [FunctionName(Name.Put)]
        [OpenApiOperation($"{Resource.Name}-Put", tags: new[] { Resource.Name }, Summary = Summary.Put)]
        [OpenApiParameter(name: Parameter.partitionKey, In = Parameter.In, Required = true, Type = typeof(string), Description = "The **partitionKey** parameter")]
        [OpenApiParameter(name: Parameter.Id, In = Parameter.In, Required = true, Type = typeof(string), Description = "The **UserProfileId** parameter")]
        [OpenApiRequestBody(contentType: ResponseBody.Json, bodyType: typeof(UserProfile), Required = true, Description = "The **UserProfile** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ResponseBody.Json, bodyType: typeof(UserProfile), Description = "The OK response")]
        [OpenApiSecurity("oidc_auth", SecuritySchemeType.OAuth2, Flows = typeof(AzureADAuth))]
        public async Task<IActionResult> Put
        (
            [HttpTrigger(AuthorizationLevel.Anonymous, Method.Put, Route = Route.Put)] HttpRequest req,
            string partitionKey,
            Guid UserProfileId,
            [CosmosDB(
                databaseName: DbStrings.CosmosDBDatabaseName, 
                containerName: DbStrings.CosmosDBContainerName, 
                Connection = DbStrings.CosmosDBConnection)] 
                CosmosClient cosmosClient)
        {
            if(!AzureADHelper.IsAuthorized(req)){
                return new UnauthorizedObjectResult(Security.UnauthorizedAccessException);
            }
            var unique_name = AzureADHelper.GetUserName(req);
            var container = cosmosClient.GetContainer(DbStrings.CosmosDBDatabaseName, DbStrings.CosmosDBContainerName);

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic userProfileInput = JsonConvert.DeserializeObject<UserProfile>(requestBody);
            userProfileInput.__T = partitionKey;
            userProfileInput.id = UserProfileId;
            userProfileInput.Modified = DateTime.UtcNow;
            userProfileInput.ModifiedBy = unique_name;
            _logger.LogInformation($"Put UserProfileId: {UserProfileId}");
            await container.UpsertItemAsync<UserProfile>(
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
        [OpenApiSecurity("oidc_auth", SecuritySchemeType.OAuth2, Flows = typeof(AzureADAuth))]
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
            if(!AzureADHelper.IsAuthorized(req)){
                return new UnauthorizedObjectResult(Security.UnauthorizedAccessException);
            }
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

