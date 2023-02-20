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
    public partial class HttpTriggerGameEntry
    {
        private readonly ILogger<HttpTriggerGameEntry> _logger;

        public HttpTriggerGameEntry(ILogger<HttpTriggerGameEntry> log)
        {
            _logger = log;
        }

        [FunctionName(Name.Search)]
        [OpenApiOperation(operationId: $"{Resource.Name}-Search", tags: new[] { Resource.Name }, Summary = Summary.Search)]
        [OpenApiParameter(name: Parameter.partitionKey, In = Parameter.In, Required = true, Type = typeof(string), Description = "The **partitionKey** parameter")]
        [OpenApiParameter(name: Parameter.Name, In = Parameter.In, Required = true, Type = typeof(string), Description = "The **name** parameter")]
        [OpenApiResponseWithBody(statusCode: ResponseBody.StatusCode, contentType: ResponseBody.ContentType, bodyType: typeof(GameEntry[]), Description = Description.Search)]
        public async Task<IActionResult> Search(
            [HttpTrigger(AuthorizationLevel.Anonymous, Method.Get, Route = Route.Search)] HttpRequest req,
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

    }
}

