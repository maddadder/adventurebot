namespace AdventureBotUI.Client.Services;
using ReadWrite;

public class AdventureBotReadWriteService
{
    private readonly leenetadventurebotClient client;
    private readonly HttpClient _httpClient;
    
    public AdventureBotReadWriteService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        client = new leenetadventurebotClient(_httpClient);
    }

    public async Task<IEnumerable<GameEntry>> List(string search)
    {
        try
        {
            return await client.GameEntryListAsync("ge", search);
        }
        catch
        {
            return new List<GameEntry>();
        }
    }
    public async Task<GameEntry> Get(string id)
    {
        return await client.GameEntryGetAsync("ge", id);
    }
    public async Task Put(GameEntry gameEntry)
    {
        await client.GameEntryPutAsync("ge", gameEntry.Id.ToString(), gameEntry);
    }
    
    public async Task Post(GameEntry gameEntry)
    {
        await client.GameEntryPostAsync("ge", gameEntry);
    }
    public async Task Delete(string gameEntryId)
    {
        await client.GameEntryDeleteAsync("ge", gameEntryId);
    }
}