using CareerCrafter.Models;
using Microsoft.AspNetCore.Http;
using CareerCrafter.DTOs;

namespace CareerCrafter.Repositories.Interfaces
{
    public interface IApplicationRepository
    {
        Task<Job?> GetJobByIdAsync(int jobId);
        Task<bool> HasAlreadyAppliedAsync(int jobId, int jobSeekerId);
        Task AddApplicationAsync(Application application);
        Task<List<ApplicantDto>> GetApplicantsForJobAsync(int jobId, int employerId, HttpRequest request);
        Task<Application?> GetApplicationWithJobAsync(int applicationId, int employerId);
        Task<Application?> GetApplicationByIdAndSeekerAsync(int applicationId, int jobSeekerId);
        Task UpdateStatusAsync(Application application, string status);

        // ✅ Soft delete method
        Task RemoveApplicationAsync(Application application);

        // ✅ Extra method to persist any manual updates
        Task SaveChangesAsync();
    }
}
