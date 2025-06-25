// DTOs/ApplicantDto.cs
namespace CareerCrafter.DTOs
{
    public class ApplicantDto
    {
        public int ApplicationId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string ApplicantEmail { get; set; } = string.Empty;
        public DateTime AppliedDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ResumeLink { get; set; }
    }
}
