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
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace AdventureBot.TriggerFunctions
{
    public partial class HttpTriggerUserRegistration
    {
        private readonly ILogger<HttpTriggerUserRegistration> _logger;

        public HttpTriggerUserRegistration(ILogger<HttpTriggerUserRegistration> log)
        {
            _logger = log;
        }
        
        [FunctionName(Name.Post)]
        [OpenApiOperation($"{Resource.Name}-Post", tags: new[] { Resource.Name }, Summary = Summary.Post)]
        [OpenApiRequestBody(contentType: ResponseBody.Json, bodyType: typeof(UserRegistrationInput), Required = true, Description = "The **UserRegistrationInput** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Accepted, contentType: ResponseBody.Json, bodyType: typeof(CheckStatusResponse), Description = "A Check Status Response")]
        public async Task<HttpResponseMessage> Post
        (
            [HttpTrigger(AuthorizationLevel.Anonymous, Method.Post, Route = Route.Post)] HttpRequestMessage req,
            [DurableClient] IDurableClient starter)
        {
            try
            {
                var input = JsonConvert.DeserializeObject<UserRegistrationInput>(await req.Content.ReadAsStringAsync());
                if (input == null ||
                    string.IsNullOrEmpty(input.Name) ||
                    string.IsNullOrEmpty(input.Email)
                    )
                {
                    return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest) { Content = new StringContent("Invalid request payload") };
                }

                var baseUri = new StringBuilder()
                    .Append(req.RequestUri.Scheme)
                    .Append("://")
                    .Append(req.RequestUri.Host);

                if (!req.RequestUri.IsDefaultPort)
                    baseUri.Append(':').Append(req.RequestUri.Port);
                baseUri.Append("/api");

                input.BaseUri = baseUri.ToString();
                input.InstanceId = $"{input.Email}-{DateTime.UtcNow.Ticks}";

                // Instance Id will be <email address>-<current ticks>
                await starter.StartNewAsync(nameof(UserRegistrationOrchestration), input.InstanceId, input);

                _logger.LogInformation($"Started orchestration with ID = '{input.InstanceId}'.");

                return starter.CreateCheckStatusResponse(req, input.InstanceId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error starting registration process", ex);
                return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError) { Content = new StringContent("Internal error") };
            }
        }
        

        [FunctionName(Name.Get)]
        [OpenApiOperation($"{Resource.Name}-Get", tags: new[] { Resource.Name }, Summary = Summary.Get)]
        [OpenApiParameter(name: Parameter.instanceId, In = Parameter.In, Required = true, Type = typeof(string), Description = "The **instanceId** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ResponseBody.Json, bodyType: typeof(UserProfile), Description = "The OK response")]
        public async Task<IActionResult> Get(
            [HttpTrigger(AuthorizationLevel.Anonymous, Method.Get, Route = Route.Get)] HttpRequest req,
            [DurableClient] IDurableClient client, 
            string instanceId)
        {
            var instanceStatus = await client.GetStatusAsync(instanceId);
            if (instanceStatus?.RuntimeStatus == OrchestrationRuntimeStatus.Running)
            {
                if (instanceStatus.CustomStatus != null)
                {
                    var status = instanceStatus.CustomStatus.ToObject<UserRegistrationOrchestatorStatus>();
                    if (status.ExpireAt.HasValue &&
                        status.ExpireAt.Value < DateTime.UtcNow)
                    {
                        return new OkObjectResult($"Registration confirmation expired {DateTime.UtcNow.Subtract(status.ExpireAt.Value).TotalSeconds} seconds ago");
                    }
                }

                await client.RaiseEventAsync(instanceId, "EmailConfirmationReceived", true);

                return new OkObjectResult("Your user registration has been received");
            }
            else
            {
                return new OkObjectResult("Registration confirmation is not valid");
            }
        }

        
    }
}

