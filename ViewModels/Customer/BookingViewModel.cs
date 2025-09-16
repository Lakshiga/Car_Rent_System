using System.ComponentModel.DataAnnotations;

namespace Car_Rent_System.ViewModels.Customer
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

        [Required]
        public string PickupLocation { get; set; } = string.Empty;

        public string? ReturnLocation { get; set; }

        public bool WithDriver { get; set; }

        public decimal TotalAmount { get; set; }

        public string? Status { get; set; }

        public string? VehicleName { get; set; }
        public string? VehicleImageUrl { get; set; }
    }
}