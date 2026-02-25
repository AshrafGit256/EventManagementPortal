using Microsoft.AspNetCore.Identity;

namespace EventManagementPortal.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName {get; set;} = string.Empty; //User's full name

    public DateTime CreatedAt { get; set;} = DateTime.UtcNow; // When was the user account created

    //Events created by this user
    public ICollection<Event> CreatedEvents { get; set; } = new List<Event>();
}