namespace CareerCrafter.DTOs
{
    public class JobDto
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string CompanyName { get; set; } = null!;
        public decimal Salary { get; set; }
    }
}
