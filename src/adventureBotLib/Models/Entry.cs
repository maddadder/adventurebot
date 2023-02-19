using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DurableFunctionDemoConfig.Models
{
    public class Entry
    {
        [Required]
        public Guid id { get; set; }
        public string __T {get;set;}
        [Required]
        public string name { get; set; }
        public DateTime Created { get;set; }
        public DateTime Modified { get;set; }
    }
}