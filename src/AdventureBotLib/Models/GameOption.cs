using System;
using System.ComponentModel.DataAnnotations;

namespace AdventureBot.Models
{
    public class GameOption
    {
        [Required]
        public string description { get; set; }
        [Required]
        public string next { get; set; }
    }
}
