using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace AdventureBot.Models
{
    public class UserProfile : Entry
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [EmailAddress]
        public string PreferredUsername { get; set; }
        [JsonProperty(PropertyName = "receiveGameAdvanceEmail", Required = Required.Always)]
        public bool ReceiveGameAdvanceEmail { get; set; }
        [JsonProperty(PropertyName = "emailIsVerified", Required = Required.Always)]
        public bool EmailIsVerified { get; set; }
        [Required]
        public string GameEntryState { get; set; }
    }
}