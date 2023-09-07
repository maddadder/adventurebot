using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AdventureBot.Models;
using AdventureBot.Orchestrators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace AdventureBot.TriggerFunctions
{
    public partial class HttpTriggerDiscordLoop
    {
        private readonly ILogger<HttpTriggerDiscordLoop> _logger;
        private readonly string BaseUrl;

        public HttpTriggerDiscordLoop(
            IOptions<ApplicationConfig> applicationConfig,
            ILogger<HttpTriggerDiscordLoop> log)
        {
            var applicationConfigValue = applicationConfig.Value;
            BaseUrl = applicationConfigValue.BaseUrl;
            _logger = log;
        }
        
        [FunctionName(Name.Post)]
        [OpenApiOperation($"{Resource.Name}-Post", tags: new[] { Resource.Name }, Summary = Summary.Post)]
        [OpenApiRequestBody(contentType: ResponseBody.Json, bodyType: typeof(InitializeDiscordLoopInput), Required = true, Description = "The **InitializeDiscordLoopInput** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Accepted, contentType: ResponseBody.Json, bodyType: typeof(CheckStatusResponse), Description = "A Check Status Response")]
        public async Task<HttpResponseMessage> Post
        (
            [HttpTrigger(AuthorizationLevel.Anonymous, Method.Post, Route = Route.Post)] HttpRequestMessage req,
            [DurableClient] IDurableClient starter)
        {
            try
            {
                var input = JsonConvert.DeserializeObject<InitializeDiscordLoopInput>(await req.Content.ReadAsStringAsync());
                if (input == null ||
                    string.IsNullOrEmpty(input.TargetChannelId) ||
                    string.IsNullOrEmpty(input.InitialGameState)
                    )
                {
                    return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest) { Content = new StringContent("Invalid request payload") };
                }
                ulong temp = 0;
                if(!ulong.TryParse(input.TargetChannelId, out temp))
                {
                    return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest) { Content = new StringContent("Invalid TargetChannelId") };
                }
                
                input.BaseUri = BaseUrl;
                input.InstanceId = $"{Guid.NewGuid()}";

                // Instance Id will be <email address>-<current ticks>
                await starter.StartNewAsync(nameof(DiscordLoopOrchestration), input.InstanceId, input);

                _logger.LogInformation($"Started orchestration with ID = '{input.InstanceId}'.");

                return starter.CreateCheckStatusResponse(req, input.InstanceId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error starting registration process", ex);
                return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError) { Content = new StringContent("Internal error") };
            }
        }
        

        [FunctionName(Name.Put)]
        [OpenApiOperation($"{Resource.Name}-Put", tags: new[] { Resource.Name }, Summary = Summary.Put)]
        [OpenApiParameter(name: Parameter.instanceId, In = Parameter.In, Required = true, Type = typeof(string), Description = "The **instanceId** parameter")]
        [OpenApiRequestBody(contentType: ResponseBody.Json, bodyType: typeof(DiscordLoopInput), Required = true, Description = "The **DiscordLoopInput** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ResponseBody.Text, bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Put(
            [HttpTrigger(AuthorizationLevel.Anonymous, Method.Put, Route = Route.Put)] HttpRequest req,
            [DurableClient] IDurableClient client, 
            string instanceId)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            DiscordLoopInput input = JsonConvert.DeserializeObject<DiscordLoopInput>(requestBody);
            if (input == null ||
                string.IsNullOrEmpty(input.GameState) ||
                string.IsNullOrEmpty(input.TargetChannelId)
                )
            {
                return new BadRequestObjectResult("Invalid request payload");
            }
            ulong temp = 0;
            if(!ulong.TryParse(input.TargetChannelId, out temp))
            {
                return new BadRequestObjectResult("Invalid TargetChannelId");
            }
            var instanceStatus = await client.GetStatusAsync(instanceId);
            if (instanceStatus?.RuntimeStatus == OrchestrationRuntimeStatus.Running)
            {
                if (instanceStatus.CustomStatus != null)
                {
                    var status = instanceStatus.CustomStatus.ToObject<DiscordLoopOrchestatorStatus>();
                    if (status.ExpireAt.HasValue &&
                        status.ExpireAt.Value < DateTime.UtcNow)
                    {
                        return new OkObjectResult($"game loop expired {DateTime.UtcNow.Subtract(status.ExpireAt.Value).TotalSeconds} seconds ago");
                    }
                }

                await client.RaiseEventAsync(instanceId, EventNames.GameStateAdvanced, input);

                return new OkObjectResult("A new game state has been received");
            }
            else
            {
                return new OkObjectResult("game state is no longer valid");
            }
        }

        [FunctionName(Name.Get)]
        [OpenApiOperation($"{Resource.Name}-Get", tags: new[] { Resource.Name }, Summary = Summary.Get)]
        [OpenApiParameter(name: Parameter.instanceId, In = Parameter.In, Required = true, Type = typeof(string), Description = "The **instanceId** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ResponseBody.Json, bodyType: typeof(DiscordVotingCounter), Description = "The OK response")]
        public async Task<IActionResult> Get(
            [HttpTrigger(AuthorizationLevel.Anonymous, Method.Get, Route = Route.Get)] HttpRequest req,
            [DurableClient] IDurableClient client, 
            string instanceId)
        {
            var input = instanceId;
            var GetOrchestatorStatusInstanceId = Guid.NewGuid().ToString();
            await client.StartNewAsync(nameof(GetDiscordLoopOrchestrationStatus), GetOrchestatorStatusInstanceId, input);
            await client.WaitForCompletionOrCreateCheckStatusResponseAsync(req, GetOrchestatorStatusInstanceId, System.TimeSpan.FromMinutes(10));
            var data = await client.GetStatusAsync(GetOrchestatorStatusInstanceId);
 
            // timeout
            if(data.RuntimeStatus != OrchestrationRuntimeStatus.Completed)
            {
                await client.TerminateAsync(GetOrchestatorStatusInstanceId, "Timeout something took too long");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
            var output = data.Output.ToObject<DiscordVotingCounter>();
 
            return new OkObjectResult(output);
        }
    }
}

