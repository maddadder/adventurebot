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
}