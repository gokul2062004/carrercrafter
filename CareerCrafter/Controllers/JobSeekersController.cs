using CareerCrafter.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareerCrafter.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobSeekersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public JobSeekersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ GET: /api/JobSeekers/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "JobSeeker")]
        public IActionResult GetProfile(int id)
        {
            try
            {
                if (id != GetCurrentUserId())
                    return Forbid("You can only view your own profile.");

                var user = _context.Users.FirstOrDefault(u => u.Id == id && u.Role == "JobSeeker");
                if (user == null)
                    return NotFound("Job seeker not found.");

                return Ok(new
                {
                    user.Id,
                    user.Email,
                    user.Role
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving the profile: {ex.Message}");
            }
        }

        // ✅ PUT: /api/JobSeekers/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> UpdateProfile(int id, [FromBody] string newEmail)
        {
            try
            {
                if (id != GetCurrentUserId())
                    return Forbid("You can only update your own profile.");

                var user = await _context.Users.FindAsync(id);
                if (user == null || user.Role != "JobSeeker")
                    return NotFound("Job seeker not found.");

                user.Email = newEmail;
                await _context.SaveChangesAsync();

                return Ok("Profile updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while updating the profile: {ex.Message}");
            }
        }

        // ✅ DELETE: /api/JobSeekers/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> DeleteProfile(int id)
        {
            try
            {
                if (id != GetCurrentUserId())
                    return Forbid("You can only delete your own account.");

                var user = await _context.Users.FindAsync(id);
                if (user == null || user.Role != "JobSeeker")
                    return NotFound("Job seeker not found.");

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return Ok("Profile deleted.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while deleting the profile: {ex.Message}");
            }
        }

        // ✅ GET: /api/JobSeekers/{id}/applications
        [HttpGet("{id}/applications")]
        [Authorize(Roles = "JobSeeker")]
        public IActionResult GetMyApplications(int id)
        {
            try
            {
                if (id != GetCurrentUserId())
                    return Forbid("You can only view your own applications.");

                var applications = (from app in _context.Applications
                                    where app.JobSeekerId == id
                                    join job in _context.Jobs on app.JobId equals job.Id
                                    select new
                                    {
                                        app.Id,
                                        job.Title,
                                        job.CompanyName,
                                        app.AppliedDate,
                                        app.Status
                                    }).ToList();

                return Ok(applications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while fetching applications: {ex.Message}");
            }
        }

        // ✅ Get user ID from JWT token
        private int GetCurrentUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            return int.TryParse(userIdClaim?.Value, out int userId) ? userId : 0;
        }
    }
}
