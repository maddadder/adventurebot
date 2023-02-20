using System.ComponentModel.DataAnnotations;

namespace ReadWrite;
public partial class GameEntryOverride : GameEntry
{
    public GameEntryOverride(){
        Options = new List<GameOption>();
    }
    public List<GameEntryDescription> Descriptions {get;set;} = new List<GameEntryDescription>();
}
public class GameEntryDescription
{
    [Required]
    public string Value {get;set;}
}