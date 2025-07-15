namespace CareerCrafter.DTOs
{
    public class ApplicationDto
    {
        public int JobId { get; set; }
        public int? ResumeId { get; set; } // ✅ NEW
                                           //  public string Status { get; set; } = "Applied"; // default
    }
}
