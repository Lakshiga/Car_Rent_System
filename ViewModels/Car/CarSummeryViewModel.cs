namespace Car_Rent_System.ViewModels.Car
{
    public class CarSummaryViewModel
    {
        public int Id { get; set; }
        public string DisplayName { get; set; } = "Vehicle";
        public string? ImageUrl { get; set; }
        public decimal DailyRate { get; set; }
        public bool IsAvailable { get; set; }
    }
}