
using Newtonsoft.Json;
namespace AdventureBot.Models
{
    public class SendConfirmationEmailInput
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }        

        [JsonProperty("confirmationURL")]
        public string RegistrationConfirmationURL { get; set; }
    }
}