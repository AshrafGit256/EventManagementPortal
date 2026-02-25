using System.ComponentModel.DataAnnotations;

namespace EventManagementPortal.Models;

//Represents the guests registered for the event
public class Guest
{
    public int Id { get; set; } //Primary key

    //Guest's full name
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    //Guest's email address
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    // Guest's phone number
    [Required(ErrorMessage = "Phone number is required")]
    [Phone(ErrorMessage = "Invalid phone number format")]
    [StringLength(20)]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; } = string.Empty;

    // Foreign key - which event this guest is registered for
    [Required]
    public int EventId { get; set; }

    // When the guest registered
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    // Access the event details
    public Event Event { get; set; } = null!;

}