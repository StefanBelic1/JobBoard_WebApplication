using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JobBoardAPI.Model;
using JobBoardAPI.Repository.Common;
using JobBoardAPI.Service.Common;

namespace JobBoardAPI.Service
{
    public class JobService : IJobService
    {
        private readonly IJobRepository _jobRepository;
        private readonly IUserRepository _userRepository;

        public JobService(IJobRepository jobRepository, IUserRepository userRepository)
        {
            _jobRepository = jobRepository ?? throw new ArgumentNullException(nameof(jobRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<PagedResult<Job>> GetAllJobsAsync(string? location, string? jobType, int page, int pageSize)
        {
            return await _jobRepository.GetAllJobsAsync(location, jobType, page, pageSize);
        }

        public async Task<Job> GetJobByIdAsync(Guid id)
        {
            return await _jobRepository.GetJobByIdAsync(id);
        }

        public async Task<IEnumerable<Job>> GetJobsByEmployerIdAsync(Guid employerId)
        {
            return await _jobRepository.GetJobsByEmployerIdAsync(employerId);
        }

        public async Task CreateJobAsync(Job job, Guid userId)
        {
            if (string.IsNullOrEmpty(job.Title))
                throw new ArgumentException("Job title is required.");
            if (string.IsNullOrEmpty(job.Description))
                throw new ArgumentException("Job description is required.");
            if (string.IsNullOrEmpty(job.JobType) ||
                (job.JobType != "remote" && job.JobType != "full-time" && job.JobType != "part-time"))
                throw new ArgumentException("Job type must be 'remote', 'full-time', or 'part-time'.");

            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null || user.Role != "employer")
                throw new InvalidOperationException("Only employers can create jobs.");

            job.EmployerId = userId;
            await _jobRepository.CreateJobAsync(job);
        }

        public async Task<bool> UpdateJobAsync(Guid id, Job job, Guid userId)
        {
            if (string.IsNullOrEmpty(job.Title))
                throw new ArgumentException("Job title is required.");
            if (string.IsNullOrEmpty(job.Description))
                throw new ArgumentException("Job description is required.");
            if (string.IsNullOrEmpty(job.JobType) ||
                (job.JobType != "remote" && job.JobType != "full-time" && job.JobType != "part-time"))
                throw new ArgumentException("Job type must be 'remote', 'full-time', or 'part-time'.");

            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null || user.Role != "employer")
                throw new InvalidOperationException("Only employers can update jobs.");

            var existingJob = await _jobRepository.GetJobByIdAsync(id);
            if (existingJob == null || existingJob.EmployerId != userId)
                return false;

            job.EmployerId = userId;
            return await _jobRepository.UpdateJobAsync(id, job);
        }

        public async Task DeleteJobAsync(Guid id, Guid userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null || user.Role != "employer")
                throw new InvalidOperationException("Only employers can delete jobs.");

            var job = await _jobRepository.GetJobByIdAsync(id);
            if (job == null || job.EmployerId != userId)
                throw new InvalidOperationException("Job not found or not owned by this employer.");

            await _jobRepository.DeleteJobAsync(id);
        }
    }
}