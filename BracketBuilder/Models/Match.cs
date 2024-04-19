using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BracketBuilder.Models;

public class Match
{
    [NotMapped]
    public Guid Guid { get; set; } = Guid.NewGuid();
    public int Id { get; set; }
    public int Position { get; set; }
    public int TournamentId { get; set; }
    public List<Player> Competitors { get; set; } = new List<Player>();

    public bool isActive
    {
        get
        {
            if (Parent?.Parent?.Competitors.Count >= 1)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
    // Navigation properties
    public Match? Parent { get; set; }
    //public Match? LeftChild { get; set; }
    //public Match? RightChild { get; set; }
    [NotMapped]
    public Match? LeftChild
    {
        get => Children[0];
        set => Children[0] = value;
    }
    [NotMapped]
    public Match? RightChild
    {
        get => Children[1];
        set => Children[1] = value;
    }
    public List<Match> Children { get; set; } = new List<Match> { };
}
