namespace AdventureBotUI.Client.Services;
using Read;

public class AdventureBotReadService
{
    private readonly NSwagClientRead client;
    private readonly HttpClient _httpClient;
    
    public AdventureBotReadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        client = new NSwagClientRead(_httpClient);
    }
    public async Task<ICollection<GameEntry>> GameEntrySearchAsync(string name)
    {
        return await client.GameEntrySearchAsync("ge",name);
    }
    public async Task<CheckStatusResponse> UserRegistrationPostAsync(UserRegistrationInput input)
    {
        return await client.UserRegistrationPostAsync(input);
    }
    public async Task<string> UserRegistrationGetAsync(string token)
    {
        return await client.UserRegistrationGetAsync(token);
    }
    public async Task<CheckStatusResponse> GameLoopPostAsync(InitializeGameLoopInput input)
    {
        return await client.GameLoopPostAsync(input);
    }
    public async Task<string> GameLoopPutAsync(string token, GameLoopInput input)
    {
        return await client.GameLoopPutAsync(token,input);
    }
    public async Task<VotingCounter> GameLoopGetAsync(string token)
    {
        return await client.GameLoopGetAsync(token);
    }
    public async Task<CheckStatusResponse> DiscordLoopPostAsync(InitializeDiscordLoopInput input)
    {
        return await client.DiscordLoopPostAsync(input);
    }
    public async Task<string> DiscordLoopPutAsync(string token, DiscordLoopInput input)
    {
        return await client.DiscordLoopPutAsync(token,input);
    }
    public async Task<DiscordVotingCounter> DiscordLoopGetAsync(string token)
    {
        return await client.DiscordLoopGetAsync(token);
    }
}