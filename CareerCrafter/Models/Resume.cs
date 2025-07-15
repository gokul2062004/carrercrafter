namespace CareerCrafter.Models
{
    public class Resume
    {
        public int Id { get; set; }
        public string FileName { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public int UserId { get; set; }
        public User? User { get; set; }
        public DateTime UploadedDate { get; set; } = DateTime.Now;

        // ✅ Soft delete flag
        public bool IsDeleted { get; set; } = false;
    }
}
