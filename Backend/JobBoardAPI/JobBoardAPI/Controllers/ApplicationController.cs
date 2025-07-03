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
    public class ApplicationController : ControllerBase
    {
        private readonly IApplicationService _applicationService;
        private readonly IJobService _jobService;
        private readonly IMapper _mapper;

        public ApplicationController(IApplicationService applicationService, IJobService jobService, IMapper mapper)
        {
            _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
            _jobService = jobService ?? throw new ArgumentNullException(nameof(jobService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet("get-applications")]
        public async Task<IActionResult> GetAllApplicationsAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 5)
        {
            var pagedApplications = await _applicationService.GetAllApplicationsAsync(page, pageSize);
            var applicationsRest = _mapper.Map<IEnumerable<ApplicationREST>>(pagedApplications.Items);
            return Ok(new
            {
                Items = applicationsRest,
                pagedApplications.TotalItems,
                pagedApplications.Page,
                pagedApplications.PageSize,
                pagedApplications.TotalPages
            });
        }

        [HttpGet("get-application/{id}")]
        public async Task<ActionResult<ApplicationREST>> GetApplicationAsync(Guid id)
        {
            var application = await _applicationService.GetApplicationByIdAsync(id);
            if (application == null)
                return NotFound();

            var applicationRest = _mapper.Map<ApplicationREST>(application);
            return Ok(applicationRest);
        }

        [HttpGet("get-applications/job/{jobId}")]
        public async Task<IActionResult> GetApplicationsByJobIdAsync(Guid jobId, [FromHeader(Name = "User-Id")] Guid userId)
        {
            var applications = await _applicationService.GetApplicationsByJobIdAsync(jobId, userId);
            var applicationsRest = new List<ApplicationREST>();
            foreach (var app in applications)
            {
                var appRest = _mapper.Map<ApplicationREST>(app);
                var job = await _jobService.GetJobByIdAsync(app.JobId);
                appRest.JobTitle = job?.Title;
                applicationsRest.Add(appRest);
            }
            return Ok(applicationsRest);
        }

        [HttpGet("get-applications/user")]
        public async Task<IActionResult> GetApplicationsByUserIdAsync([FromHeader(Name = "User-Id")] Guid userId)
        {
            var applications = await _applicationService.GetApplicationsByUserIdAsync(userId);
            var applicationsRest = new List<ApplicationREST>();
            foreach (var app in applications)
            {
                var appRest = _mapper.Map<ApplicationREST>(app);
                var job = await _jobService.GetJobByIdAsync(app.JobId);
                appRest.JobTitle = job?.Title;
                applicationsRest.Add(appRest);
            }
            return Ok(applicationsRest);
        }

        
        [HttpPost("create-application")]
        public async Task<IActionResult> CreateApplicationAsync([FromBody] ApplicationREST applicationRest, [FromHeader(Name = "User-Id")] Guid userId)
        {
            if (applicationRest == null)
                return BadRequest("Application cannot be null.");

            var application = _mapper.Map<Application>(applicationRest);
            await _applicationService.CreateApplicationAsync(application, userId);
            return Ok();
        }

        [HttpPut("update-application/{id}")]
        public async Task<IActionResult> UpdateApplicationAsync(Guid id, [FromBody] ApplicationREST applicationRest, [FromHeader(Name = "User-Id")] Guid userId)
        {
            if (applicationRest == null)
                return BadRequest("Application cannot be null.");

            var application = _mapper.Map<Application>(applicationRest);
            bool updated = await _applicationService.UpdateApplicationAsync(id, application, userId);
            if (!updated)
                return NotFound("Application not found or not updated.");

            return Ok("Application updated.");
        }

        [HttpDelete("delete-application/{id}")]
        public async Task<IActionResult> DeleteApplicationAsync(Guid id, [FromHeader(Name = "User-Id")] Guid userId)
        {
            await _applicationService.DeleteApplicationAsync(id, userId);
            return NoContent();
        }
    }
}