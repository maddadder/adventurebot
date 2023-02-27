using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using AdventureBotUI.Client.Services;

namespace Read;
public partial class InitializeGameLoopInputOverride : InitializeGameLoopInput
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
}
