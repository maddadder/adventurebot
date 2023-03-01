using Newtonsoft.Json;

namespace AdventureBot.Models
{
    public class InitializeGameLoopInput
    {

        [JsonProperty("subscribers")]
        public List<string> Subscribers { get; set; } = new List<string>();

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