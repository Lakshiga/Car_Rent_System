using System.ComponentModel.DataAnnotations;
using System.Security.Principal;

namespace Car_Rent_System.Models
{
    public class OwnDrivers
    {

/* Persinol Information*/
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        [StringLength(50, ErrorMessage = "First Name cannot exceed 50 characters")]
        [Display(Name = "First Name")]
        public string Fname { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        [StringLength(50, ErrorMessage = "Last Name cannot exceed 50 characters")]
        [Display(Name = "Last Name")]
        public string Lname { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Phone Number is required")]
        [StringLength(10,ErrorMessage ="Phone Number cannot exceed10 characters")]
        public int PhoneNumber { get; set; }

        [Required(ErrorMessage ="NIC No is required")]
        [StringLength(10,MinimumLength =10, ErrorMessage="NIC No is cannot exceed 20 characters")]
        [Display(Name = "National Identity Card Number")]
        public string NIC { get; set; }

        [Required(ErrorMessage ="L_No is required")]
        [StringLength(20, ErrorMessage = "L_No is cannot exceed 20 characters")]
        [Display(Name = "Driver’s License Number")]
        public string DriversLicense_No{ get; set; }

        [Required(ErrorMessage = "Experience is required")]
        [Range(0, 50, ErrorMessage = "Experience must be between 0 and 50 years")]
        [Display(Name = "Experience (Years)")]
        public int Experience { get; set; }


        //<!-- Earnings -->

        public decimal TodayEarnings { get; set; }
        public decimal WeeklyEarnings { get; set; }
        public decimal MonthlyEarnings { get; set; }
        public decimal PendingPayments { get; set; }

        //<!-- Current Trip -->

        public string CurrentTripCustomer { get; set; }
        public DateTime? TripStartTime { get; set; }
        public DateTime? TripEndTime { get; set; }
        public string PickupLocation { get; set; }
        public string DropLocation { get; set; }


        //Status

        public bool IsActive { get; set; }
        public bool ReadyForTrips { get; set; }


        //Reviews

        public double AverageRating { get; set; }
        public List<string> CustomerFeedback { get; set; } = new();

        //Notifications

        public List<string> Notifications { get; set; } = new();
    }
}
