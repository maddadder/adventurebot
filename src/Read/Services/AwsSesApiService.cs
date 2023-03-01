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
        private readonly string BaseUrl;
        
        public AwsSesApiService(
            IOptions<AwsSesApiConfig> awsSesApiConfig,
            IOptions<ApplicationConfig> applicationConfig)
        {
            var awsSesApiConfigValue = awsSesApiConfig.Value;
            var applicationConfigValue = applicationConfig.Value;
            SmtpHost = awsSesApiConfigValue.SmtpHost;
            SmtpPort = awsSesApiConfigValue.SmtpPort;
            SmtpUserName = awsSesApiConfigValue.SmtpUserName;
            SmtpPassword = awsSesApiConfigValue.SmtpPassword;
            SmtpToEmail = awsSesApiConfigValue.SmtpToEmail;
            SmtpFromEmail = awsSesApiConfigValue.SmtpFromEmail;
            BaseUrl = applicationConfigValue.BaseUrl;
        }
        public async Task<string> RenderUserProfileGameEntry(UserProfileGameEntry userProfileGameEntry){
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if(userProfileGameEntry.userProfile == null)
                return "";
            if(userProfileGameEntry.gameEntry == null)
            {
                string UserName = $"{userProfileGameEntry.userProfile.FirstName} {userProfileGameEntry.userProfile.LastName}";
                sb.Append($@"Dear {UserName},<br/><br/>");
                sb.Append($@"This part of the game is still under construction or has no ending.<br/><br/>");
                sb.Append($"<a href='{BaseUrl}/gameadvance/begin'>Start Over</a><br/><br/>");
                sb.Append($@"<br/>
<br/>
You received the above message because you have 'Receive Game Advance Email' turned on. <br/>
To unsubscribe from these messages click <a href='{BaseUrl}/unsubscribe'>here</a>");
                return sb.ToString();
            }
            else
            {
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
        }
        public async Task<string> RenderGameStateGameEntry(SendReceiveGameStateInput gameState, GameEntry gameEntry){
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if(gameState == null || string.IsNullOrEmpty(gameState.Email))
                return "";
            if(gameEntry == null)
            {
                string UserName = $"{gameState.Name}";
                sb.Append($@"Dear {UserName},<br/><br/>");
                sb.Append($@"The adventurers in your party are: {string.Join(",",gameState.Subscribers)}<br/><br/>");
                sb.Append($@"This part of the game is still under construction or has no ending.<br/><br/>");
                sb.Append($"<a href='{gameState.RegistrationConfirmationURL}/{gameState.Email}/begin/'>Start Over</a><br/><br/>");
                sb.Append($@"<br/>
<br/>
You received the above message because you responded to the game within 24 hours. <br/>
To unsubscribe from these messages do not respond for 24 hours and the game will end.");
                return sb.ToString();
            }
            else
            {
                string UserName = $"{gameState.Name}";
                sb.Append($@"Dear {UserName},<br/><br/>");
                sb.Append($@"The adventurers in your party are: {string.Join(",",gameState.Subscribers)}<br/><br/>");
                foreach(var desc in gameEntry.description){
                    sb.Append($"{desc}<br/>");
                }
                sb.Append($"<br/>Options:<br/><br/>");
                if(!gameEntry.options.Any())
                {
                    sb.Append($"<a href='{gameState.RegistrationConfirmationURL}/{gameState.Email}/begin/'>Start Over</a><br/><br/>");
                }
                else
                {
                    foreach(var option in gameEntry.options){
                        sb.Append($"<a href='{gameState.RegistrationConfirmationURL}/{gameState.Email}/{option.next}/'>{option.description}</a><br/><br/>");
                    }
                }
                sb.Append($@"<br/>
<br/>
You received the above message because you responded to the game within 24 hours. <br/>
To unsubscribe from these messages do not respond for 24 hours and the game will end.");
                return sb.ToString();
            }
        }
        public async Task SendEmail(string ToEmailAddress, string Subject, string Body)
        {

            int port = 25;
            int.TryParse(SmtpPort, out port);
            using (var client = new System.Net.Mail.SmtpClient(SmtpHost, port))
            {
                client.Credentials = new System.Net.NetworkCredential(SmtpUserName, SmtpPassword);
                client.EnableSsl = true;
                var mailMessage = new System.Net.Mail.MailMessage(SmtpFromEmail, ToEmailAddress);
                mailMessage.Subject = Subject;
                mailMessage.Body = Body;
                mailMessage.IsBodyHtml = true;
                await client.SendMailAsync(mailMessage);
            }
        }
    }
}