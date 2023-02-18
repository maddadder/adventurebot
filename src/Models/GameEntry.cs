using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace DurableFunctionDemoConfig.Models
{
    public class GameEntry
    {
        [Required]
        public Guid id { get; set; }
        [Required]
        public string __T {get;set;}
        [Required]
        public string name { get; set; }
        [Required]
        public List<string> description { get; set; }
        [Required]
        public List<GameOption> options { get; set; }
        public DateTime Created { get;set; }
        public DateTime Modified { get;set; }

    }
}