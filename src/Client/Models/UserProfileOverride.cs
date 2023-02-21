using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using Newtonsoft.Json;

namespace ReadWrite;
public partial class UserProfileOverride : UserProfile
{
    public UserProfileOverride()
    {
        
    }

    public string EmailEntry
    { 
        get
        {
            return this.Email;
        }
        set
        {
            if(!string.IsNullOrEmpty(value)){
                var address = new MailAddress(value); 
                this.Email = value;
            }
            else
            {
                this.Email = value;
            }
        }
    }
    [JsonProperty(Required = Required.Always)]
    public string PreferredUsernameEntry
    { 
        get
        {
            return this.PreferredUsername;
        }
        set
        {
            var address = new MailAddress(value); 
            this.PreferredUsername = value;
        }
    }
}