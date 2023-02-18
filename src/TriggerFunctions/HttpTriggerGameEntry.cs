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
        [OpenApiParameter(name: Parameter.Name, In = Parameter.In, Type = typeof(string), Description = "The **name** parameter")]
        [OpenApiResponseWithBody(statusCode: ResponseBody.StatusCode, contentType: ResponseBody.ContentType, bodyType: typeof(GameEntry[]), Description = Description.List)]
        public async Task<IActionResult> List(
            [HttpTrigger(AuthorizationLevel.Anonymous, Method.Get, Route = Route.List)] HttpRequest req,
            string GameEntryName,
            [CosmosDB(
                databaseName: DbStrings.CosmosDBDatabaseName, 
                containerName: DbStrings.CosmosDBContainerName, 
                Connection = DbStrings.CosmosDBConnection,
                SqlQuery = "select * from gameentry ge where ge.__T = 'ge' and ge.name = {GameEntryName}")] 
                IEnumerable<GameEntry> gameEntries)
        {
            _logger.LogInformation($"name: {GameEntryName}");
            return new OkObjectResult(gameEntries);
        }

        [FunctionName(Name.Get)]
        [OpenApiOperation($"{Resource.Name}-Get", tags: new[] { Resource.Name }, Summary = Summary.Get)]
        [OpenApiParameter(name: Parameter.Id, In = Parameter.In, Required = true, Type = typeof(string), Description = "The **pid** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ResponseBody.ContentType, bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Get(
            [HttpTrigger(AuthorizationLevel.Anonymous, Method.Get, Route = Route.Get)] HttpRequest req,
            string GameEntryId,
            [CosmosDB(
                databaseName: DbStrings.CosmosDBDatabaseName, 
                containerName: DbStrings.CosmosDBContainerName, 
                Connection = DbStrings.CosmosDBConnection,
                SqlQuery = "select * from gameentry ge where ge.__T = 'ge' and ge.id = {GameEntryId}")] 
                IEnumerable<GameEntry> gameEntries)
        {
            _logger.LogInformation($"GameEntryId: {GameEntryId}");
            return new OkObjectResult(gameEntries.FirstOrDefault());
        }

        [FunctionName(Name.Post)]
        [OpenApiOperation($"{Resource.Name}-Post", tags: new[] { Resource.Name }, Summary = Summary.Post)]
        [OpenApiRequestBody(contentType: ResponseBody.ContentType, bodyType: typeof(GameEntry), Required = true, Description = "The **GameEntry** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ResponseBody.ContentType, bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Post
        (
            [HttpTrigger(AuthorizationLevel.Anonymous, Method.Post, Route = Route.Post)] HttpRequest req,
            [CosmosDB(
                databaseName: DbStrings.CosmosDBDatabaseName, 
                containerName: DbStrings.CosmosDBContainerName, 
                Connection = DbStrings.CosmosDBConnection)] 
                IAsyncCollector<dynamic> documentsOut)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            GameEntry gameEntry = JsonConvert.DeserializeObject<GameEntry>(requestBody);
            _logger.LogInformation($"GameEntryId: {gameEntry.name}");
            gameEntry.id = Guid.NewGuid();
            gameEntry.Created = DateTime.UtcNow;
            gameEntry.Modified = DateTime.UtcNow;
            gameEntry.__T = "ge";
            await documentsOut.AddAsync(gameEntry);
            return new CreatedResult($"/api/{Resource.Name}/get/{gameEntry.id}", gameEntry);
        }
        
    }
}

