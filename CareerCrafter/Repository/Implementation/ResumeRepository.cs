using CareerCrafter.Data;
using CareerCrafter.Models;
using CareerCrafter.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CareerCrafter.Repositories.Implementations
{
    public class ResumeRepository : IResumeRepository
    {
        private readonly ApplicationDbContext _context;

        public ResumeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task UploadResumeAsync(Resume resume)
        {
            try
            {
                await _context.Resumes.AddAsync(resume);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to upload resume.", ex);
            }
        }

        public async Task<Resume?> GetLatestResumeByUserIdAsync(int userId)
        {
            try
            {
                return await _context.Resumes
                    .Where(r => r.UserId == userId)
                    .OrderByDescending(r => r.UploadedDate)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to retrieve resume.", ex);
            }
        }

        public async Task<Resume?> GetResumeByIdAndUserAsync(int id, int userId)
        {
            try
            {
                return await _context.Resumes
                    .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to find resume.", ex);
            }
        }

        public async Task DeleteResumeAsync(Resume resume, string wwwRootPath)
        {
            try
            {
                var fullPath = Path.Combine(wwwRootPath, resume.FilePath.TrimStart('/'));
                if (File.Exists(fullPath))
                    File.Delete(fullPath);

                _context.Resumes.Remove(resume);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to delete resume.", ex);
            }
        }
    }
}
