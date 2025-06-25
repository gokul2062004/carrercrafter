using Microsoft.EntityFrameworkCore;
using CareerCrafter.Models;

namespace CareerCrafter.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Job> Jobs => Set<Job>();
        public DbSet<Application> Applications => Set<Application>();
        public DbSet<Resume> Resumes => Set<Resume>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ✅ Application → JobSeeker (disable cascade to prevent cycle)
            modelBuilder.Entity<Application>()
                .HasOne(a => a.JobSeeker)
                .WithMany()
                .HasForeignKey(a => a.JobSeekerId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ Job → Employer (safe to cascade)
            modelBuilder.Entity<Job>()
                .HasOne(j => j.Employer)
                .WithMany()
                .HasForeignKey(j => j.EmployerId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ Resume → User (optional, restrict delete)
            modelBuilder.Entity<Resume>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict); // prevents accidental delete of resumes
        }
    }
}
