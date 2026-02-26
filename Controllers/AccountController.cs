using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EventManagementPortal.Models;
using EventManagementPortal.ViewModels;

namespace EventManagementPortal.Controllers;

/// <summary>
/// Handles user authentication: registration, login, logout
/// </summary>
public class AccountController : Controller
{
    // Services injected by ASP.NET Core
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<AccountController> _logger;

    /// <summary>
    /// Constructor - ASP.NET Core automatically provides these services
    /// </summary>
    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    // ========================================
    // REGISTER - GET
    // ========================================
    /// <summary>
    /// Shows the registration form
    /// GET: /Account/Register
    /// </summary>
    [HttpGet]
    [AllowAnonymous]  // Anyone can access (even not logged in)
    public IActionResult Register()
    {
        // If user is already logged in, redirect to home
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View();
    }

    // ========================================
    // REGISTER - POST
    // ========================================
    /// <summary>
    /// Processes the registration form submission
    /// POST: /Account/Register
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]  // Prevents CSRF attacks
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        // Check if the form data is valid based on validation attributes
        if (!ModelState.IsValid)
        {
            // If invalid, show the form again with error messages
            return View(model);
        }

        // Create a new ApplicationUser from the form data
        var user = new ApplicationUser
        {
            UserName = model.Email,           // Use email as username
            Email = model.Email,
            FullName = model.FullName,
            CreatedAt = DateTime.UtcNow
        };

        // Attempt to create the user with the provided password
        // UserManager hashes the password automatically
        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            // User created successfully
            _logger.LogInformation("User created a new account with password.");

            // Assign "Organiser" role to new users by default
            // Admin creates Organiser accounts, regular users can also register as Organisers
            await _userManager.AddToRoleAsync(user, "Organiser");

            // Automatically sign in the user after registration
            await _signInManager.SignInAsync(user, isPersistent: false);

            // Show success message
            TempData["SuccessMessage"] = "Registration successful! Welcome to Event Management Portal.";

            // Redirect to events page (we'll create this later)
            return RedirectToAction("Index", "Events");
        }

        // If we got here, something went wrong
        // Add all errors to ModelState so they appear in the view
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        // Show the form again with error messages
        return View(model);
    }

    // ========================================
    // LOGIN - GET
    // ========================================
    /// <summary>
    /// Shows the login form
    /// GET: /Account/Login
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        // If user is already logged in, redirect to home
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        // Store the return URL so we can redirect after login
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    // ========================================
    // LOGIN - POST
    // ========================================
    /// <summary>
    /// Processes the login form submission
    /// POST: /Account/Login
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        // Store return URL
        ViewData["ReturnUrl"] = returnUrl;

        // Check if form data is valid
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Attempt to sign in the user
        // Parameters:
        // - email: the username (we use email as username)
        // - password: the password
        // - isPersistent: RememberMe checkbox - keeps user logged in
        // - lockoutOnFailure: Lock account after too many failed attempts
        var result = await _signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            // Login successful
            _logger.LogInformation("User logged in.");

            // Find the user to check their role
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);

                // Redirect based on role
                if (roles.Contains("Admin"))
                {
                    // Admin goes to admin dashboard
                    return RedirectToAction("Dashboard", "Admin");
                }
                else if (roles.Contains("Organiser"))
                {
                    // Organiser goes to their events
                    return RedirectToAction("Index", "Events");
                }
            }

            // If returnUrl exists and is local, redirect there
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // Default redirect
            return RedirectToAction("Index", "Home");
        }

        if (result.IsLockedOut)
        {
            // Account is locked due to too many failed login attempts
            _logger.LogWarning("User account locked out.");
            ModelState.AddModelError(string.Empty,
                "Your account has been locked out due to too many failed login attempts. Please try again later.");
            return View(model);
        }

        // Login failed
        ModelState.AddModelError(string.Empty, "Invalid email or password.");
        return View(model);
    }

    // ========================================
    // LOGOUT
    // ========================================
    /// <summary>
    /// Logs out the current user
    /// POST: /Account/Logout
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        // Sign out the user
        await _signInManager.SignOutAsync();

        _logger.LogInformation("User logged out.");

        TempData["SuccessMessage"] = "You have been logged out successfully.";

        // Redirect to home page
        return RedirectToAction("Index", "Home");
    }

    // ========================================
    // ACCESS DENIED
    // ========================================
    /// <summary>
    /// Shows access denied page when user tries to access unauthorized page
    /// GET: /Account/AccessDenied
    /// </summary>
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
