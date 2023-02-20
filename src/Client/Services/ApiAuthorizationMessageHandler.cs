using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace AdventureBotUI.Client.Services;

public class ApiAuthorizationMessageHandler : AuthorizationMessageHandler
{
    public ApiAuthorizationMessageHandler(IAccessTokenProvider provider,
        NavigationManager navigationManager)
        : base(provider, navigationManager)
    {
        ConfigureHandler(
            authorizedUrls: new[] { "https://leenetadventurebot.azurewebsites.net", "https://localhost:7017" },
            scopes: new[] { "api://efdbc13e-cee9-4f9b-8fa2-4044faa4674a/user_impersonation" });
    }
}