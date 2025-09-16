namespace Car_Rent_System.ViewModels.Booking
{
    public class MonthlySpendingViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName => new DateTime(Year, Month, 1).ToString("MMMM");
        public decimal TotalSpent { get; set; }
    }
}