using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventManagementPortal.Data;
using EventManagementPortal.DTOs;
using System.Text;
using System.Text.Json;

namespace EventManagementPortal.Controllers;

/// <summary>
/// Public controller for guest-facing pages
/// No authentication required
/// </summary>
[AllowAnonymous]
public class PublicController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<PublicController> _logger;

    public PublicController(
        ApplicationDbContext context,
        IHttpClientFactory httpClientFactory,
        ILogger<PublicController> logger)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Shows the guest registration form
    /// GET: /Public/RegisterGuest?eventId=5
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> RegisterGuest(int eventId)
    {
        // Get event details
        var eventDetails = await _context.Events
            .Include(e => e.CreatedBy)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (eventDetails == null)
        {
            ViewBag.ErrorMessage = "Event not found.";
            return View("Error");
        }

        // Pass event details to view
        ViewBag.Event = eventDetails;
        
        return View();
    }

    /// <summary>
    /// Processes the guest registration form
    /// Submits data to the API endpoint via HTTP POST
    /// POST: /Public/RegisterGuest
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterGuest(GuestRegistrationDTO model)
    {
        // Get event details for display
        var eventDetails = await _context.Events
            .Include(e => e.CreatedBy)
            .FirstOrDefaultAsync(e => e.Id == model.EventId);

        if (eventDetails == null)
        {
            ViewBag.ErrorMessage = "Event not found.";
            return View("Error");
        }

        ViewBag.Event = eventDetails;

        // Validate form data
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            // Create HTTP client
            var httpClient = _httpClientFactory.CreateClient();
            
            // Build the API URL
            var apiUrl = $"{Request.Scheme}://{Request.Host}/api/guests";
            
            // Serialize the DTO to JSON
            var jsonContent = JsonSerializer.Serialize(model);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Make POST request to the API
            var response = await httpClient.PostAsync(apiUrl, httpContent);

            // Read the response
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<GuestRegistrationResponseDTO>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (response.IsSuccessStatusCode && result?.Success == true)
            {
                // Registration successful
                ViewBag.SuccessMessage = result.Message;
                ViewBag.GuestId = result.GuestId;
                return View("RegistrationSuccess");
            }
            else
            {
                // Registration failed
                ModelState.AddModelError(string.Empty, result?.Message ?? "Registration failed. Please try again.");
                return View(model);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during guest registration");
            ModelState.AddModelError(string.Empty, "An error occurred. Please try again later.");
            return View(model);
        }
    }
}