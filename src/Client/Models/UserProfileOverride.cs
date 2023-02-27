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
            if(!string.IsNullOrEmpty(value))
            {
                try
                {
                    var address = new MailAddress(value); 
                    this.Email = value.Trim();
                }
                catch
                {
                    
                }
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
            if(!string.IsNullOrEmpty(value)){
                try
                {
                    var address = new MailAddress(value); 
                    this.PreferredUsername = value.Trim();
                }
                catch
                {
                    
                }
            }
            else
            {
                this.PreferredUsername = value;
            }
        }
    }
}