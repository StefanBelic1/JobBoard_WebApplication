using JobBoardAPI.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JobBoardAPI.Repository.Common
{
    public interface IApplicationRepository
    {
        Task<PagedResult<Application>> GetAllApplicationsAsync(int page, int pageSize);
        Task<Application> GetApplicationByIdAsync(Guid id);
        Task<IEnumerable<Application>> GetApplicationsByJobIdAsync(Guid jobId);
        Task<IEnumerable<Application>> GetApplicationsByUserIdAsync(Guid userId);
        Task<bool> ApplicationExistsAsync(Guid userId, Guid jobId);
        Task CreateApplicationAsync(Application application);
        Task<bool> UpdateApplicationAsync(Guid id, Application application);
        Task DeleteApplicationAsync(Guid id);
    }
}