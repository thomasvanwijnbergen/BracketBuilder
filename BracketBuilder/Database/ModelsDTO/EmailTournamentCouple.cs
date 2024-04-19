namespace BracketBuilder.Database.ModelsDTO
{
    public class EmailTournamentCouple
    {
        public EmailTournamentCouple() { }
        public EmailTournamentCouple(int tournamentId, string? domain)
        { 
            this.TournamentId = tournamentId;
            this.Domain = domain;
        }
        public int Id { get; set; }
        public int? TournamentId { get; set; }
        public string? Domain { get; set; }
    }
}
