namespace Car_Rent_System.ViewModels
{
    public class RentalActivityViewModel
    {
        public string CustomerName { get; set; }
        public string CarDisplay { get; set; }
        public string Status { get; set; }  // Active, Completed, Pending
        public string AgoText { get; set; } // "2 hours ago"
    }

    public class FleetPerformanceViewModel
    {
        public int Available { get; set; }
        public int Rented { get; set; }
        public int Maintenance { get; set; }
    }

    public class AdminDashboardViewModel
    {
        public decimal TotalRevenue { get; set; }
        public int ActiveRentals { get; set; }
        public int TotalCustomers { get; set; }
        public double FleetUtilizationPercent { get; set; }

        public List<RentalActivityViewModel> RecentRentals { get; set; }
        public FleetPerformanceViewModel FleetPerformance { get; set; }
    }
}
