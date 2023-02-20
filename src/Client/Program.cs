using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using AdventureBotUI.Client;
using AdventureBotUI.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<ApiAuthorizationMessageHandler>();

builder.Services.AddHttpClient<AdventureBotReadService>(client =>
{
    client.BaseAddress = new Uri("https://playleenetadventurebot.azurewebsites.net/api/");
});

builder.Services.AddHttpClient<AdventureBotReadWriteService>(client =>
{
    client.BaseAddress = new Uri("https://leenetadventurebot.azurewebsites.net/api/");
}).AddHttpMessageHandler<ApiAuthorizationMessageHandler>();

builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
    options.ProviderOptions.DefaultAccessTokenScopes.Add("api://efdbc13e-cee9-4f9b-8fa2-4044faa4674a/user_impersonation");
});
await builder.Build().RunAsync();