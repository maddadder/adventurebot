using Newtonsoft.Json;

namespace AdventureBot.Models
{
    public class InitializeGameLoopInput
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("initialGameState")]
        public string InitialGameState { get; set; }

        [JsonProperty("baseUri")]
        public string BaseUri { get; set; }

        [JsonProperty("instanceId")]
        public string InstanceId { get; set; }
        
        public Queue<string> PriorState { get; set; } = new Queue<string>();
    }
}