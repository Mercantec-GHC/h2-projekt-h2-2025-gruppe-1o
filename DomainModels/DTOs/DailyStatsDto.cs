namespace DomainModels.DTOs
{
    public class DailyStatsDto
    {
        public int ArrivalsCount { get; set; }
        public int DeparturesCount { get; set; }
        public double OccupancyPercentage { get; set; }
        public decimal TodaysRevenue { get; set; }
    }
}