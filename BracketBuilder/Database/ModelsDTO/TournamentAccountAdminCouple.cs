namespace BracketBuilder.Database.ModelsDTO
{
    public class TournamentAccountAdminCouple
    {
        public int Id { get; set; }

        public int TournamentId { get; set; }
        public int UserAccountId { get; set; }
    }
}
