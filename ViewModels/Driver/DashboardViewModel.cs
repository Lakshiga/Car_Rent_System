using Car_Rent_System.ViewModels.Customer;

namespace Car_Rent_System.ViewModels.Driver
{
    public class DashboardViewModel
    {
        public string DriverName { get; set; } = "Driver";
        public int AssignedTrips { get; set; }
        public bool IsAvailable { get; set; }
        public string? CurrentVehicle { get; set; }
        public DateTime? NextTripTime { get; set; }
        public List<BookingViewModel> UpcomingTrips { get; set; }

    }
}