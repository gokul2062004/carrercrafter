using CareerCrafter.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareerCrafter.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EmployersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ GET: /api/Employers/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Employer")] // ⛔ Removed AllowAnonymous
        public IActionResult GetEmployer(int id)
        {
            try
            {
                // Ensure employer can only view their own profile
                if (id != GetCurrentUserId())
                    return Forbid("You can only view your own employer profile.");

                var employer = _context.Users.FirstOrDefault(u => u.Id == id && u.Role == "Employer");
                if (employer == null)
                    return NotFound("Employer not found.");

                return Ok(new
                {
                    employer.Id,
                    employer.Email,
                    employer.Role
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving the employer: {ex.Message}");
            }
        }

        // ✅ GET: /api/Employers/{id}/jobs
        [HttpGet("{id}/jobs")]
        [Authorize(Roles = "Employer")]
        public IActionResult GetEmployerJobs(int id)
        {
            try
            {
                if (id != GetCurrentUserId())
                    return Forbid("You can only view your own jobs.");

                var jobs = _context.Jobs
                    .Where(j => j.EmployerId == id)
                    .Select(j => new
                    {
                        j.Id,
                        j.Title,
                        j.Description,
                        j.Location,
                        j.CompanyName,
                        j.Salary,
                        j.PostedDate
                    })
                    .ToList();

                return Ok(jobs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while fetching jobs: {ex.Message}");
            }
        }

        // ✅ PUT: /api/Employers/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> UpdateEmployer(int id, [FromBody] string newEmail)
        {
            try
            {
                if (id != GetCurrentUserId())
                    return Forbid("You can only update your own profile.");

                var employer = await _context.Users.FindAsync(id);
                if (employer == null || employer.Role != "Employer")
                    return NotFound("Employer not found.");

                employer.Email = newEmail;
                await _context.SaveChangesAsync();

                return Ok("Employer profile updated.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while updating employer: {ex.Message}");
            }
        }

        // ✅ DELETE: /api/Employers/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> DeleteEmployer(int id)
        {
            try
            {
                if (id != GetCurrentUserId())
                    return Forbid("You can only delete your own account.");

                var employer = await _context.Users.FindAsync(id);
                if (employer == null || employer.Role != "Employer")
                    return NotFound("Employer not found.");

                _context.Users.Remove(employer);
                await _context.SaveChangesAsync();

                return Ok("Employer deleted.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while deleting employer: {ex.Message}");
            }
        }

        // ✅ DELETE: /api/Employers/{employerId}/jobs/{jobId}
        [HttpDelete("{employerId}/jobs/{jobId}")]
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> DeleteJob(int employerId, int jobId)
        {
            try
            {
                if (employerId != GetCurrentUserId())
                    return Forbid("You can only delete your own jobs.");

                var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == jobId && j.EmployerId == employerId);
                if (job == null)
                    return NotFound("Job not found or access denied.");

                _context.Jobs.Remove(job);
                await _context.SaveChangesAsync();

                return Ok("Job deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while deleting the job: {ex.Message}");
            }
        }

        // 🔐 Utility to get current user ID from token
        private int GetCurrentUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            return int.TryParse(userIdClaim?.Value, out int userId) ? userId : 0;
        }
    }
}
