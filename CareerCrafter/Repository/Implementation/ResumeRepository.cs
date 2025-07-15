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
            await _context.Resumes.AddAsync(resume);
            await _context.SaveChangesAsync();
        }

        public async Task<Resume?> GetLatestResumeByUserIdAsync(int userId)
        {
            return await _context.Resumes
                .Where(r => r.UserId == userId && !r.IsDeleted)
                .OrderByDescending(r => r.UploadedDate)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Resume>> GetAllResumesByUserIdAsync(int userId)
        {
            return await _context.Resumes
                .Where(r => r.UserId == userId && !r.IsDeleted)
                .OrderByDescending(r => r.UploadedDate)
                .ToListAsync();
        }

        public async Task<Resume?> GetResumeByIdAndUserAsync(int id, int userId)
        {
            return await _context.Resumes
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId && !r.IsDeleted);
        }

        public async Task<bool> SoftDeleteResumeAsync(Resume resume)
        {
            bool usedInApplications = await _context.Applications
                .AnyAsync(a => a.ResumeId == resume.Id);

            if (usedInApplications)
                return false;

            resume.IsDeleted = true;
            _context.Resumes.Update(resume);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
