using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
    public class HttpTriggerGameEntry
    {
        private readonly ILogger<HttpTriggerGameEntry> _logger;

        public HttpTriggerGameEntry(ILogger<HttpTriggerGameEntry> log)
        {
            _logger = log;
        }

        [FunctionName(nameof(HttpTriggerGameEntry))]
        [OpenApiOperation(operationId: "Run", tags: new[] { "pid" })]
        [OpenApiParameter(name: "pid", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The **pid** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "gameentry/{pid}")] HttpRequest req,
            string pid,
            [CosmosDB(
                databaseName: Strings.CosmosDBDatabaseName, 
                containerName: Strings.CosmosDBContainerName, 
                Connection = Strings.CosmosDBConnection,
                SqlQuery = "select * from gameentry ge where ge.pid = {pid}")] 
                IEnumerable<GameEntry> gameEntries)
        {
            _logger.LogInformation($"pid: {pid}");
            return new OkObjectResult(gameEntries.FirstOrDefault());
        }
    }
}

