namespace Car_Rent_System.Enums
{
    public enum Role
    {
        Admin,
        SubAdmin,
        Staff,
        Customer,
        Driver,
        VehicleOwner
    }

    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Cancelled,
        Completed,
        Rented,
        Rejected,
        Approved
    }

    public enum VerificationStatus
    {
        Pending,
        Approved,
        Rejected
    }


    public static class BookingStatusExtensions
    {
        public static string ToDisplayString(this BookingStatus status)
        {
            return status switch
            {
                BookingStatus.Pending => "Pending",
                BookingStatus.Confirmed => "Confirmed",
                BookingStatus.Cancelled => "Cancelled",
                BookingStatus.Completed => "Completed",
                _ => status.ToString()
            };
        }
    }
}