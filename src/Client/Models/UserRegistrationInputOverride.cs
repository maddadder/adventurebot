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
}
