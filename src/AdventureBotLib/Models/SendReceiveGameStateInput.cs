
using Newtonsoft.Json;
namespace AdventureBot.Models
{
    public class SendReceiveGameStateInput
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("subscriberId")]
        public string SubscriberId { get; set; }
        
        [JsonProperty("subscribers")]
        public List<string> Subscribers { get; set; } = new List<string>();
        
        [JsonProperty("gameState")]
        public string GameState { get; set; }

        [JsonProperty("instanceId")]
        public string InstanceId { get; set; }

        [JsonProperty("confirmationURL")]
        public string RegistrationConfirmationURL { get; set; }
    }
}