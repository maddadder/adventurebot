using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace DurableFunctionDemoConfig.Models
{
    public class GameEntry : Entry
    {
        [Required]
        public List<string> description { get; set; }
        [Required]
        public List<GameOption> options { get; set; }
        
    }
}