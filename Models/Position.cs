// Models/Position.cs
namespace Car_Rent_System.Models
{
    public class Position
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public long DeviceId { get; set; }
        public DateTime FixTime { get; set; }
    }
}
