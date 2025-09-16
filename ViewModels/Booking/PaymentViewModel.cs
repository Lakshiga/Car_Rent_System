using System.ComponentModel.DataAnnotations;

namespace Car_Rent_System.ViewModels.Booking
{
    public class PaymentViewModel
    {
        public int BookingId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string PaymentMethod { get; set; } = "CreditCard";

        [Required]
        public string Currency { get; set; } = "USD";

        public string? StripePaymentIntentId { get; set; }

        public string? Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Card details (for UI only — never store!)
        [CreditCard]
        public string? CardNumber { get; set; }

        [Range(1, 12)]
        public int? ExpiryMonth { get; set; }

        [Range(2023, 2035)]
        public int? ExpiryYear { get; set; }

        [StringLength(4)]
        public string? Cvc { get; set; }
    }
}