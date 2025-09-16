namespace Car_Rent_System.DTOs
{
    public class CarPreferenceDto
    {
        public string CarType { get; set; } = string.Empty;
        public int Count { get; set; }
        public int BookingCount { get; set; }
        public decimal TotalSpent { get; set; }
    }
}