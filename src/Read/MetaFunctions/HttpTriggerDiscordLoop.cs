using System;
using System.Net;
using System.Net.Http;
using AdventureBot.Models;
using Microsoft.OpenApi.Models;

namespace AdventureBot.TriggerFunctions
{
    public partial class HttpTriggerDiscordLoop
    {
        private static class Resource
        {
            public const string Name = "DiscordLoop";
        }

        private static class Summary
        {
            private const string resource = Resource.Name;
            public const string Post = $"Create one {resource}";
            public const string Put = $"Put one {resource}";
            public const string Get = $"Get one {resource}";
        }
        private static class Parameter
        {
            public const ParameterLocation In = ParameterLocation.Path;
            public const bool IsRequired = true;
            public const string instanceId = nameof(instanceId);
        }

        private static class Name
        {
            private const string prefix = nameof(InitializeDiscordLoopInput);
            public const string Post = prefix + nameof(Post);
            public const string Put = prefix + nameof(Put);
            public const string Get = prefix + nameof(Get);
        }

        private static class Route
        {
            private const string prefix = Resource.Name;
            public const string Post = prefix + "/post/";
            public const string Put = prefix + "/put/{" + Parameter.instanceId + ":required}";
            public const string Get = prefix + "/get/{" + Parameter.instanceId + ":required}";

        }

        private static class Method
        {
            public const string Put = nameof(HttpMethod.Put);
            public const string Post = nameof(HttpMethod.Post);
            public const string Get = nameof(HttpMethod.Get);
        }
        private static class ResponseBody
        {
            public const HttpStatusCode StatusCode = HttpStatusCode.OK;
            public const string Json = "application/json";
            public const string Text = "text/plain";
        }
    }
}

