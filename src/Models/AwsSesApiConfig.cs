using System;
using System.Collections.Generic;
using System.Text;

namespace DurableFunctionDemoConfig.Models
{
    public class AwsSesApiConfig
    {
        public string SmtpHost { get; set; }
        public string SmtpPort { get; set; }
        public string SmtpUserName { get; set; }
        public string SmtpPassword { get; set; }
        public string SmtpToEmail { get; set; }
        public string SmtpFromEmail { get; set; }
    }
}
