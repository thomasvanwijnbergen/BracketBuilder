using BracketBuilder.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BracketBuilder.Database.ModelsDTO;

public class MatchDTO
{
    public int Id { get; set; }
    public int Position { get; set; }
    public int TournamentId { get; set; }

    public int? ParentId { get; set; }
    //public int? LeftChildId { get; set; }
    //public int? RightChildId { get; set; }

}
