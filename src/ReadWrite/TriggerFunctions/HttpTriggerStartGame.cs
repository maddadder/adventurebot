using AdventureBot.Orchestrators;
using AdventureBot.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AdventureBot.TriggerFunctions
{
    public partial class HttpTriggerStartGame
    {
        private readonly ILogger<HttpTriggerStartGame> _logger;

        public HttpTriggerStartGame(ILogger<HttpTriggerStartGame> log)
        {
            _logger = log;
        }

        [FunctionName(Name.Get)]
        [OpenApiOperation($"{Resource.Name}-Get", tags: new[] { Resource.Name }, Summary = Summary.Get)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ResponseBody.Json, bodyType: typeof(string), Description = "The OK response")]
        [OpenApiSecurity("oidc_auth", SecuritySchemeType.OAuth2, Flows = typeof(AzureADAuth))]
        public async Task<IActionResult> Get
        (
            [HttpTrigger(AuthorizationLevel.Anonymous, Method.Get, Route = Route.Get)] HttpRequest req,
            [DurableClient] IDurableClient client,
            ILogger log)
        {
            if(!AzureADHelper.IsAuthorized(req)){
                return new UnauthorizedObjectResult("You do not have access to start the game");
            }
            var instanceId = await client.StartNewAsync(nameof(StartGameOrchestrator), null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return client.CreateCheckStatusResponse(req, instanceId);
        }
    }
}
