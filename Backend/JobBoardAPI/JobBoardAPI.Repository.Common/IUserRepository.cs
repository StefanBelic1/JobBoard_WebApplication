using JobBoardAPI.Model;
namespace JobBoardAPI.Repository.Common
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> GetUserByIdAsync(Guid id);
        Task<bool> UsernameExistsAsync(string username);
        Task<bool> EmailExistsAsync(string email);
        Task CreateUserAsync(List<User> users);
        Task<bool> UpdateUserAsync(Guid id, User user);
        Task DeleteUserAsync(Guid id);
        Task<User> GetUserByEmailAsync(string email);
        Task CreateUserAsync(User user);
    }
}
