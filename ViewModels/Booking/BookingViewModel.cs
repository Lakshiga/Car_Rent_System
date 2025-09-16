using System.ComponentModel.DataAnnotations;
using Car_Rent_System.Enums;

namespace Car_Rent_System.ViewModels.Booking
{
    public class BookingViewModel
    {
        public int Id { get; set; }

        [Required]
        public int VehicleId { get; set; }

        [Required]
        public DateTime PickupDate { get; set; }

        [Required]
        public DateTime ReturnDate { get; set; }

        [Required, StringLength(200)]
        public string PickupLocation { get; set; } = string.Empty;

        [StringLength(200)]
        public string? ReturnLocation { get; set; }

        // ✅ ADD THIS PROPERTY
        [StringLength(500)]
        public string? SpecialRequirements { get; set; }
        public bool? WithDriver { get; set; } // 👈 Nullable

        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        [StringLength(50)]
        public string? Status { get; set; } = "Pending";

        // For display
        public string? VehicleName { get; set; }
        public string? CustomerName { get; set; }
        public string? DriverName { get; set; }
        public string? CarName { get; set; }
        public string? UserName { get; set; }
        public string? StripeSessionId { get; set; }
        public DateTime BookingDate { get; set; }

        // Add these for dashboard compatibility:
        public Car_Rent_System.ViewModels.Car.CarViewModel? Car { get; set; } // For @booking.Car?.ImageUrl, etc.
        public decimal TotalCost { get; set; } // For @booking.TotalCost
        public BookingStatus BookingStatus { get; set; } // For @booking.BookingStatus

        // New booking details for enhanced flow
        [StringLength(50)]
        public string? LicenseNumber { get; set; }

        [StringLength(20)]
        public string? NICNumber { get; set; }

        [StringLength(500)]
        public string? DocumentImageUrl { get; set; }

        [StringLength(500)]
        public string? LicenseImageFrontUrl { get; set; }

        [StringLength(500)]
        public string? LicenseImageBackUrl { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Odometer reading must be a positive number")]
        public int? OdometerStartReading { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Odometer reading must be a positive number")]
        public int? OdometerEndReading { get; set; }

        public decimal? DistanceTraveled { get; set; }

        public decimal? PerKilometerRate { get; set; }

        public decimal? AdvancePayment { get; set; }

        public decimal? RemainingAmount { get; set; }
    }
}