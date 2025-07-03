using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using JobBoardAPI.Model;
using JobBoardAPI.Repository.Common;

namespace JobBoardAPI.Repository
{
    public class JobRepository : IJobRepository
    {
        private readonly string _connectionString;

        public JobRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task<PagedResult<Job>> GetAllJobsAsync(string? location, string? jobType, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 5; // Default to 5 if invalid

            var jobs = new List<Job>();
            var query = "SELECT id, title, description, location, job_type, category, posted_at, expires_at, employer_id FROM jobs WHERE 1=1";
            var countQuery = "SELECT COUNT(*) FROM jobs WHERE 1=1";
            var queryParameters = new List<NpgsqlParameter>();
            var countParameters = new List<NpgsqlParameter>();

            if (!string.IsNullOrEmpty(location))
            {
                query += " AND location = @Location";
                countQuery += " AND location = @Location";
                queryParameters.Add(new NpgsqlParameter("@Location", location));
                countParameters.Add(new NpgsqlParameter("@Location", location));
            }

            if (!string.IsNullOrEmpty(jobType))
            {
                query += " AND job_type = @JobType";
                countQuery += " AND job_type = @JobType";
                queryParameters.Add(new NpgsqlParameter("@JobType", jobType));
                countParameters.Add(new NpgsqlParameter("@JobType", jobType));
            }

            query += " ORDER BY posted_at DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            queryParameters.Add(new NpgsqlParameter("@Offset", (page - 1) * pageSize));
            queryParameters.Add(new NpgsqlParameter("@PageSize", pageSize));

            int totalItems;
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                // Get total count
                using (var countCmd = new NpgsqlCommand(countQuery, conn))
                {
                    countCmd.Parameters.AddRange(countParameters.ToArray());
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
                            jobs.Add(new Job
                            {
                                Id = reader.GetGuid(0),
                                Title = reader.GetString(1),
                                Description = reader.GetString(2),
                                Location = reader.IsDBNull(3) ? null : reader.GetString(3),
                                JobType = reader.GetString(4),
                                Category = reader.IsDBNull(5) ? null : reader.GetString(5),
                                PostedAt = reader.GetDateTime(6),
                                ExpiresAt = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                                EmployerId = reader.GetGuid(8)
                            });
                        }
                    }
                }
            }

            return new PagedResult<Job>(jobs, totalItems, page, pageSize);
        }

        public async Task<Job> GetJobByIdAsync(Guid id)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT id, title, description, location, job_type, category, posted_at, expires_at, employer_id FROM jobs WHERE id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Job
                            {
                                Id = reader.GetGuid(0),
                                Title = reader.GetString(1),
                                Description = reader.GetString(2),
                                Location = reader.IsDBNull(3) ? null : reader.GetString(3),
                                JobType = reader.GetString(4),
                                Category = reader.IsDBNull(5) ? null : reader.GetString(5),
                                PostedAt = reader.GetDateTime(6),
                                ExpiresAt = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                                EmployerId = reader.GetGuid(8)
                            };
                        }
                    }
                }
            }
            return null;
        }

        public async Task<IEnumerable<Job>> GetJobsByEmployerIdAsync(Guid employerId)
        {
            var jobs = new List<Job>();
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT id, title, description, location, job_type, category, posted_at, expires_at, employer_id FROM jobs WHERE employer_id = @EmployerId", conn))
                {
                    cmd.Parameters.AddWithValue("@EmployerId", employerId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            jobs.Add(new Job
                            {
                                Id = reader.GetGuid(0),
                                Title = reader.GetString(1),
                                Description = reader.GetString(2),
                                Location = reader.IsDBNull(3) ? null : reader.GetString(3),
                                JobType = reader.GetString(4),
                                Category = reader.IsDBNull(5) ? null : reader.GetString(5),
                                PostedAt = reader.GetDateTime(6),
                                ExpiresAt = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                                EmployerId = reader.GetGuid(8)
                            });
                        }
                    }
                }
            }
            return jobs;
        }

        public async Task CreateJobAsync(Job job)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand("INSERT INTO jobs (id, title, description, location, job_type, category, posted_at, expires_at, employer_id) VALUES (@Id, @Title, @Description, @Location, @JobType, @Category, @PostedAt, @ExpiresAt, @EmployerId)", conn))
                {
                    job.Id = Guid.NewGuid();
                    cmd.Parameters.AddWithValue("@Id", job.Id);
                    cmd.Parameters.AddWithValue("@Title", job.Title);
                    cmd.Parameters.AddWithValue("@Description", job.Description);
                    cmd.Parameters.AddWithValue("@Location", job.Location ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@JobType", job.JobType);
                    cmd.Parameters.AddWithValue("@Category", job.Category ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@PostedAt", job.PostedAt == default ? DateTime.UtcNow : job.PostedAt);
                    cmd.Parameters.AddWithValue("@ExpiresAt", job.ExpiresAt ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EmployerId", job.EmployerId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<bool> UpdateJobAsync(Guid id, Job job)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand("UPDATE jobs SET title = @Title, description = @Description, location = @Location, job_type = @JobType, category = @Category, expires_at = @ExpiresAt, employer_id = @EmployerId WHERE id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@Title", job.Title);
                    cmd.Parameters.AddWithValue("@Description", job.Description);
                    cmd.Parameters.AddWithValue("@Location", job.Location ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@JobType", job.JobType);
                    cmd.Parameters.AddWithValue("@Category", job.Category ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ExpiresAt", job.ExpiresAt ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EmployerId", job.EmployerId);
                    int affectedRows = await cmd.ExecuteNonQueryAsync();
                    return affectedRows > 0;
                }
            }
        }

        public async Task DeleteJobAsync(Guid id)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand("DELETE FROM jobs WHERE id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}