using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace AdventureBotUI.Client.Services;

public class ApiAuthorizationMessageHandler : AuthorizationMessageHandler
{
    public ApiAuthorizationMessageHandler(AppSettings appsettings, IAccessTokenProvider provider,
        NavigationManager navigationManager)
        : base(provider, navigationManager)
    {
        ConfigureHandler
        (
            authorizedUrls: new[] { appsettings.ReadWriteApiUrl },
            scopes: new[] { appsettings.ReadWriteScope });
    }
}