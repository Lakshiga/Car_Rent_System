using System.ComponentModel.DataAnnotations;
using Car_Rent_System.Enums;

namespace Car_Rent_System.DTOs
{
    public class BookingDto
    {
        public int BookingID { get; set; }
        public string CustomerID { get; set; }
        public int CarID { get; set; }
        public DateTime PickupDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public DateTime BookingDate { get; set; }
        public decimal TotalCost { get; set; }
        public BookingStatus BookingStatus { get; set; }
        public string? PickupLocation { get; set; }
        public string? ReturnLocation { get; set; }
        public string? SpecialRequirements { get; set; }
        public string? StripeSessionId { get; set; }
        public string? CarName { get; internal set; }
        public string? UserName { get; internal set; }
        public bool WithDriver { get; set; } // Added this property to fix CS0117
    }
}