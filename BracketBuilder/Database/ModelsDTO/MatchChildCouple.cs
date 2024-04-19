namespace BracketBuilder.Database.ModelsDTO
{
    public class MatchChildCouple
    {
        public int Id { get; set; }
        public int? BelongToMatchId { get; set; }
        public int? ChildId { get; set; }
    }
}
