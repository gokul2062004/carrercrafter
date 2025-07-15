using CareerCrafter.DTOs;
using CareerCrafter.Models;
using CareerCrafter.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareerCrafter.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResumesController : ControllerBase
    {
        private readonly IResumeRepository _resumeRepo;
        private readonly IWebHostEnvironment _env;

        public ResumesController(IResumeRepository resumeRepo, IWebHostEnvironment env)
        {
            _resumeRepo = resumeRepo;
            _env = env;
        }

        [HttpPost("upload")]
        [Authorize(Roles = "JobSeeker")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadResume([FromForm] ResumeDto dto)
        {
            try
            {
                var file = dto.ResumeFile;

                if (file == null || file.Length == 0)

                    return BadRequest("No file uploaded.");

                if (!file.FileName.EndsWith(".pdf"))
                    return BadRequest("Only PDF files are allowed.");

                int userId = GetCurrentUserId();
                var uploadsFolder = Path.Combine(
                    _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
                    "resumes");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var resume = new Resume
                {
                    FileName = file.FileName,
                    FilePath = $"/resumes/{uniqueFileName}",
                    UserId = userId,
                    UploadedDate = DateTime.UtcNow
                };

                await _resumeRepo.UploadResumeAsync(resume);

                return Ok("Resume uploaded successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error uploading resume: {ex.Message}");
            }
        }

        [HttpGet]
        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> GetAllMyResumes()
        {
            try
            {
                int userId = GetCurrentUserId();
                var resumes = await _resumeRepo.GetAllResumesByUserIdAsync(userId);

                if (resumes == null || !resumes.Any())
                    return NotFound("No resumes found.");

                var result = resumes.Select(r => new
                {
                    r.Id,
                    r.FileName,
                    r.UploadedDate,
                    ResumeLink = $"{Request.Scheme}://{Request.Host}{r.FilePath}"
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving resumes: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> DeleteResume(int id)
        {
            try
            {
                int userId = GetCurrentUserId();
                var resume = await _resumeRepo.GetResumeByIdAndUserAsync(id, userId);

                if (resume == null)
                    return NotFound("Resume not found or already deleted.");

                var deleted = await _resumeRepo.SoftDeleteResumeAsync(resume);
                if (!deleted)
                    return BadRequest("Resume is already used in job applications and cannot be deleted.");

                return Ok("Resume deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting resume: {ex.Message}");
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            return int.TryParse(userIdClaim?.Value, out int userId) ? userId : 0;
        }
    }
}
