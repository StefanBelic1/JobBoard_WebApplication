namespace JobBoardAPI.RestModels
{
    public class JobREST
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string? Location { get; set; }
        public string JobType { get; set; }
        public string? Category { get; set; }
        public DateTime PostedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public Guid EmployerId { get; set; }
    }
}