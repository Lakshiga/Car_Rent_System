namespace Car_Rent_System.DTOs
{
    public class ProcessRentRequest
    {
        public int BookingId { get; set; }
        public int OdometerStartReading { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal PartialPayment { get; set; }
    }
}
