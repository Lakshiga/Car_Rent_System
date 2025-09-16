using Car_Rent_System.ViewModels.Customer;

namespace Car_Rent_System.ViewModels.Staff
{
    public class DashboardViewModel
    {
        public int TodayCheckIns { get; set; }
        public int TodayCheckOuts { get; set; }
        public int PendingMaintenance { get; set; }
        public int OpenSupportTickets { get; set; }
        public List<BookingViewModel> TodaysBookings { get; set; } = new();
    }
}