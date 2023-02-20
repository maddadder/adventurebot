namespace AdventureBotUI.Client.Services;
using Read;

public class AdventureBotReadService
{
    private readonly playleenetadventurebotClient client;
    private readonly HttpClient _httpClient;
    
    public AdventureBotReadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        client = new playleenetadventurebotClient(_httpClient);
    }
    public async Task<ICollection<GameEntry>> GameEntrySearchAsync(string name)
    {
        return await client.GameEntrySearchAsync("ge",name);
    }
}