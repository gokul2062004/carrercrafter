using CareerCrafter.DTOs;
using CareerCrafter.Models;
using CareerCrafter.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareerCrafter.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApplicationsController : ControllerBase
    {
        private readonly IApplicationRepository _applicationRepo;
        private readonly IJobRepository _jobRepo;

        public ApplicationsController(IApplicationRepository applicationRepo, IJobRepository jobRepo)
        {
            _applicationRepo = applicationRepo;
            _jobRepo = jobRepo;
        }

        [HttpPost]
        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> ApplyForJob([FromBody] ApplicationDto dto)
        {
            try
            {
                int jobSeekerId = GetCurrentUserId();

                var job = await _jobRepo.GetJobByIdAsync(dto.JobId);
                if (job == null)
                    return NotFound("Job not found.");

                bool alreadyApplied = await _applicationRepo.HasAlreadyAppliedAsync(jobSeekerId, dto.JobId);
                if (alreadyApplied)
                    return BadRequest("You have already applied.");

                var application = new Application
                {
                    JobId = dto.JobId,
                    JobSeekerId = jobSeekerId,
                    ResumeId = dto.ResumeId,
                    AppliedDate = DateTime.UtcNow,
                    Status = "Pending",
                    IsDeleted = false
                };

                await _applicationRepo.AddApplicationAsync(application);
                return Ok("Application submitted.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while applying: {ex.Message}");
            }
        }

        [HttpGet("job/{jobId}/applicants")]
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> GetApplicantsForJob(int jobId)
        {
            try
            {
                int employerId = GetCurrentUserId();

                var applicants = await _applicationRepo.GetApplicantsForJobAsync(jobId, employerId, Request);
                if (applicants == null || !applicants.Any())
                    return NotFound("Job not found or no applicants.");

                return Ok(applicants);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while fetching applicants: {ex.Message}");
            }
        }

        [HttpPut("{applicationId}/status")]
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> UpdateStatus(int applicationId, [FromBody] UpdateApplicationStatusDto dto)
        {
            try
            {
                int employerId = GetCurrentUserId();

                var application = await _applicationRepo.GetApplicationWithJobAsync(applicationId, employerId);
                if (application == null || application.IsDeleted)
                    return NotFound("Not found or access denied.");

                await _applicationRepo.UpdateStatusAsync(application, dto.Status);
                return Ok("Application status updated.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while updating status: {ex.Message}");
            }
        }

        [HttpPatch("{applicationId}/status")]
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> PatchStatus(int applicationId, [FromBody] UpdateApplicationStatusDto dto)
        {
            return await UpdateStatus(applicationId, dto);
        }

        [HttpDelete("{applicationId}")]
        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> WithdrawApplication(int applicationId)
        {
            try
            {
                int jobSeekerId = GetCurrentUserId();

                var application = await _applicationRepo.GetApplicationByIdAndSeekerAsync(applicationId, jobSeekerId);
                if (application == null || application.IsDeleted)
                    return NotFound("Application not found or access denied.");

                application.IsDeleted = true;
                await _applicationRepo.SaveChangesAsync();

                return Ok("Application withdrawn successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while withdrawing: {ex.Message}");
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            return int.TryParse(userIdClaim?.Value, out int userId) ? userId : 0;
        }
    }
}
