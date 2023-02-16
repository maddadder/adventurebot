using DurableFunctionDemoConfig.Orchestrators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DurableFunctionDemoConfig.TriggerFunctions
{
    public static class HttpTrigger
    {
        [FunctionName(nameof(HttpTrigger))]
        public static async Task<IActionResult> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            [DurableClient] IDurableClient client,
            ILogger log)
        {
            var instanceId = await client.StartNewAsync(nameof(DemoOrchestrator), null);
            return new OkObjectResult("");
        }
    }
}
