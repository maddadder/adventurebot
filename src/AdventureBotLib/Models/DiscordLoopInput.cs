using Newtonsoft.Json;

namespace AdventureBot.Models
{
    public class DiscordLoopInput
    {
        public DiscordLoopInput(string subscriberId, string targetChannelId, string gameState)
        {
            this.SubscriberId = subscriberId;
            this.TargetChannelId = targetChannelId;
            this.GameState = gameState;
        }
        
        [JsonProperty("subscriberId")]
        public string SubscriberId { get; set; }

        [JsonProperty("targetChannelId")]
        public string TargetChannelId { get; set; }

        [JsonProperty("gameState")]
        public string GameState { get; set; }
    }
}