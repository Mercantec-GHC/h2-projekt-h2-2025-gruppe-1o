namespace DomainModels.DTOs
{
    public class RoomTypeCardDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int AvailableCount { get; set; }
    }
}