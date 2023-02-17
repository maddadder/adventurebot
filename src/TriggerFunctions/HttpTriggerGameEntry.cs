using System;
using System.Collections.Generic;
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

namespace DurableFunctionDemoConfig.TriggerFunctions
{
    public class HttpTriggerGameEntry
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
                SqlQuery = "select * from gameentry ge where ge.name = {GameEntryName}")] 
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
            string GameEntryPid,
            [CosmosDB(
                databaseName: DbStrings.CosmosDBDatabaseName, 
                containerName: DbStrings.CosmosDBContainerName, 
                Connection = DbStrings.CosmosDBConnection,
                SqlQuery = "select * from gameentry ge where ge.pid = {GameEntryPid}")] 
                IEnumerable<GameEntry> gameEntries)
        {
            _logger.LogInformation($"GameEntryPid: {GameEntryPid}");
            return new OkObjectResult(gameEntries.FirstOrDefault());
        }
        
        private static class Resource
        {
            public const string Name = "GameEntry";
        }

        private static class Summary
        {
            private const string resource = Resource.Name;
            public const string List = $"Retrieve each {resource} by name";
            public const string Get = $"Retrieve one {resource}";
        }
        private static class Parameter
        {
            public const string Id = $"{Resource.Name}Pid";
            public const string Name = $"{Resource.Name}Name";
            public const ParameterLocation In = ParameterLocation.Path;
            public const bool IsRequired = true;
        }

        private static class Name
        {
            private const string prefix = nameof(GameEntry);
            public const string List = prefix + nameof(List);
            public const string Delete = prefix + nameof(Delete);
            public const string Get = prefix + nameof(Get);
        }

        private static class Route
        {
            private const string prefix = Resource.Name;
            public const string List = prefix + "/search/{"+ Parameter.Name + ":required}";
            public const string Get = prefix + "/get/{"+ Parameter.Id + ":required}";
            public const string Delete = prefix + "/delete/{"+ Parameter.Id + ":required}";
        }

        private static class Method
        {
            public const string Get = nameof(HttpMethod.Get);
            public const string Delete = nameof(HttpMethod.Delete);
        }
        private static class ResponseBody
        {
            public const HttpStatusCode StatusCode = HttpStatusCode.OK;
            public const string ContentType = "application/json";
        }

        private static class Description
        {
            private const string resource = Resource.Name;
            public const string List = $"Retrieve each {resource} by name";
            public const string Get = $"Retrieve one {resource}";
        }
    }
}

