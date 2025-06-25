using CareerCrafter.DTOs;
using CareerCrafter.Models;
using CareerCrafter.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareerCrafter.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController : ControllerBase
    {
        private readonly IJobRepository _jobRepository;

        public JobsController(IJobRepository jobRepository)
        {
            _jobRepository = jobRepository;
        }

        /// <summary>
        /// POST: /api/Jobs
        /// Allows only Employers to post new jobs.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> PostJob([FromBody] JobDto jobDto)
        {
            try
            {
                if (jobDto == null)
                    return BadRequest("❌ Invalid job data.");

                int employerId = GetCurrentUserId();
                if (employerId == 0)
                    return Unauthorized("❌ Invalid or missing user ID.");

                var job = new Job
                {
                    Title = jobDto.Title,
                    Description = jobDto.Description,
                    Location = jobDto.Location,
                    CompanyName = jobDto.CompanyName,
                    Salary = jobDto.Salary,
                    PostedDate = DateTime.UtcNow,
                    EmployerId = employerId
                };

                await _jobRepository.AddJobAsync(job);
                return Ok("✅ Job posted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"❌ An error occurred while posting the job: {ex.Message}");
            }
        }

        /// <summary>
        /// GET: /api/Jobs
        /// Open to all users - returns job listings.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetJobs()
        {
            try
            {
                var jobs = await _jobRepository.GetAllJobsAsync();

                var jobList = jobs.Select(j => new
                {
                    j.Id,
                    j.Title,
                    j.Description,
                    j.Location,
                    j.CompanyName,
                    j.Salary,
                    j.PostedDate
                });

                return Ok(jobList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"❌ An error occurred while retrieving jobs: {ex.Message}");
            }
        }

        // 🔐 Helper method: extract UserId from JWT token
        private int GetCurrentUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            return int.TryParse(userIdClaim?.Value, out int userId) ? userId : 0;
        }
    }
}
