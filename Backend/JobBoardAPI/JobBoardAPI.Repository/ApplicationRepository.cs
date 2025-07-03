using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using JobBoardAPI.Model;
using JobBoardAPI.Repository.Common;

namespace JobBoardAPI.Repository
{
    public class ApplicationRepository : IApplicationRepository
    {
        private readonly string _connectionString;

        public ApplicationRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task<PagedResult<Application>> GetAllApplicationsAsync(int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 5;

            var applications = new List<Application>();
            var query = "SELECT id, user_id, job_id, cover_letter, applied_at FROM applications WHERE 1=1";
            var countQuery = "SELECT COUNT(*) FROM applications WHERE 1=1";

            query += " ORDER BY applied_at DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            var queryParameters = new List<NpgsqlParameter>
            {
                new NpgsqlParameter("@Offset", (page - 1) * pageSize),
                new NpgsqlParameter("@PageSize", pageSize)
            };

            int totalItems;
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                // Get total count
                using (var countCmd = new NpgsqlCommand(countQuery, conn))
                {
                    totalItems = Convert.ToInt32(await countCmd.ExecuteScalarAsync());
                }

                // Get paginated results
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddRange(queryParameters.ToArray());
                    

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            applications.Add(new Application
                            {
                                Id = reader.GetGuid(0),
                                UserId = reader.GetGuid(1),
                                JobId = reader.GetGuid(2),
                                CoverLetter = reader.IsDBNull(3) ? null : reader.GetString(3),
                                AppliedAt = reader.GetDateTime(4)
                            });
                        }
                    }
                }
            }

            return new PagedResult<Application>(applications, totalItems, page, pageSize);
        }

        public async Task<Application> GetApplicationByIdAsync(Guid id)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT id, user_id, job_id, cover_letter, applied_at FROM applications WHERE id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Application
                            {
                                Id = reader.GetGuid(0),
                                UserId = reader.GetGuid(1),
                                JobId = reader.GetGuid(2),
                                CoverLetter = reader.IsDBNull(3) ? null : reader.GetString(3),
                                AppliedAt = reader.GetDateTime(4)
                            };
                        }
                    }
                }
            }
            return null;
        }

        public async Task<IEnumerable<Application>> GetApplicationsByJobIdAsync(Guid jobId)
        {
            var applications = new List<Application>();
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT id, user_id, job_id, cover_letter, applied_at FROM applications WHERE job_id = @JobId", conn))
                {
                    cmd.Parameters.AddWithValue("@JobId", jobId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            applications.Add(new Application
                            {
                                Id = reader.GetGuid(0),
                                UserId = reader.GetGuid(1),
                                JobId = reader.GetGuid(2),
                                CoverLetter = reader.IsDBNull(3) ? null : reader.GetString(3),
                                AppliedAt = reader.GetDateTime(4)
                            });
                        }
                    }
                }
            }
            return applications;
        }

        public async Task<IEnumerable<Application>> GetApplicationsByUserIdAsync(Guid userId)
        {
            var applications = new List<Application>();
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT id, user_id, job_id, cover_letter, applied_at FROM applications WHERE user_id = @UserId", conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            applications.Add(new Application
                            {
                                Id = reader.GetGuid(0),
                                UserId = reader.GetGuid(1),
                                JobId = reader.GetGuid(2),
                                CoverLetter = reader.IsDBNull(3) ? null : reader.GetString(3),
                                AppliedAt = reader.GetDateTime(4)
                            });
                        }
                    }
                }
            }
            return applications;
        }

        public async Task<bool> ApplicationExistsAsync(Guid userId, Guid jobId)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT COUNT(1) FROM applications WHERE user_id = @UserId AND job_id = @JobId", conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@JobId", jobId);
                    var count = (long)await cmd.ExecuteScalarAsync();
                    return count > 0;
                }
            }
        }

        public async Task CreateApplicationAsync(Application application)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand("INSERT INTO applications (id, user_id, job_id, cover_letter, applied_at) VALUES (@Id, @UserId, @JobId, @CoverLetter, @AppliedAt)", conn))
                {
                    application.Id = Guid.NewGuid();
                    cmd.Parameters.AddWithValue("@Id", application.Id);
                    cmd.Parameters.AddWithValue("@UserId", application.UserId);
                    cmd.Parameters.AddWithValue("@JobId", application.JobId);
                    cmd.Parameters.AddWithValue("@CoverLetter", application.CoverLetter ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@AppliedAt", application.AppliedAt == default ? DateTime.UtcNow : application.AppliedAt);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<bool> UpdateApplicationAsync(Guid id, Application application)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand("UPDATE applications SET user_id = @UserId, job_id = @JobId, cover_letter = @CoverLetter WHERE id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@UserId", application.UserId);
                    cmd.Parameters.AddWithValue("@JobId", application.JobId);
                    cmd.Parameters.AddWithValue("@CoverLetter", application.CoverLetter ?? (object)DBNull.Value);
                    int affectedRows = await cmd.ExecuteNonQueryAsync();
                    return affectedRows > 0;
                }
            }
        }

        public async Task DeleteApplicationAsync(Guid id)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand("DELETE FROM applications WHERE id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}