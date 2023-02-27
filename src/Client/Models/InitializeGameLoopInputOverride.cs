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
            this.Email = ConstantsLib.SetEmail(value);
        }
    }
}
