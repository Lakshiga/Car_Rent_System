using Stripe;
using Car_Rent_System.DTOs;
using Microsoft.Extensions.Configuration;
using Stripe.Checkout;

namespace Car_Rent_System.Services
{
    public class StripeService : IStripeService
    {
        private readonly IConfiguration _config;

        public StripeService(IConfiguration config)
        {
            _config = config;
            var envSecret = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY");
            var secret = string.IsNullOrWhiteSpace(envSecret) ? _config["Stripe:SecretKey"] : envSecret;
            if (string.IsNullOrWhiteSpace(secret))
            {
                throw new InvalidOperationException("Stripe secret key is not configured. Set STRIPE_SECRET_KEY in .env or Stripe:SecretKey in appsettings.");
            }
            StripeConfiguration.ApiKey = secret;
        }

        public async Task<string> CreateCheckoutSessionAsync(BookingDto bookingDto, string carName)
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "usd",
                            UnitAmount = (long)(bookingDto.TotalCost * 100),
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Car Rental: {carName}",
                                Description = $"{bookingDto.PickupDate:yyyy-MM-dd} to {bookingDto.ReturnDate:yyyy-MM-dd}"
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = _config["AppUrl"] + "/Customer/BookingSuccess?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = _config["AppUrl"] + "/Customer/BookingCancelled",
                Metadata = new Dictionary<string, string>
                {
                    { "booking_id", bookingDto.BookingID.ToString() },
                    { "customer_id", bookingDto.CustomerID },
                    { "car_id", bookingDto.CarID.ToString() }
                }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);
            return session.Id;
        }
    }
}