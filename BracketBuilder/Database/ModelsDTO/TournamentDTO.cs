using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BracketBuilder.Database.ModelsDTO;

public class TournamentDTO
{ 
    public int Id { get; set; }
    public Guid Guid { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "Tournament";
    public int CreatorId { get; set; } //secondairy key?? or something?
    public int FinalMatchId { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.Now;
    public DateTime? StartingDate { get; set; } = null;
    public DateTime? FinishDate { get; set; } = null;
    public string? Description { get; set; }
    public int MaxPlayers { get; set; }
    public string Password { get; set; } = string.Empty;
    public bool isPublic { get; set; }

    public bool Finished { get; set; }
    public int FirstId { get; set; }
    public int SecondId { get; set; }
    public int ThirdId { get; set; }

    public int AmountOfPlayerInMatch { get; set; } = 4;
    public int AmountOfMatchesInPool { get; set; } = 2;
    public int AmountOfStart { get; set; } = 2;
    public bool AlwaysFit { get; set; } = true;

}
