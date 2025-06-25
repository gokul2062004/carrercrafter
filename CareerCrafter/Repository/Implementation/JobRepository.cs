using CareerCrafter.Data;
using CareerCrafter.Models;
using CareerCrafter.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CareerCrafter.Repositories.Implementations
{
    public class JobRepository : IJobRepository
    {
        private readonly ApplicationDbContext _context;

        public JobRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ Add a new job
        public async Task AddJobAsync(Job job)
        {
            if (job == null)
                throw new ArgumentNullException(nameof(job), "Job object cannot be null.");

            try
            {
                await _context.Jobs.AddAsync(job);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("❌ Failed to add job to the database.", ex);
            }
        }

        // ✅ Get all jobs
        public async Task<IEnumerable<Job>> GetAllJobsAsync()
        {
            try
            {
                return await _context.Jobs.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("❌ Failed to retrieve job listings.", ex);
            }
        }

        // ✅ Get job by ID (missing method)
        public async Task<Job?> GetJobByIdAsync(int jobId)
        {
            try
            {
                return await _context.Jobs.FirstOrDefaultAsync(j => j.Id == jobId);
            }
            catch (Exception ex)
            {
                throw new Exception("❌ Failed to fetch job by ID.", ex);
            }
        }
    }
}
