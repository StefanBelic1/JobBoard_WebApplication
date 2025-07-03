using BCrypt.Net;
using JobBoardAPI.Model;
using JobBoardAPI.Repository.Common;
using JobBoardAPI.Service.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JobBoardAPI.Service
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _usersRepository;

        public UserService(IUserRepository usersRepository)
        {
            _usersRepository = usersRepository ?? throw new ArgumentNullException(nameof(usersRepository));
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _usersRepository.GetAllUsersAsync();
        }

        public async Task<User> GetUserByIdAsync(Guid id)
        {
            return await _usersRepository.GetUserByIdAsync(id);
        }

        public async Task CreateUserAsync(List<User> users)
        {
            foreach (var user in users)
            {
                if (string.IsNullOrEmpty(user.FullName))
                    throw new ArgumentException("Username is required.");
                if (string.IsNullOrEmpty(user.Email))
                    throw new ArgumentException("Email is required.");
                if (string.IsNullOrEmpty(user.PasswordHash))
                    throw new ArgumentException("Password is required.");
                if (string.IsNullOrEmpty(user.Role) ||
                    (user.Role != "candidate" && user.Role != "employer"))
                    throw new ArgumentException("Role must be 'candidate' or 'employer'.");

                if (await _usersRepository.UsernameExistsAsync(user.FullName))
                    throw new InvalidOperationException($"Username '{user.FullName}' already exists.");
                if (await _usersRepository.EmailExistsAsync(user.Email))
                    throw new InvalidOperationException($"Email '{user.Email}' already exists.");

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
                user.CreatedAt = DateTime.UtcNow;
            }
            await _usersRepository.CreateUserAsync(users);
        }

        public async Task<bool> UpdateUserAsync(Guid id, User user)
        {
            if (string.IsNullOrEmpty(user.FullName))
                throw new ArgumentException("Username is required.");
            if (string.IsNullOrEmpty(user.Email))
                throw new ArgumentException("Email is required.");
            if (string.IsNullOrEmpty(user.Role) ||
                (user.Role != "candidate" && user.Role != "employer"))
                throw new ArgumentException("Role must be 'candidate' or 'employer'.");

            var existingUser = await _usersRepository.GetUserByIdAsync(id);
            if (existingUser == null)
                return false;

            if (user.FullName != existingUser.FullName && await _usersRepository.UsernameExistsAsync(user.FullName))
                throw new InvalidOperationException($"Username '{user.FullName}' already exists.");

            if (user.Email != existingUser.Email && await _usersRepository.EmailExistsAsync(user.Email))
                throw new InvalidOperationException($"Email '{user.Email}' already exists.");

            if (!string.IsNullOrEmpty(user.PasswordHash))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            else
                user.PasswordHash = existingUser.PasswordHash;

            return await _usersRepository.UpdateUserAsync(id, user);
        }

        public async Task DeleteUserAsync(Guid id)
        {
            await _usersRepository.DeleteUserAsync(id);
        }

        public async Task<Guid> RegisterAsync(string email, string fullname, string password, string role)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(fullname))
                throw new ArgumentException("Email, password, and full name are required.");
            if (role != "employer" && role != "candidate")
                throw new ArgumentException("Role must be 'employer' or 'candidate'.");

            var existingUser = await _usersRepository.GetUserByEmailAsync(email);
            if (existingUser != null)
                throw new InvalidOperationException("Email already in use.");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                FullName = fullname,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = role,
                CreatedAt = DateTime.UtcNow
            };

            await _usersRepository.CreateUserAsync(user);
            return user.Id;
        }

        public async Task<User> LoginAsync(string email, string password)
        {
            var user = await _usersRepository.GetUserByEmailAsync(email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) // Use BCrypt
                return null;

            return user;
        }
    }
}