namespace JobBoardAPI.Model
{
    public class Application
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid JobId { get; set; }
        public string? CoverLetter { get; set; }
        public DateTime AppliedAt { get; set; }
    }
}