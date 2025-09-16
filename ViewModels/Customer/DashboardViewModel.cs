using Car_Rent_System.ViewModels.Car;
using Car_Rent_System.ViewModels.Booking;
using Car_Rent_System.DTOs;

namespace Car_Rent_System.ViewModels.Customer
{
    public class DashboardViewModel
    {
        public string WelcomeMessage { get; set; } = "Welcome back!";
        public int TotalBookings { get; set; }
        public int ActiveBookings { get; set; }
        public int CompletedTrips { get; set; }
        public decimal TotalSpent { get; set; }
        public List<CarSummaryViewModel> RecommendedVehicles { get; set; } = new();
        public List<Car_Rent_System.ViewModels.Booking.BookingViewModel> RecentBookings { get; set; } = new();
        public List<MonthlySpendingViewModel> MonthlySpending { get; set; } = new();
        public List<CarPreferenceDto> CarPreferences { get; set; } = new();
    }
}