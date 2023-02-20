using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using AdventureBotUI.Client;
using AdventureBotUI.Client.Services;

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

builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
    options.ProviderOptions.DefaultAccessTokenScopes.Add(bindAppSettings.ReadWriteScope);
});
await builder.Build().RunAsync();