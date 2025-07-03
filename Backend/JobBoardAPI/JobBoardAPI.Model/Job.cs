namespace JobBoardAPI.Model
{
    public class Job
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string? Location { get; set; }
        public string JobType { get; set; } //remote, full-time, part-time
        public string? Category { get; set; }
        public DateTime PostedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public Guid EmployerId { get; set; }
    }
}