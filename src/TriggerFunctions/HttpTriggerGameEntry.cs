using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DurableFunctionDemoConfig.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace DurableFunctionDemoConfig.TriggerFunctions
{
    public partial class HttpTriggerGameEntry
    {
        private readonly ILogger<HttpTriggerGameEntry> _logger;

        public HttpTriggerGameEntry(ILogger<HttpTriggerGameEntry> log)
        {
            _logger = log;
        }

        [FunctionName(Name.List)]
        [OpenApiOperation(operationId: $"{Resource.Name}-List", tags: new[] { Resource.Name }, Summary = Summary.List)]
        [OpenApiParameter(name: Parameter.partitionKey, In = Parameter.In, Required = true, Type = typeof(string), Description = "The **partitionKey** parameter")]
        [OpenApiParameter(name: Parameter.Name, In = Parameter.In, Required = true, Type = typeof(string), Description = "The **name** parameter")]
        [OpenApiResponseWithBody(statusCode: ResponseBody.StatusCode, contentType: ResponseBody.ContentType, bodyType: typeof(GameEntry[]), Description = Description.List)]
        public async Task<IActionResult> List(
            [HttpTrigger(AuthorizationLevel.Anonymous, Method.Get, Route = Route.List)] HttpRequest req,
            string partitionKey,
            string GameEntryName,
            [CosmosDB(
                databaseName: DbStrings.CosmosDBDatabaseName, 
                containerName: DbStrings.CosmosDBContainerName, 
                Connection = DbStrings.CosmosDBConnection,
                SqlQuery = "select * from gameentry ge where ge.__T = {partitionKey} and ge.name = {GameEntryName}")] 
                IEnumerable<GameEntry> gameEntries)
        {
            _logger.LogInformation($"List name: {GameEntryName}");
            return new OkObjectResult(gameEntries);
        }

        [FunctionName(Name.Get)]
        [OpenApiOperation($"{Resource.Name}-Get", tags: new[] { Resource.Name }, Summary = Summary.Get)]
        [OpenApiParameter(name: Parameter.partitionKey, In = Parameter.In, Required = true, Type = typeof(string), Description = "The **partitionKey** parameter")]
        [OpenApiParameter(name: Parameter.Id, In = Parameter.In, Required = true, Type = typeof(string), Description = "The **GameEntryId** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ResponseBody.ContentType, bodyType: typeof(GameEntry), Description = "The OK response")]
        public async Task<IActionResult> Get(
            [HttpTrigger(AuthorizationLevel.Anonymous, Method.Get, Route = Route.Get)] HttpRequest req,
            string partitionKey,
            Guid GameEntryId,
            [CosmosDB(
                databaseName: DbStrings.CosmosDBDatabaseName, 
                containerName: DbStrings.CosmosDBContainerName, 
                Connection = DbStrings.CosmosDBConnection,
                Id = "{GameEntryId}",
                PartitionKey = "{partitionKey}")] GameEntry gameEntry)
        {
            _logger.LogInformation($"Get GameEntryId: {GameEntryId}");
            return new OkObjectResult(gameEntry);
        }

        [FunctionName(Name.Post)]
        [OpenApiOperation($"{Resource.Name}-Post", tags: new[] { Resource.Name }, Summary = Summary.Post)]
        [OpenApiParameter(name: Parameter.partitionKey, In = Parameter.In, Required = true, Type = typeof(string), Description = "The **partitionKey** parameter")]
        [OpenApiRequestBody(contentType: ResponseBody.ContentType, bodyType: typeof(GameEntry), Required = true, Description = "The **GameEntry** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ResponseBody.ContentType, bodyType: typeof(GameEntry), Description = "The OK response")]
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
            dynamic gameEntry = JsonConvert.DeserializeObject<dynamic>(requestBody);
            
            if(gameEntry.id == Guid.Empty)
            {
                gameEntry.id = Guid.NewGuid();
            }
            _logger.LogInformation($"Post GameEntryId: {gameEntry.id}");
            gameEntry.Created = DateTime.UtcNow;
            gameEntry.Modified = DateTime.UtcNow;
            gameEntry.__T = partitionKey;
            await documentsOut.AddAsync(gameEntry);
            return new CreatedResult($"/api/{Resource.Name}/get/{partitionKey}/{gameEntry.id}", gameEntry);
        }
        
        [FunctionName(Name.Put)]
        [OpenApiOperation($"{Resource.Name}-Put", tags: new[] { Resource.Name }, Summary = Summary.Put)]
        [OpenApiParameter(name: Parameter.partitionKey, In = Parameter.In, Required = true, Type = typeof(string), Description = "The **partitionKey** parameter")]
        [OpenApiParameter(name: Parameter.Id, In = Parameter.In, Required = true, Type = typeof(string), Description = "The **GameEntryId** parameter")]
        [OpenApiRequestBody(contentType: ResponseBody.ContentType, bodyType: typeof(GameEntry), Required = true, Description = "The **GameEntry** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ResponseBody.ContentType, bodyType: typeof(GameEntry), Description = "The OK response")]
        public async Task<IActionResult> Put
        (
            [HttpTrigger(AuthorizationLevel.Anonymous, Method.Put, Route = Route.Put)] HttpRequest req,
            string partitionKey,
            Guid GameEntryId,
            [CosmosDB(
                databaseName: DbStrings.CosmosDBDatabaseName, 
                containerName: DbStrings.CosmosDBContainerName, 
                Connection = DbStrings.CosmosDBConnection)] 
                CosmosClient client)
        {
            var container = client.GetContainer(DbStrings.CosmosDBDatabaseName, DbStrings.CosmosDBContainerName);

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic gameEntryInput = JsonConvert.DeserializeObject(requestBody);
            gameEntryInput.__T = partitionKey;
            gameEntryInput.id = GameEntryId;
            gameEntryInput.Modified = DateTime.UtcNow;
            _logger.LogInformation($"Put GameEntryId: {GameEntryId}");
            await container.UpsertItemAsync<dynamic>(
                item: gameEntryInput,
                partitionKey: new PartitionKey(partitionKey)
            );

            return new OkObjectResult(gameEntryInput);
        }

        [FunctionName(Name.Delete)]
        [OpenApiOperation($"{Resource.Name}-Delete", tags: new[] { Resource.Name }, Summary = Summary.Delete)]
        [OpenApiParameter(name: Parameter.partitionKey, In = Parameter.In, Required = true, Type = typeof(string), Description = "The **partitionKey** parameter")]
        [OpenApiParameter(name: Parameter.Id, In = Parameter.In, Required = true, Type = typeof(string), Description = "The **GameEntryId** parameter")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Description = "The OK response")]
        public async Task<IActionResult> Delete
        (
            [HttpTrigger(AuthorizationLevel.Anonymous, Method.Delete, Route = Route.Delete)] HttpRequest req,
            string partitionKey,
            Guid GameEntryId,
            [CosmosDB(
                databaseName: DbStrings.CosmosDBDatabaseName, 
                containerName: DbStrings.CosmosDBContainerName, 
                Connection = DbStrings.CosmosDBConnection)] 
                CosmosClient client)
        {
            var container = client.GetContainer(DbStrings.CosmosDBDatabaseName, DbStrings.CosmosDBContainerName);
            _logger.LogInformation($"Delete GameEntryId: {GameEntryId}");
            await container.DeleteItemAsync<dynamic>(
                id: GameEntryId.ToString(),
                partitionKey: new PartitionKey(partitionKey)
            );
            return new OkResult();
        }
    }
}

