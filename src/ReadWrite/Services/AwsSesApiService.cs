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
            sb.Append($@"Dear {UserName},<br/>
Your game state is the following: {userProfileGameEntry.gameEntry.name}<br/><br/>");
            foreach(var desc in userProfileGameEntry.gameEntry.description){
                sb.Append($"{desc}<br/>");
            }
            sb.Append($"<br/>Options:<br/>");
            foreach(var option in userProfileGameEntry.gameEntry.options){
                sb.Append($"<a href='{BaseUrl}/gameview/{option.next}'>{option.description}</a><br/>");
            }
            sb.Append($@"<br/>
<br/>
You received the above message because you have 'Receive Email Notifications' turned on. <br/>
To unsubscribe from these messages click here:");
            return sb.ToString();
        }
        public async Task SendEmail(UserProfile userProfile, string Subject, string Body)
        {
            if(userProfile.ReceiveEmailNotificationFromSms == false)
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