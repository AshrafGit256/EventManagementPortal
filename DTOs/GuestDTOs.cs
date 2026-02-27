using System.ComponentModel.DataAnnotations;

namespace EventManagementPortal.DTOs;

/// <summary>
/// Data Transfer Object for guest registration via API
/// This is what the API expects to receive
/// </summary>
public class GuestRegistrationDTO
{
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string FullName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Phone number is required")]
    [Phone(ErrorMessage = "Invalid phone number")]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Event ID is required")]
    public int EventId { get; set; }
}

/// <summary>
/// Response returned by the API after guest registration
/// </summary>
public class GuestRegistrationResponseDTO
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? GuestId { get; set; }
}