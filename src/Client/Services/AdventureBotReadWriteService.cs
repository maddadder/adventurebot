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

    public async Task<IEnumerable<GameEntry>> GameEntryListAsync(string search)
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
    public async Task<GameEntry> GameEntryGetAsync(string id)
    {
        return await client.GameEntryGetAsync("ge", id);
    }
    public async Task GameEntryPutAsync(GameEntry gameEntry)
    {
        await client.GameEntryPutAsync("ge", gameEntry.Id.ToString(), gameEntry);
    }
    
    public async Task GameEntryPostAsync(GameEntry gameEntry)
    {
        await client.GameEntryPostAsync("ge", gameEntry);
    }
    public async Task GameEntryDeleteAsync(string gameEntryId)
    {
        await client.GameEntryDeleteAsync("ge", gameEntryId);
    }

    public async Task<IEnumerable<UserProfile>> UserProfileListAsync(string search)
    {
        try
        {
            return await client.UserProfileListAsync("up", search);
        }
        catch
        {
            return new List<UserProfile>();
        }
    }
    public async Task<UserProfile> UserProfileGetAsync(string id)
    {
        return await client.UserProfileGetAsync("up", id);
    }
    public async Task UserProfilePutAsync(UserProfile userProfile)
    {
        await client.UserProfilePutAsync("up", userProfile.Id.ToString(), userProfile);
    }
    
    public async Task UserProfilePostAsync(UserProfile userProfile)
    {
        await client.UserProfilePostAsync("up", userProfile);
    }
    public async Task UserProfileDeleteAsync(string userProfileId)
    {
        await client.UserProfileDeleteAsync("up", userProfileId);
    }
}