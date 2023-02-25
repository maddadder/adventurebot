using System;
using Newtonsoft.Json;
namespace AdventureBot.Models
{
    public class EmailConfirmationOrchestratorStatus
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("expireAt")]
        public DateTime? ExpireAt { get; set; }

        [JsonProperty("registrationConfirmationURL")]
        public string RegistrationConfirmationURL { get; set; }

    }
}