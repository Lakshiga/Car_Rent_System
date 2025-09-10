namespace Car_Rent_System.Models
{
    public class Rental
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public string CarDisplay { get; set; } // e.g., "Tesla Model 3"
        public DateTime StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        public string Status { get; set; } // Active, Completed, Pending
        public decimal TotalPrice { get; set; }
    }
}
