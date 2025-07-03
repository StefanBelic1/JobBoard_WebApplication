namespace JobBoardAPI.RestModels
{
    public class UserREST
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; } 
    }
}
