using CareerCrafter.Data;
using CareerCrafter.DTOs;
using Microsoft.AspNetCore.Mvc;

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

        [HttpPost("jobs")]
        public IActionResult SearchJobs([FromBody] JobSearchDto dto)
        {
            try
            {
                var query = _context.Jobs.AsQueryable();

                if (!string.IsNullOrWhiteSpace(dto.Title))
                    query = query.Where(j => j.Title.Contains(dto.Title));

                if (!string.IsNullOrWhiteSpace(dto.Location))
                    query = query.Where(j => j.Location.Contains(dto.Location));

                if (dto.MinSalary.HasValue)
                    query = query.Where(j => j.Salary >= dto.MinSalary);

                if (dto.MaxSalary.HasValue)
                    query = query.Where(j => j.Salary <= dto.MaxSalary);

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
