using Car_Rent_System.Ennum;

namespace Car_Rent_System.Models
{
    public class User2
    {

            public int Id { get; set; }
            //public string Username { get; set; }
            public string FullName { get; set; }
            public string Email { get; set; }
            public UserRole Role { get; set; } // store role name e.g. "SuperAdmin"
            public string ProfileImageUrl { get; set; }
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 
    }
}
