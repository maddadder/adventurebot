using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using AdventureBotUI.Client.Services;

namespace Read;
public partial class UserRegistrationInputOverride : UserRegistrationInput
{

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
    public string UserName
    { 
        get
        {
            if(string.IsNullOrEmpty(this.Email))
                return ConstantsLib.TenantName;
            var emailParams = this.Email.Split("@");
            if(emailParams.Count() < 2){
                return ConstantsLib.TenantName;
            }
            return $"{emailParams[0]}@{ConstantsLib.TenantName}";
        }
    }
}
