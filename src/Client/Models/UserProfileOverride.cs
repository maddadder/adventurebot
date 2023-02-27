using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using AdventureBotUI.Client.Services;
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
            this.Email = ConstantsLib.SetEmail(value);
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
            this.PreferredUsername = ConstantsLib.SetEmail(value);
        }
    }
}