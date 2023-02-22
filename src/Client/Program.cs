using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using AdventureBotUI.Client;
using AdventureBotUI.Client.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var bindAppSettings = new AppSettings();
builder.Configuration.Bind("AppSettings", bindAppSettings);
builder.Services.AddSingleton(bindAppSettings);

builder.Services.AddScoped<ApiAuthorizationMessageHandler>();

builder.Services.AddHttpClient<AdventureBotReadService>(client =>
{
    client.BaseAddress = new Uri($"{bindAppSettings.ReadApiUrl}/api/");
});

builder.Services.AddHttpClient<AdventureBotReadWriteService>(client =>
{
    client.BaseAddress = new Uri($"{bindAppSettings.ReadWriteApiUrl}/api/");
}).AddHttpMessageHandler<ApiAuthorizationMessageHandler>();

var baseUrl = string.Join("/", 
    builder.Configuration.GetSection("MicrosoftGraph")["BaseUrl"], 
    builder.Configuration.GetSection("MicrosoftGraph")["Version"]);
var scopes = builder.Configuration.GetSection("MicrosoftGraph:Scopes")
    .Get<List<string>>();

builder.Services.AddGraphClient(baseUrl, scopes);

builder.Services.AddMsalAuthentication<RemoteAuthenticationState,
    CustomUserAccount>(options =>
    {
        builder.Configuration.Bind("AzureAd",
            options.ProviderOptions.Authentication);
        options.ProviderOptions.DefaultAccessTokenScopes.Add(bindAppSettings.ReadWriteScope);
    })
    .AddAccountClaimsPrincipalFactory<RemoteAuthenticationState, CustomUserAccount,
        CustomAccountFactory>();
await builder.Build().RunAsync();