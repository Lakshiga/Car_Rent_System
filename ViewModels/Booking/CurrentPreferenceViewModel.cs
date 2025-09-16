namespace Car_Rent_System.ViewModels.Booking
{
    public class CarPreferenceViewModel
    {
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int BookingCount { get; set; }
        public decimal TotalSpent { get; set; }
    }
}