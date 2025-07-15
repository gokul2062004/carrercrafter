using CareerCrafter.Data;
using CareerCrafter.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace CareerCrafter.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SearchController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🔍 POST: /api/Search/jobs
        [HttpPost("jobs")]
        public IActionResult SearchJobs([FromBody] JobSearchDto dto)
        {
            try
            {
                var query = _context.Jobs.AsQueryable();

                // 🔍 Unified search by keyword (Title, Location, CompanyName)
                if (!string.IsNullOrWhiteSpace(dto.Title))
                {
                    var keyword = dto.Title.ToLower();
                    query = query.Where(j =>
                        j.Title.ToLower().Contains(keyword) ||
                        j.Location.ToLower().Contains(keyword) ||
                        j.CompanyName.ToLower().Contains(keyword));
                }

                // 💰 Optional Salary filters
                if (dto.MinSalary.HasValue)
                    query = query.Where(j => j.Salary >= dto.MinSalary.Value);

                if (dto.MaxSalary.HasValue)
                    query = query.Where(j => j.Salary <= dto.MaxSalary.Value);

                // 📄 Convert to list
                var result = query.ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while searching for jobs: {ex.Message}");
            }
        }
    }
}
