using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace BracketBuilder.Models;

[DebuggerDisplay($"{{Account.DisplayName}} - {{Account?.{nameof(UserAccount.AmoutOfWonMatches)}}}")]
public class Player
{
    [NotMapped]
    public Guid Guid { get; set; } = Guid.NewGuid();
    public int Id { get; set; } 
    public UserAccount Account { get; set; }
    public int Score { get; set; } = 0; 
    public int Position { get; set; } = 1; 
    public bool Winner { get; set; }

    [NotMapped]
    public string Identifier = "0";
}
