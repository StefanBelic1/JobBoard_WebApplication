using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JobBoardAPI.Model;
using JobBoardAPI.Repository.Common;
using JobBoardAPI.Service.Common;

namespace JobBoardAPI.Service
{
    public class ApplicationService : IApplicationService
    {
        private readonly IApplicationRepository _applicationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IJobRepository _jobRepository;

        public ApplicationService(IApplicationRepository applicationRepository, IUserRepository userRepository, IJobRepository jobRepository)
        {
            _applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _jobRepository = jobRepository ?? throw new ArgumentNullException(nameof(jobRepository));
        }

        public async Task<PagedResult<Application>> GetAllApplicationsAsync(int page, int pageSize)
        {
            return await _applicationRepository.GetAllApplicationsAsync(page, pageSize);
        }

        public async Task<Application> GetApplicationByIdAsync(Guid id)
        {
            return await _applicationRepository.GetApplicationByIdAsync(id);
        }

        public async Task<IEnumerable<Application>> GetApplicationsByJobIdAsync(Guid jobId, Guid userId)
        {
            var job = await _jobRepository.GetJobByIdAsync(jobId);
            if (job == null)
                throw new InvalidOperationException("Job not found.");

            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null || (user.Role != "employer" || job.EmployerId != userId))
                throw new InvalidOperationException("Only the employer who posted the job can view its applications.");

            return await _applicationRepository.GetApplicationsByJobIdAsync(jobId);
        }

        public async Task<IEnumerable<Application>> GetApplicationsByUserIdAsync(Guid userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null || user.Role != "candidate")
                throw new InvalidOperationException("Only candidates can view their own applications.");

            return await _applicationRepository.GetApplicationsByUserIdAsync(userId);
        }

        public async Task CreateApplicationAsync(Application application, Guid userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null || user.Role != "candidate")
                throw new InvalidOperationException("Only candidates can apply for jobs.");

            var job = await _jobRepository.GetJobByIdAsync(application.JobId);
            if (job == null)
                throw new InvalidOperationException("Job not found.");

            if (await _applicationRepository.ApplicationExistsAsync(userId, application.JobId))
                throw new InvalidOperationException("User has already applied for this job.");

            application.UserId = userId;
            await _applicationRepository.CreateApplicationAsync(application);
        }

        public async Task<bool> UpdateApplicationAsync(Guid id, Application application, Guid userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null || user.Role != "candidate")
                throw new InvalidOperationException("Only candidates can update their applications.");

            var existingApplication = await _applicationRepository.GetApplicationByIdAsync(id);
            if (existingApplication == null || existingApplication.UserId != userId)
                return false;

            var job = await _jobRepository.GetJobByIdAsync(application.JobId);
            if (job == null)
                throw new InvalidOperationException("Job not found.");

            if (existingApplication.JobId != application.JobId || existingApplication.UserId != application.UserId)
            {
                if (await _applicationRepository.ApplicationExistsAsync(application.UserId, application.JobId))
                    throw new InvalidOperationException("User has already applied for this job.");
            }

            application.UserId = userId;
            return await _applicationRepository.UpdateApplicationAsync(id, application);
        }

        public async Task DeleteApplicationAsync(Guid id, Guid userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null || user.Role != "candidate")
                throw new InvalidOperationException("Only candidates can delete their applications.");

            var application = await _applicationRepository.GetApplicationByIdAsync(id);
            if (application == null || application.UserId != userId)
                throw new InvalidOperationException("Application not found or not owned by this user.");

            await _applicationRepository.DeleteApplicationAsync(id);
        }
    }
}