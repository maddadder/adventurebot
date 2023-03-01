using Newtonsoft.Json;

namespace AdventureBot.Models
{
    public class GameLoopInput
    {
        public GameLoopInput(string subscriber, string gameState){
            this.Subscriber = subscriber;
            this.GameState = gameState;
        }
        [JsonProperty("subscriber")]
        public string Subscriber { get; set; }

        [JsonProperty("gameState")]
        public string GameState { get; set; }
    }
}