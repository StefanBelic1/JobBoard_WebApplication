using AutoMapper;
using JobBoardAPI.Model;
using JobBoardAPI.RestModels;
using JobBoardAPI.Service.Common;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JobBoardAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobController : ControllerBase
    {
        private readonly IJobService _jobService;
        private readonly IMapper _mapper;

        public JobController(IJobService jobService, IMapper mapper)
        {
            _jobService = jobService ?? throw new ArgumentNullException(nameof(jobService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet("get-jobs")]
        public async Task<IActionResult> GetAllJobsAsync([FromQuery] string? location, [FromQuery] string? jobType, [FromQuery] int page = 1, [FromQuery] int pageSize = 5)
        {
            var pagedJobs = await _jobService.GetAllJobsAsync(location, jobType, page, pageSize);
            var jobsRest = _mapper.Map<IEnumerable<JobREST>>(pagedJobs.Items);
            return Ok(new
            {
                Items = jobsRest,
                pagedJobs.TotalItems,
                pagedJobs.Page,
                pagedJobs.PageSize,
                pagedJobs.TotalPages
            });
        }

        [HttpGet("get-job/{id}")]
        public async Task<ActionResult<JobREST>> GetJobAsync(Guid id)
        {
            var job = await _jobService.GetJobByIdAsync(id);
            if (job == null)
                return NotFound();

            var jobRest = _mapper.Map<JobREST>(job);
            return Ok(jobRest);
        }

        [HttpGet("get-jobs/employer/{employerId}")]
        public async Task<IActionResult> GetJobsByEmployerIdAsync(Guid employerId)
        {
            var jobs = await _jobService.GetJobsByEmployerIdAsync(employerId);
            var jobsRest = _mapper.Map<IEnumerable<JobREST>>(jobs);
            return Ok(jobsRest);
        }

        [HttpPost("create-job")]
        public async Task<IActionResult> CreateJobAsync([FromBody] JobREST jobRest, [FromHeader(Name = "User-Id")] Guid userId)
        {
            if (jobRest == null)
                return BadRequest("Job cannot be null.");

            var job = _mapper.Map<Job>(jobRest);
            await _jobService.CreateJobAsync(job, userId);
            return Ok();
        }

        [HttpPut("update-job/{id}")]
        public async Task<IActionResult> UpdateJobAsync(Guid id, [FromBody] JobREST jobRest, [FromHeader(Name = "User-Id")] Guid userId)
        {
            if (jobRest == null)
                return BadRequest("Job cannot be null.");

            var job = _mapper.Map<Job>(jobRest);
            bool updated = await _jobService.UpdateJobAsync(id, job, userId);
            if (!updated)
                return NotFound("Job not found or not updated.");

            return Ok("Job updated.");
        }

        [HttpDelete("delete-job/{id}")]
        public async Task<IActionResult> DeleteJobAsync(Guid id, [FromHeader(Name = "User-Id")] Guid userId)
        {
            await _jobService.DeleteJobAsync(id, userId);
            return NoContent();
        }
    }
}