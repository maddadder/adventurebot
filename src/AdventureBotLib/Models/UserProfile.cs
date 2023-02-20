using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
        public bool ReceiveEmailNotificationFromSms { get; set; }
        public bool EmailIsVerified { get; set; }
        
    }
}