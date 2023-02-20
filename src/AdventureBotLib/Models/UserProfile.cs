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
        [Required]
        public bool ReceiveEmailNotificationFromSms { get; set; }
        [Required]
        public bool EmailIsVerified { get; set; }
        
    }
}