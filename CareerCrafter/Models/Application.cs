namespace CareerCrafter.Models
{
    public class Application
    {
        public int Id { get; set; }

        public int JobId { get; set; }

        public Job? Job { get; set; }

        public int JobSeekerId { get; set; }

        public User? JobSeeker { get; set; }

        public DateTime AppliedDate { get; set; } = DateTime.Now;

        public string Status { get; set; } = "Pending"; // Pending, Accepted, Rejected
    }
}
