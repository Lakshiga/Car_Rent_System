using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Car_Rent_System.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public int PaymentMethodId { get; set; }
        public string Currency { get; set; } = "USD";
        public string? StripePaymentIntentId { get; set; }
        public string? StripeSessionId { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }

        // Navigation properties
        [ForeignKey("PaymentMethodId")]
        public virtual PaymentMethod PaymentMethod { get; set; } = null!;

        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; } = null!;

        // ❗ NEVER store raw card details in DB — mark as [NotMapped]
        [NotMapped]
        public object CardNumber { get; internal set; }

        [NotMapped]
        public object Cvc { get; internal set; }

        // ❗ NEVER store raw card details in DB
        // public string? CardNumber { get; set; }
        // public string? Cvc { get; set; }
    }
}