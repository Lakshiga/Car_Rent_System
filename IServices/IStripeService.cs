using Car_Rent_System.DTOs;

namespace Car_Rent_System.Services
{
    public interface IStripeService
    {
        Task<string> CreateCheckoutSessionAsync(BookingDto bookingDto, string carName);
    }
}