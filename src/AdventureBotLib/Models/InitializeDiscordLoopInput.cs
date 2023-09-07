using Newtonsoft.Json;

namespace AdventureBot.Models
{
    public class InitializeDiscordLoopInput
    {

        [JsonProperty("targetChannelId")]
        public string TargetChannelId { get; set; }

        [JsonProperty("initialGameState")]
        public string InitialGameState { get; set; }

        [JsonProperty("baseUri")]
        public string BaseUri { get; set; }

        [JsonProperty("instanceId")]
        public string InstanceId { get; set; }
        
        [JsonProperty("gameDelay")]
        public TimeSpan GameDelay { get; set; }

    }
}