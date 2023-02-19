using AdventureBot.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;


namespace AdventureBot.Services
{
    public class AwsSesApiService : IAwsSesApiService
    {
        private readonly string SmtpHost;
        private readonly string SmtpPort;
        private readonly string SmtpUserName;
        private readonly string SmtpPassword;
        private readonly string SmtpToEmail;
        private readonly string SmtpFromEmail;
        private readonly string GithubUsername;
        private readonly string BaseUrl;
        
        public AwsSesApiService(
            IOptions<AwsSesApiConfig> awsSesApiConfig,
            IOptions<GitHubApiConfig> gitHubApiConfig,
            IOptions<ApplicationConfig> applicationConfig)
        {
            var awsSesApiConfigValue = awsSesApiConfig.Value;
            var gitHubApiConfigValue = gitHubApiConfig.Value;
            var applicationConfigValue = applicationConfig.Value;
            SmtpHost = awsSesApiConfigValue.SmtpHost;
            SmtpPort = awsSesApiConfigValue.SmtpPort;
            SmtpUserName = awsSesApiConfigValue.SmtpUserName;
            SmtpPassword = awsSesApiConfigValue.SmtpPassword;
            SmtpToEmail = awsSesApiConfigValue.SmtpToEmail;
            SmtpFromEmail = awsSesApiConfigValue.SmtpFromEmail;
            BaseUrl = applicationConfigValue.BaseUrl;
            GithubUsername = gitHubApiConfigValue.Username;
        }
        public async Task<string> RenderRepoViewCount(RepoViewCount[] repoViewCounts){
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append($@"Dear {SmtpToEmail},
A user has requested to a repo view count from {GithubUsername}
");
            foreach(RepoViewCount repoViewCount in repoViewCounts){
                sb.Append($"{repoViewCount.RepoName} ({repoViewCount.ViewCount})\n");
            }
            sb.Append($@"

You received the above message because you have 'Receive Email Notifications' turned on. 
To resend this message click here: {BaseUrl}/api/HttpTrigger
To unsubscribe from these messages click here:");
            return sb.ToString();
        }
        public async Task SendEmail(string Subject, string Body)
        {
            int port = 25;
            int.TryParse(SmtpPort, out port);
            using (var client = new System.Net.Mail.SmtpClient(SmtpHost, port))
            {
                client.Credentials = new System.Net.NetworkCredential(SmtpUserName, SmtpPassword);
                client.EnableSsl = true;
                var mailMessage = new System.Net.Mail.MailMessage(SmtpFromEmail, SmtpToEmail);
                mailMessage.Subject = Subject;
                mailMessage.Body = Body;
                await client.SendMailAsync(mailMessage);
            }
        }
    }
}