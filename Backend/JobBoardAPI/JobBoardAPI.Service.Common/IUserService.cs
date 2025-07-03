using JobBoardAPI.Model;
namespace JobBoardAPI.Service.Common
{
    public interface IUserService
    {
        public Task<IEnumerable<User>> GetAllUsersAsync();
        public Task<User?> GetUserByIdAsync(Guid id);
        public Task CreateUserAsync(List<User> users);
        public Task<bool> UpdateUserAsync(Guid id, User user);
        public Task DeleteUserAsync(Guid id);
        Task<Guid> RegisterAsync(string email,string fullname, string password, string role);
        Task<User> LoginAsync(string email, string password);
    }
}

