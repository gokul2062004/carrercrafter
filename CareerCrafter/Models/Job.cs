namespace CareerCrafter.Models
{
    public class Job
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string Location { get; set; } = null!;

        public string CompanyName { get; set; } = null!;

        public decimal Salary { get; set; }

        public DateTime PostedDate { get; set; } = DateTime.Now;

        public int EmployerId { get; set; }  // Foreign key reference to User (Employer)

        public User? Employer { get; set; }  // Navigation property
    }
}
