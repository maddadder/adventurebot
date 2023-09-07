
using Newtonsoft.Json;
namespace AdventureBot.Models
{
    public class SendReceiveDiscordStateInput
    {
        [JsonProperty("targetChannelId")]
        public string TargetChannelId { get; set; }

        [JsonProperty("subscriberId")]
        public string SubscriberId { get; set; }
        
        [JsonProperty("gameState")]
        public string GameState { get; set; }

        [JsonProperty("instanceId")]
        public string InstanceId { get; set; }

        [JsonProperty("confirmationURL")]
        public string RegistrationConfirmationURL { get; set; }
    }
}