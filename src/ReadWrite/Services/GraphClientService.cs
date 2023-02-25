// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using AdventureBot.Models;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;

namespace AdventureBot.Services
{
    public class GraphClientService : IGraphClientService
    {
        private GraphServiceClient? _appGraphClient;
        private readonly GraphApiAppConfig _config;
        private readonly ILogger<GraphClientService> _logger;

        public GraphClientService(
            IOptions<GraphApiAppConfig> graphApiAppConfig,
            ILogger<GraphClientService> log)
        {
            _config = graphApiAppConfig.Value;
            _logger = log;
        }
        public GraphServiceClient? GetAppGraphClient()
        {
            if (_appGraphClient == null)
            {
                var tenantId = _config.TenantId;
                var clientId = _config.ClientId;
                var clientSecret = _config.ClientSecret;

                if (string.IsNullOrEmpty(tenantId) ||
                    string.IsNullOrEmpty(clientId) ||
                    string.IsNullOrEmpty(clientSecret))
                {
                    _logger.LogError("Required settings missing: 'tenantId', 'webhookClientId', and 'webhookClientSecret'.");
                    return null;
                }

                var clientSecretCredential = new ClientSecretCredential(
                    tenantId, clientId, clientSecret);

                _appGraphClient = new GraphServiceClient(clientSecretCredential);
            }

            return _appGraphClient;
        }
    }
}
