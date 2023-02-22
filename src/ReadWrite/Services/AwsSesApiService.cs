using AdventureBot.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public async Task<string> RenderUserProfileGameEntry(UserProfileGameEntry userProfileGameEntry){
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if(userProfileGameEntry.userProfile == null)
                return "";
            if(userProfileGameEntry.gameEntry == null)
                return "";
            string UserName = $"{userProfileGameEntry.userProfile.FirstName} {userProfileGameEntry.userProfile.LastName}";
            sb.Append($@"Dear {UserName},<br/><br/>");
            foreach(var desc in userProfileGameEntry.gameEntry.description){
                sb.Append($"{desc}<br/>");
            }
            sb.Append($"<br/>Options:<br/><br/>");
            if(!userProfileGameEntry.gameEntry.options.Any())
            {
                sb.Append($"<a href='{BaseUrl}/gameadvance/begin'>Start Over</a><br/><br/>");
            }
            else
            {
                foreach(var option in userProfileGameEntry.gameEntry.options){
                    sb.Append($"<a href='{BaseUrl}/gameadvance/{option.next}'>{option.description}</a><br/><br/>");
                }
            }
            sb.Append($@"<br/>
<br/>
You received the above message because you have 'Receive Game Advance Email' turned on. <br/>
To unsubscribe from these messages click <a href='{BaseUrl}/unsubscribe'>here</a>");
            return sb.ToString();
        }
        public async Task SendEmail(UserProfile userProfile, string Subject, string Body)
        {
            if(userProfile.ReceiveGameAdvanceEmail == false)
            {
                return;
            }
            if(string.IsNullOrEmpty(userProfile.Email))
            {
                return;
            }
            int port = 25;
            int.TryParse(SmtpPort, out port);
            using (var client = new System.Net.Mail.SmtpClient(SmtpHost, port))
            {
                client.Credentials = new System.Net.NetworkCredential(SmtpUserName, SmtpPassword);
                client.EnableSsl = true;
                var mailMessage = new System.Net.Mail.MailMessage(SmtpFromEmail, userProfile.Email);
                mailMessage.Subject = Subject;
                mailMessage.Body = Body;
                mailMessage.IsBodyHtml = true;
                await client.SendMailAsync(mailMessage);
            }
        }
    }
}