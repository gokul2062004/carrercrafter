using CareerCrafter.Data;
using CareerCrafter.DTOs;
using CareerCrafter.Models;
using CareerCrafter.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CareerCrafter.Repositories.Implementations
{
    public class ApplicationRepository : IApplicationRepository
    {
        private readonly ApplicationDbContext _context;

        public ApplicationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Job?> GetJobByIdAsync(int jobId)
        {
            try
            {
                return await _context.Jobs.FirstOrDefaultAsync(j => j.Id == jobId);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching job by ID.", ex);
            }
        }

        public async Task<bool> HasAlreadyAppliedAsync(int jobId, int jobSeekerId)
        {
            try
            {
                return await _context.Applications
                    .AnyAsync(a => a.JobId == jobId && a.JobSeekerId == jobSeekerId);
            }
            catch (Exception ex)
            {
                throw new Exception("Error checking application status.", ex);
            }
        }

        public async Task AddApplicationAsync(Application application)
        {
            try
            {
                await _context.Applications.AddAsync(application);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding application.", ex);
            }
        }

        public async Task<List<ApplicantDto>> GetApplicantsForJobAsync(int jobId, int employerId, HttpRequest request)
        {
            try
            {
                var job = await _context.Jobs
                    .FirstOrDefaultAsync(j => j.Id == jobId && j.EmployerId == employerId);

                if (job == null)
                    return new List<ApplicantDto>();

                var applicants = await (
                    from a in _context.Applications
                    where a.JobId == jobId
                    join u in _context.Users on a.JobSeekerId equals u.Id
                    join r in _context.Resumes on u.Id equals r.UserId into resumeGroup
                    from resume in resumeGroup.DefaultIfEmpty()
                    select new ApplicantDto
                    {
                        ApplicationId = a.Id,
                        JobTitle = job.Title,
                        ApplicantEmail = u.Email,
                        AppliedDate = a.AppliedDate,
                        Status = a.Status,
                        ResumeLink = resume != null ? $"{request.Scheme}://{request.Host}{resume.FilePath}" : null
                    }).ToListAsync();

                return applicants;
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching applicants for job.", ex);
            }
        }

        public async Task<Application?> GetApplicationWithJobAsync(int applicationId, int employerId)
        {
            try
            {
                return await _context.Applications
                    .Include(a => a.Job)
                    .FirstOrDefaultAsync(a => a.Id == applicationId && a.Job.EmployerId == employerId);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching application with job details.", ex);
            }
        }

        public async Task<Application?> GetApplicationByIdAndSeekerAsync(int applicationId, int jobSeekerId)
        {
            try
            {
                return await _context.Applications
                    .FirstOrDefaultAsync(a => a.Id == applicationId && a.JobSeekerId == jobSeekerId);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching application by seeker ID.", ex);
            }
        }

        public async Task UpdateStatusAsync(Application application, string status)
        {
            try
            {
                application.Status = status;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating application status.", ex);
            }
        }

        public async Task RemoveApplicationAsync(Application application)
        {
            try
            {
                _context.Applications.Remove(application);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error removing application.", ex);
            }
        }
    }
}
