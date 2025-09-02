using System.ComponentModel.DataAnnotations;

namespace DomainModels
{
    public class Service : Common
    {
        public int Id { get; set; } 
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string BillingType { get; set; } = string.Empty; // Tilføj = string.Empty;
    }
}