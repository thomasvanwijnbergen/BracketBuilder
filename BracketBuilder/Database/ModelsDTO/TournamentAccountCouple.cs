namespace BracketBuilder.Database.ModelsDTO
{
    public class TournamentAccountCouple
    {
        public int Id { get; set; }

        public int TournamentId { get; set; }
        public int UserAccountId { get; set; }
    }
}
