namespace Car_Rent_System.DTOs
{
    public class UpdateBookingStatusRequest
    {
        public int BookingId { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
