using JobBoardAPI.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JobBoardAPI.Repository.Common
{
    public interface IJobRepository
    {
        Task<PagedResult<Job>> GetAllJobsAsync(string? location, string? jobType, int page, int pageSize);
        Task<Job> GetJobByIdAsync(Guid id);
        Task<IEnumerable<Job>> GetJobsByEmployerIdAsync(Guid employerId);
        Task CreateJobAsync(Job job);
        Task<bool> UpdateJobAsync(Guid id, Job job);
        Task DeleteJobAsync(Guid id);
    }
}