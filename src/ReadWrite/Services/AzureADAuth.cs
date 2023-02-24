using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using AdventureBot.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.OpenApi.Models;

namespace AdventureBot.Services
{
    public class AzureADAuth : OpenApiOAuthSecurityFlows
    {
        public AzureADAuth()
        {
            this.Implicit = new OpenApiOAuthFlow()
            {
                AuthorizationUrl = new Uri($"https://login.microsoftonline.com/{AzureAd.TenantId}/oauth2/v2.0/authorize"),
                Scopes = { { $"api://{AzureAd.ClientId}/user_impersonation", "impersonate user" } }
            };
        }
    }

    public static class AzureADHelper
    {
        public static readonly List<string> GameManageRole = new List<string>(){
            "game.manage"
        };
        internal static bool IsAuthorized(HttpRequest req)
        {
            var headers = req.Headers.ToDictionary(p => p.Key, p => (string)p.Value);
            var handler = new JwtSecurityTokenHandler();
            if(!headers.ContainsKey("Authorization"))
            {
                return false;
            }
            string tokenString = headers["Authorization"].Split(' ').Last();
            var token = handler.ReadJwtToken(tokenString);
            var roles = token.Claims.Where(e => e.Type == "roles").Select(e => e.Value);
            var isMember = roles.Intersect(GameManageRole).Count() > 0;
            return isMember;
        }

        internal static string GetUserName(HttpRequest req)
        {
            var headers = req.Headers.ToDictionary(p => p.Key, p => (string)p.Value);
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(headers["Authorization"].Split(' ').Last());
            return token.Claims.Where(e => e.Type == "unique_name").Select(e => e.Value).First();
        }
    }
}