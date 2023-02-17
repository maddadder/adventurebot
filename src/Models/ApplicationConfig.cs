using System;
using System.Collections.Generic;
using System.Text;

namespace DurableFunctionDemoConfig.Models
{
    public class ApplicationConfig
    {
        public string BaseUrl { get; set; }
        public string CosmosDbConnectionString { get; set; }
    }
}
