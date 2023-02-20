using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;

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
        [OpenApiProperty(Nullable = false, Default=false)]
        public bool ReceiveEmailNotificationFromSms { get; set; }
        [OpenApiProperty(Nullable = false, Default=false)]
        public bool EmailIsVerified { get; set; }
        
    }
}