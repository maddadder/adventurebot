using System;
using System.Net;
using System.Net.Http;
using DurableFunctionDemoConfig.Models;
using Microsoft.OpenApi.Models;

namespace DurableFunctionDemoConfig.TriggerFunctions
{
    public partial class HttpTriggerGameEntry
    {
        private static class Resource
        {
            public const string Name = "GameEntry";
        }

        private static class Summary
        {
            private const string resource = Resource.Name;
            public const string List = $"Retrieve each {resource} by name";
            public const string Get = $"Retrieve one {resource}";
            public const string Post = $"Create one {resource}";
        }
        private static class Parameter
        {
            public const string Id = $"{Resource.Name}Id";
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
            public const string Post = prefix + nameof(Post);
        }

        private static class Route
        {
            private const string prefix = Resource.Name;
            public const string List = prefix + "/search/{"+ Parameter.Name + ":required}";
            public const string Get = prefix + "/get/{"+ Parameter.Id + ":required}";
            public const string Delete = prefix + "/delete/{"+ Parameter.Id + ":required}";
            public const string Post = prefix + "/post";
        }

        private static class Method
        {
            public const string Get = nameof(HttpMethod.Get);
            public const string Delete = nameof(HttpMethod.Delete);
            public const string Post = nameof(HttpMethod.Post);
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

