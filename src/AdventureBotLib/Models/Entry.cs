using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace AdventureBot.Models
{
    public class Entry
    {
        [Required]
        public Guid id { get; set; }
        public string __T {get;set;}
        public DateTime Created { get;set; }
        public DateTime Modified { get;set; }
        [JsonProperty(PropertyName = "createdBy", Required = Required.AllowNull)]
        public string CreatedBy { get;set; }
        [JsonProperty(PropertyName = "modifiedBy", Required = Required.AllowNull)]
        public string ModifiedBy { get;set; }
    }
}