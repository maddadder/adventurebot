
namespace AdventureBot.Models
{
    public static class DbStrings
    {
        public const string CosmosDBDatabaseName = "leenetDb";
        public const string CosmosDBContainerName = "adventureBotCollection";
        public const string CosmosDBConnection = "CosmosDbConnectionString";
    }
    public static class AzureAd
    {
        public const string TenantId = "46f630ab-810d-4f10-b533-6532e8afe44a";
        public const string ClientId = "0cd119e5-2fad-4aba-a39f-d04e3b26f4ae";
        public const string TennantName = "leenet.link"; //e.g. contoso.onmicrosoft.com
    }
    public static class EventNames
    {
        public const string GameStateAdvanced = nameof(GameStateAdvanced);
        public const string DiscordStateAdvanced = nameof(DiscordStateAdvanced);
    }
    public static class VotingCounterOperationNames
    {
        public const string SetPriorVote = nameof(SetPriorVote);
        public const string Vote = nameof(Vote);
        public const string Get = nameof(Get);
        public const string Delete = nameof(Delete);
    }
    public static class DiscordVotingCounterOperationNames
    {
        public const string DiscordSetPriorVote = nameof(DiscordSetPriorVote);
        public const string DiscordVote = nameof(DiscordVote);
        public const string DiscordGet = nameof(DiscordGet);
        public const string DiscordDelete = nameof(DiscordDelete);
    }
}