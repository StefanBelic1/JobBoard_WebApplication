namespace JobBoardAPI.RestModels
{
    public class ApplicationREST
    {
        public Guid Id { get; set; }
       
        public Guid JobId { get; set; }
        public Guid UserId { get; set; }
        public string CoverLetter { get; set; }
       
        public DateTime AppliedAt { get; set; }
        public string JobTitle { get; set; }
    }
}