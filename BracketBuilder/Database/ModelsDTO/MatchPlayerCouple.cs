namespace BracketBuilder.Database.ModelsDTO
{
    public class MatchPlayerCouple
    {
        public int Id { get; set; }

        public int MatchId { get; set; }
        public int UserAccountId { get; set; }
        public int Position { get; set; } = 1;
        public int Score { get; set; } = 0;
        public bool Winner { get; set; }

    }
}
