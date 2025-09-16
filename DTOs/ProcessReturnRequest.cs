namespace Car_Rent_System.DTOs
{
    public class ProcessReturnRequest
    {
        public int BookingId { get; set; }
        public DateTime ReturnDate { get; set; }
        public int OdometerEndReading { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
    }
}
