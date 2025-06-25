using CareerCrafter.Models;

namespace CareerCrafter.Repositories.Interfaces
{
    public interface IJobRepository
    {
        Task<IEnumerable<Job>> GetAllJobsAsync();
        Task AddJobAsync(Job job);
        Task<Job?> GetJobByIdAsync(int jobId); // ✅ Add this line
    }
}
