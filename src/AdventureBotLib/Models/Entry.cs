using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AdventureBot.Models
{
    public class Entry
    {
        [Required]
        public Guid id { get; set; }
        public string __T {get;set;}
        public DateTime Created { get;set; }
        public DateTime Modified { get;set; }
        public string CreatedBy { get;set; }
        public string ModifiedBy { get;set; }
    }
}