namespace Car_Rent_System.ViewModels.Admin
{
    public class DashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int PendingApprovals { get; set; }
        public int ActiveRentals { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public int TotalVehicles { get; set; }
        public int SupportTickets { get; set; }
        public List<string> RecentActivities { get; set; } = new();
    }
}