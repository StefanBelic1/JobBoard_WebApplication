using JobBoardAPI.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JobBoardAPI.Service.Common
{
    public interface IApplicationService
    {
        Task<PagedResult<Application>> GetAllApplicationsAsync(int page, int pageSize);
        Task<Application> GetApplicationByIdAsync(Guid id);
        Task<IEnumerable<Application>> GetApplicationsByJobIdAsync(Guid jobId, Guid userId);
        Task<IEnumerable<Application>> GetApplicationsByUserIdAsync(Guid userId);
        Task CreateApplicationAsync(Application application, Guid userId);
        Task<bool> UpdateApplicationAsync(Guid id, Application application, Guid userId);
        Task DeleteApplicationAsync(Guid id, Guid userId);
    }
}