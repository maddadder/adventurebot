namespace AdventureBotUI.Client.Services;
using ReadWrite;

public class AdventureBotReadWriteService
{
    private readonly NSwagClientReadWrite client;
    private readonly HttpClient _httpClient;
    
    public AdventureBotReadWriteService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        client = new NSwagClientReadWrite(_httpClient);
    }

    public async Task<IEnumerable<GameEntry>> GameEntrySearchAsync(string search)
    {
        try
        {
            return await client.GameEntrySearchAsync("ge", search);
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

    public async Task<IEnumerable<UserProfile>> UserProfileSearchAsync(string search)
    {
        try
        {
            return await client.UserProfileSearchAsync("up", search);
        }
        catch
        {
            return new List<UserProfile>();
        }
    }

    public async Task<IEnumerable<UserProfile>> UserProfileListAsync()
    {
        try
        {
            return await client.UserProfileListAsync("up");
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
    public async Task<CheckStatusResponse> UserRegistrationPostAsync(UserProfile userProfile)
    {
        UserRegistrationInput input = new UserRegistrationInput();
        input.Email = userProfile.Email;
        input.Name = userProfile.FirstName + " " + userProfile.LastName;
        return await client.UserRegistrationPostAsync(input);
    }
}