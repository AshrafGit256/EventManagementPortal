using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventManagementPortal.Data;
using EventManagementPortal.Models;
using EventManagementPortal.ViewModels;

namespace EventManagementPortal.Controllers;

/// <summary>
/// Controller for Organiser event management
/// Only users with Organiser role can access these actions
/// Organisers can only manage their own events
/// </summary>
[Authorize(Roles = "Organiser")]
public class EventsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<EventsController> _logger;

    public EventsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<EventsController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    // ========================================
    // INDEX - VIEW MY EVENTS
    // ========================================
    /// <summary>
    /// Shows list of events created by the current organiser
    /// Organisers can ONLY see their own events
    /// GET: /Events/Index
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // Get the current logged-in user's ID
        var userId = _userManager.GetUserId(User);
        
        // Get only events created by this user
        var myEvents = await _context.Events
            .Where(e => e.CreatedByUserId == userId)
            .OrderByDescending(e => e.EventDate)
            .ToListAsync();

        return View(myEvents);
    }

    // ========================================
    // CREATE EVENT - GET
    // ========================================
    /// <summary>
    /// Shows the create event form
    /// GET: /Events/Create
    /// </summary>
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    // ========================================
    // CREATE EVENT - POST
    // ========================================
    /// <summary>
    /// Processes the create event form
    /// POST: /Events/Create
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateEventViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Validate that event date is in the future
        if (model.EventDate <= DateTime.Now)
        {
            ModelState.AddModelError("EventDate", "Event date must be in the future.");
            return View(model);
        }

        // Get current user ID
        var userId = _userManager.GetUserId(User);

        // Create new event
        var newEvent = new Event
        {
            Title = model.Title,
            Description = model.Description,
            Location = model.Location,
            EventDate = model.EventDate,
            CreatedByUserId = userId!,
            CreatedAt = DateTime.UtcNow
        };

        // Add to database
        _context.Events.Add(newEvent);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Organiser {userId} created event: {newEvent.Title} (ID: {newEvent.Id})");

        TempData["SuccessMessage"] = "Event created successfully!";
        return RedirectToAction(nameof(Index));
    }

    // ========================================
    // EDIT EVENT - GET
    // ========================================
    /// <summary>
    /// Shows the edit event form
    /// Organiser can ONLY edit their own events
    /// GET: /Events/Edit/5
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var userId = _userManager.GetUserId(User);
        
        // Find the event
        var eventToEdit = await _context.Events
            .FirstOrDefaultAsync(e => e.Id == id);

        if (eventToEdit == null)
        {
            TempData["ErrorMessage"] = "Event not found.";
            return RedirectToAction(nameof(Index));
        }

        // CRITICAL: Check ownership - organiser can only edit their own events
        if (eventToEdit.CreatedByUserId != userId)
        {
            TempData["ErrorMessage"] = "You can only edit your own events.";
            return RedirectToAction(nameof(Index));
        }

        // Map to view model
        var viewModel = new EditEventViewModel
        {
            Id = eventToEdit.Id,
            Title = eventToEdit.Title,
            Description = eventToEdit.Description,
            Location = eventToEdit.Location,
            EventDate = eventToEdit.EventDate,
            CreatedByUserId = eventToEdit.CreatedByUserId
        };

        return View(viewModel);
    }

    // ========================================
    // EDIT EVENT - POST
    // ========================================
    /// <summary>
    /// Processes the edit event form
    /// POST: /Events/Edit/5
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditEventViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = _userManager.GetUserId(User);
        
        // Find the event
        var eventToUpdate = await _context.Events.FindAsync(id);

        if (eventToUpdate == null)
        {
            TempData["ErrorMessage"] = "Event not found.";
            return RedirectToAction(nameof(Index));
        }

        // CRITICAL: Check ownership - organiser can only edit their own events
        if (eventToUpdate.CreatedByUserId != userId)
        {
            TempData["ErrorMessage"] = "You can only edit your own events.";
            return RedirectToAction(nameof(Index));
        }

        // Validate event date
        if (model.EventDate <= DateTime.Now)
        {
            ModelState.AddModelError("EventDate", "Event date must be in the future.");
            return View(model);
        }

        // Update properties
        eventToUpdate.Title = model.Title;
        eventToUpdate.Description = model.Description;
        eventToUpdate.Location = model.Location;
        eventToUpdate.EventDate = model.EventDate;

        // Save changes
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Organiser {userId} updated event: {eventToUpdate.Title} (ID: {id})");

        TempData["SuccessMessage"] = "Event updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    // ========================================
    // DELETE EVENT - POST
    // ========================================
    /// <summary>
    /// Deletes an event
    /// Organiser can ONLY delete their own events
    /// POST: /Events/Delete/5
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = _userManager.GetUserId(User);
        
        // Find the event
        var eventToDelete = await _context.Events.FindAsync(id);

        if (eventToDelete == null)
        {
            TempData["ErrorMessage"] = "Event not found.";
            return RedirectToAction(nameof(Index));
        }

        // CRITICAL: Check ownership - organiser can only delete their own events
        if (eventToDelete.CreatedByUserId != userId)
        {
            TempData["ErrorMessage"] = "You can only delete your own events.";
            return RedirectToAction(nameof(Index));
        }

        // Delete the event
        _context.Events.Remove(eventToDelete);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Organiser {userId} deleted event: {eventToDelete.Title} (ID: {id})");

        TempData["SuccessMessage"] = $"Event '{eventToDelete.Title}' has been deleted.";
        return RedirectToAction(nameof(Index));
    }

    // ========================================
    // DETAILS - VIEW EVENT DETAILS
    // ========================================
    /// <summary>
    /// Shows detailed view of an event
    /// GET: /Events/Details/5
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var userId = _userManager.GetUserId(User);
        
        // Find the event with guests
        var eventDetails = await _context.Events
            .Include(e => e.CreatedBy)
            .Include(e => e.Guests)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (eventDetails == null)
        {
            TempData["ErrorMessage"] = "Event not found.";
            return RedirectToAction(nameof(Index));
        }

        // Check ownership
        if (eventDetails.CreatedByUserId != userId)
        {
            TempData["ErrorMessage"] = "You can only view your own events.";
            return RedirectToAction(nameof(Index));
        }

        return View(eventDetails);
    }
}