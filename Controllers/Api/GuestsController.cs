using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventManagementPortal.Data;
using EventManagementPortal.Models;
using EventManagementPortal.DTOs;

namespace EventManagementPortal.Controllers.Api;

/// <summary>
/// Web API Controller for guest registration
/// This endpoint accepts POST requests with guest data in JSON format
/// Returns appropriate HTTP status codes
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class GuestsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GuestsController> _logger;

    public GuestsController(ApplicationDbContext context, ILogger<GuestsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Registers a guest for an event
    /// POST: api/guests
    /// 
    /// Request Body (JSON):
    /// {
    ///   "fullName": "John Doe",
    ///   "email": "john@example.com",
    ///   "phoneNumber": "+1234567890",
    ///   "eventId": 1
    /// }
    /// 
    /// Returns:
    /// - 201 Created: Guest registered successfully
    /// - 400 Bad Request: Invalid input data
    /// - 404 Not Found: Event doesn't exist
    /// - 409 Conflict: Guest already registered for this event
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> RegisterGuest([FromBody] GuestRegistrationDTO dto)
    {
        // Validate input using data annotations
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            
            return BadRequest(new GuestRegistrationResponseDTO
            {
                Success = false,
                Message = string.Join(", ", errors)
            });
        }

        // Check if event exists
        var eventExists = await _context.Events
            .AnyAsync(e => e.Id == dto.EventId);

        if (!eventExists)
        {
            return NotFound(new GuestRegistrationResponseDTO
            {
                Success = false,
                Message = $"Event with ID {dto.EventId} does not exist."
            });
        }

        // Check if guest is already registered for this event
        var alreadyRegistered = await _context.Guests
            .AnyAsync(g => g.Email == dto.Email && g.EventId == dto.EventId);

        if (alreadyRegistered)
        {
            return Conflict(new GuestRegistrationResponseDTO
            {
                Success = false,
                Message = "This email is already registered for this event."
            });
        }

        // Create new guest
        var guest = new Guest
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            EventId = dto.EventId,
            RegisteredAt = DateTime.UtcNow
        };

        // Save to database
        _context.Guests.Add(guest);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Guest registered: {guest.FullName} for event ID {dto.EventId}");

        // Return 201 Created with guest details
        return CreatedAtAction(
            nameof(GetGuest),
            new { id = guest.Id },
            new GuestRegistrationResponseDTO
            {
                Success = true,
                Message = "Registration successful!",
                GuestId = guest.Id
            });
    }

    /// <summary>
    /// Gets a guest by ID
    /// GET: api/guests/5
    /// This is used for the CreatedAtAction response
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetGuest(int id)
    {
        var guest = await _context.Guests
            .Include(g => g.Event)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (guest == null)
        {
            return NotFound();
        }

        return Ok(new
        {
            id = guest.Id,
            fullName = guest.FullName,
            email = guest.Email,
            phoneNumber = guest.PhoneNumber,
            eventId = guest.EventId,
            eventTitle = guest.Event.Title,
            registeredAt = guest.RegisteredAt
        });
    }

    /// <summary>
    /// Gets all guests for a specific event
    /// GET: api/guests/event/5
    /// Organiser can use this to see who registered
    /// </summary>
    [HttpGet("event/{eventId}")]
    public async Task<IActionResult> GetEventGuests(int eventId)
    {
        var guests = await _context.Guests
            .Where(g => g.EventId == eventId)
            .OrderBy(g => g.RegisteredAt)
            .Select(g => new
            {
                id = g.Id,
                fullName = g.FullName,
                email = g.Email,
                phoneNumber = g.PhoneNumber,
                registeredAt = g.RegisteredAt
            })
            .ToListAsync();

        return Ok(guests);
    }
}