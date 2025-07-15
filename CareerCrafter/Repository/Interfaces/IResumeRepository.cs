using CareerCrafter.Models;

namespace CareerCrafter.Repositories.Interfaces
{
    public interface IResumeRepository
    {
        Task UploadResumeAsync(Resume resume);
        Task<Resume?> GetLatestResumeByUserIdAsync(int userId);
        Task<List<Resume>> GetAllResumesByUserIdAsync(int userId);
        Task<Resume?> GetResumeByIdAndUserAsync(int id, int userId);
        Task<bool> SoftDeleteResumeAsync(Resume resume);
    }
}
