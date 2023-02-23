using System;
using System.Net;
using System.Net.Http;
using AdventureBot.Models;
using Microsoft.OpenApi.Models;

namespace AdventureBot.TriggerFunctions
{
    public partial class HttpTriggerStartGame
    {
        private static class Resource
        {
            public const string Name = "StartGame";
        }

        private static class Summary
        {
            private const string resource = Resource.Name;
            public const string Get = $"Retrieve one {resource}";
        }

        private static class Name
        {
            private const string prefix = Resource.Name;
            public const string Get = prefix + nameof(Get);
        }

        private static class Route
        {
            private const string prefix = Resource.Name;
            public const string Get = prefix + "/get/";

        }

        private static class Method
        {
            public const string Get = nameof(HttpMethod.Get);
        }
        
        private static class ResponseBody
        {
            public const HttpStatusCode StatusCode = HttpStatusCode.OK;
            public const string Json = "application/json";
        }
    }
}

