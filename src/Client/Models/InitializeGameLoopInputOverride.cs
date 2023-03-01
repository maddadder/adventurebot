using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using AdventureBotUI.Client.Services;

namespace Read;
public partial class InitializeGameLoopInputOverride : InitializeGameLoopInput
{

    public List<GameLoopSubscription> Subscriptions {get;set;} = new List<GameLoopSubscription>();
}
public class GameLoopSubscription
{
    [Required]
    public string Value{get;set;}
}