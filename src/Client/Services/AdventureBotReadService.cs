namespace AdventureBotUI.Client.Services;

public class AdventureBotReadService
{
    private readonly swaggerClient client;
    private readonly HttpClient _httpClient;
    
    public AdventureBotReadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        client = new swaggerClient(_httpClient);
    }
    public async Task<ICollection<GameEntry>> GameEntryListAsync(string name)
    {
        return await client.GameEntryListAsync("ge",name);
    }
}