using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using JobBoardAPI.Model;
using JobBoardAPI.Repository.Common;

namespace JobBoardAPI.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public UserRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            var users = new List<User>();
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT \"id\", \"email\", \"password_hash\", \"role\", \"full_name\", \"created_at\" FROM \"users\"", conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        users.Add(new User
                        {
                            Id = reader.GetGuid(0),
                            Email = reader.IsDBNull(1) ? null : reader.GetString(1),
                            PasswordHash = reader.IsDBNull(2) ? null : reader.GetString(2),
                            Role = reader.IsDBNull(3) ? null : reader.GetString(3),
                            FullName = reader.IsDBNull(4) ? null : reader.GetString(4),
                            CreatedAt = reader.GetDateTime(5)
                        });
                    }
                }
            }
            return users;
        }

        public async Task<User> GetUserByIdAsync(Guid id)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT \"id\", \"email\", \"password_hash\", \"role\", \"full_name\", \"created_at\" FROM \"users\" WHERE \"id\" = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new User
                            {
                                Id = reader.GetGuid(0),
                                Email = reader.IsDBNull(1) ? null : reader.GetString(1),
                                PasswordHash = reader.IsDBNull(2) ? null : reader.GetString(2),
                                Role = reader.IsDBNull(3) ? null : reader.GetString(3),
                                FullName = reader.IsDBNull(4) ? null : reader.GetString(4),
                                CreatedAt = reader.GetDateTime(5)
                            };
                        }
                    }
                }
            }
            return null;
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            if (string.IsNullOrEmpty(username))
                return false;

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT COUNT(1) FROM \"users\" WHERE \"full_name\" = @FullName", conn))
                {
                    cmd.Parameters.AddWithValue("@FullName", username);
                    var count = (long)await cmd.ExecuteScalarAsync();
                    return count > 0;
                }
            }
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT COUNT(1) FROM \"users\" WHERE \"email\" = @Email", conn))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    var count = (long)await cmd.ExecuteScalarAsync();
                    return count > 0;
                }
            }
        }

        public async Task CreateUserAsync(List<User> users)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand("INSERT INTO \"users\" (\"id\", \"email\", \"password_hash\", \"role\", \"full_name\", \"created_at\") VALUES (@Id, @Email, @PasswordHash, @Role, @FullName, @CreatedAt)", conn))
                {
                    foreach (var user in users)
                    {
                        user.Id = Guid.NewGuid();
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@Id", user.Id);
                        cmd.Parameters.AddWithValue("@Email", user.Email ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Role", user.Role ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@FullName", user.FullName ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@CreatedAt", user.CreatedAt == default ? DateTime.UtcNow : user.CreatedAt);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
        }

        public async Task<bool> UpdateUserAsync(Guid id, User user)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand("UPDATE \"users\" SET \"full_name\" = @FullName, \"email\" = @Email, \"password_hash\" = @PasswordHash,\"role\"= @Role WHERE \"id\" = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@FullName", user.FullName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", user.Email ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Role", user.Role ?? (object)DBNull.Value);
                    int affectedRows = await cmd.ExecuteNonQueryAsync();
                    return affectedRows > 0;
                }
            }
        }

        public async Task DeleteUserAsync(Guid id)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand("DELETE FROM \"users\" WHERE \"id\" = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT id, email, password_hash, role, full_name, created_at FROM users WHERE email = @Email", conn))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new User
                            {
                                Id = reader.GetGuid(0),
                                Email = reader.GetString(1),
                                PasswordHash = reader.GetString(2),
                                Role = reader.GetString(3),
                                FullName = reader.IsDBNull(4) ? null : reader.GetString(4),
                                CreatedAt = reader.GetDateTime(5)
                            };
                        }
                    }
                }
            }
            return null;
        }

        public async Task CreateUserAsync(User user)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand("INSERT INTO users (id, email, password_hash, role, full_name, created_at) VALUES (@Id, @Email, @PasswordHash, @Role, @FullName, @CreatedAt)", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", user.Id);
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                    cmd.Parameters.AddWithValue("@Role", user.Role);
                    cmd.Parameters.AddWithValue("@FullName", user.FullName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}

/*public Guid Id { get; set; } 
        public string Email { get; set; }
        public string PasswordHash { get; set; } 
        public string Role { get; set; } // User role (candidate, employer)
        public string FullName { get; set; } 
        public DateTime CreatedAt { get; set; } */