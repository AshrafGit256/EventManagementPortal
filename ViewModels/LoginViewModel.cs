using System.ComponentModel.DataAnnotations;

namespace EventManagementPortal.ViewModels;

/// <summary>
/// ViewModel for login form
/// Only requires email and password
/// </summary>
public class LoginViewModel
{
    // Email address for login
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    // Password
    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    // Remember me checkbox - keeps user logged in longer
    [Display(Name = "Remember me?")]
    public bool RememberMe { get; set; }

    // Optional: Return URL after successful login
    public string? ReturnUrl { get; set; }
}