using AutoMapper;
using Car_Rent_System.DTOs;
using Car_Rent_System.Enums;
using Car_Rent_System.Models;
using Car_Rent_System.ViewModels.Account;
using Car_Rent_System.ViewModels.Booking;
using Car_Rent_System.ViewModels.Car;

namespace Car_Rent_System.Mappers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ========== ACCOUNT ==========
            CreateMap<ApplicationUser, ProfileViewModel>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.LicenseNumber, opt => opt.MapFrom(src => src.LicenseNumber))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.CompanyName))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl));

            CreateMap<ProfileViewModel, ApplicationUser>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.LicenseNumber, opt => opt.MapFrom(src => src.LicenseNumber))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.CompanyName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore());

            // ========== VEHICLE ==========
            CreateMap<Car, CarViewModel>() // ✅ FIXED: Was "ViewModels.Vehicle"
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id)) // ✅ Map CarID to Id
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.IsAvailable ? "Available" : "Rented"));

            CreateMap<CarViewModel, Car>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id)) // ✅ Map Id to CarID
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore());

            CreateMap<Car, CarSummaryViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => $"{src.Year} {src.Make} {src.CarModel}"))
                .ForMember(dest => dest.DailyRate, opt => opt.MapFrom(src => src.DailyRate))
                .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.IsAvailable))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl));

            // Car to CarDto mapping
            CreateMap<Car, CarDto>()
                .ForMember(dest => dest.CarID, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CarName, opt => opt.MapFrom(src => src.CarName))
                .ForMember(dest => dest.CarModel, opt => opt.MapFrom(src => src.CarModel))
                .ForMember(dest => dest.CarType, opt => opt.MapFrom(src => src.CarType))
                .ForMember(dest => dest.DailyRate, opt => opt.MapFrom(src => src.DailyRate))
                .ForMember(dest => dest.PerKilometerRate, opt => opt.MapFrom(src => src.PerKilometerRate))
                .ForMember(dest => dest.FuelType, opt => opt.MapFrom(src => src.FuelType))
                .ForMember(dest => dest.Transmission, opt => opt.MapFrom(src => src.Transmission))
                .ForMember(dest => dest.SeatingCapacity, opt => opt.MapFrom(src => src.SeatingCapacity))
                .ForMember(dest => dest.Mileage, opt => opt.MapFrom(src => src.Mileage))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl))
                .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.IsAvailable))
                .ForMember(dest => dest.DateAdded, opt => opt.MapFrom(src => src.DateAdded));

            // ========== BOOKING ==========
            CreateMap<Booking, BookingViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.BookingID))
                .ForMember(dest => dest.VehicleId, opt => opt.MapFrom(src => src.CarID))
                .ForMember(dest => dest.VehicleName, opt => opt.MapFrom(src => src.Car.CarName))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.FullName))
                .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalCost))
                .ForMember(dest => dest.TotalCost, opt => opt.MapFrom(src => src.TotalCost))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.BookingStatus.ToString()))
                .ForMember(dest => dest.BookingStatus, opt => opt.MapFrom(src => src.BookingStatus))
                .ForMember(dest => dest.PickupDate, opt => opt.MapFrom(src => src.PickupDate))
                .ForMember(dest => dest.ReturnDate, opt => opt.MapFrom(src => src.ReturnDate))
                .ForMember(dest => dest.PickupLocation, opt => opt.MapFrom(src => src.PickupLocation))
                .ForMember(dest => dest.ReturnLocation, opt => opt.MapFrom(src => src.ReturnLocation))
                .ForMember(dest => dest.SpecialRequirements, opt => opt.MapFrom(src => src.SpecialRequirements))
                .ForMember(dest => dest.WithDriver, opt => opt.MapFrom(src => src.WithDriver))
                .ForMember(dest => dest.Car, opt => opt.MapFrom(src => src.Car));


            CreateMap<BookingViewModel, Booking>()
                .ForMember(dest => dest.BookingID, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CarID, opt => opt.MapFrom(src => src.VehicleId))
                .ForMember(dest => dest.TotalCost, opt => opt.MapFrom(src => src.TotalAmount))
                .ForMember(dest => dest.BookingStatus, opt => opt.MapFrom(src => Enum.Parse<BookingStatus>(src.Status)))
                .ForMember(dest => dest.CustomerID, opt => opt.Ignore())
                .ForMember(dest => dest.BookingDate, opt => opt.Ignore());

            // ========== PAYMENT ==========
            CreateMap<Payment, PaymentViewModel>()
                .ForMember(dest => dest.BookingId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.PaymentMethod))
                .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Currency))
                .ForMember(dest => dest.StripePaymentIntentId, opt => opt.MapFrom(src => src.StripePaymentIntentId))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));

            CreateMap<PaymentViewModel, Payment>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.BookingId))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.PaymentMethod))
                .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Currency))
                .ForMember(dest => dest.StripePaymentIntentId, opt => opt.MapFrom(src => src.StripePaymentIntentId))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.CardNumber, opt => opt.Ignore()) // Never store
                .ForMember(dest => dest.Cvc, opt => opt.Ignore());

            // ========== ANALYTICS ==========
            CreateMap<MonthlySpendingViewModel, MonthlySpendingViewModel>();
            CreateMap<CarPreferenceViewModel, CarPreferenceViewModel>();


            // ========== ADMIN ==========
            // (Populate from services, not direct mapping)

            // ========== CUSTOMER ==========
            // (Populate from services)

            // ========== DRIVER ==========
            // (Populate from services)

            // ========== OWNER ==========
            // (Populate from services)

            // ========== STAFF ==========
            // (Populate from services)
        }
    }
}