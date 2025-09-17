namespace DomainModels.DTOs
{
    /// <summary>
    /// DTO til at vise en service i frontend.
    /// </summary>
    public class ServiceGetDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string BillingType { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }
}