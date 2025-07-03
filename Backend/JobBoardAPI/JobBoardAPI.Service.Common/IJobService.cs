using JobBoardAPI.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JobBoardAPI.Service.Common
{
    public interface IJobService
    {
        Task<PagedResult<Job>> GetAllJobsAsync(string? location, string? jobType, int page, int pageSize);
        Task<Job> GetJobByIdAsync(Guid id);
        Task<IEnumerable<Job>> GetJobsByEmployerIdAsync(Guid employerId);
        Task CreateJobAsync(Job job, Guid userId);
        Task<bool> UpdateJobAsync(Guid id, Job job, Guid userId);
        Task DeleteJobAsync(Guid id, Guid userId);
    }
}