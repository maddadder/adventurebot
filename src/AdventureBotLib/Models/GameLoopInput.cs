using Newtonsoft.Json;

namespace AdventureBot.Models
{
    public class GameLoopInput
    {
        public GameLoopInput(string subscriberId, string subscriberEmail, string gameState)
        {
            this.SubscriberId = subscriberId;
            this.SubscriberEmail = subscriberEmail;
            this.GameState = gameState;
        }
        
        [JsonProperty("subscriberId")]
        public string SubscriberId { get; set; }

        [JsonProperty("subscriberEmail")]
        public string SubscriberEmail { get; set; }

        [JsonProperty("gameState")]
        public string GameState { get; set; }
    }
}