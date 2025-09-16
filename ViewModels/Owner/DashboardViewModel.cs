using Car_Rent_System.ViewModels.Car;

namespace Car_Rent_System.ViewModels.Owner
{
    public class DashboardViewModel
    {
        public string BusinessName { get; set; } = "My Fleet";
        public int TotalVehicles { get; set; }
        public int ActiveRentals { get; set; }
        public decimal MonthlyEarnings { get; set; }
        public int TotalDrivers { get; set; }
        public List<CarSummaryViewModel> Cars { get; set; } = new();
    }
}