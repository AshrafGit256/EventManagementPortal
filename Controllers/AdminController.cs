using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventManagementPortal.Data;
using EventManagementPortal.Models;
using EventManagementPortal.ViewModels;

namespace EventManagementPortal.Controllers;

/// <summary>
/// Controller for Admin-specific functionality
/// Only users with Admin role can access these actions
/// </summary>
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        ILogger<AdminController> logger)
    {
        _userManager = userManager;
        _context = context;
        _logger = logger;
    }

    // ========================================
    // ADMIN DASHBOARD
    // ========================================
    public async Task<IActionResult> Dashboard()
    {
        var totalEvents = await _context.Events.CountAsync();
        var totalOrganisers = await _userManager.GetUsersInRoleAsync("Organiser");
        var totalGuests = await _context.Guests.CountAsync();
        
        var upcomingEvents = await _context.Events
            .Include(e => e.CreatedBy)
            .Where(e => e.EventDate > DateTime.Now)
            .OrderBy(e => e.EventDate)
            .Take(5)
            .ToListAsync();

        ViewBag.TotalEvents = totalEvents;
        ViewBag.TotalOrganisers = totalOrganisers.Count;
        ViewBag.TotalGuests = totalGuests;
        ViewBag.UpcomingEvents = upcomingEvents;

        return View();
    }

    // ========================================
    // CREATE ORGANISER - GET
    // ========================================
    [HttpGet]
    public IActionResult CreateOrganiser()
    {
        return View();
    }

    // ========================================
    // CREATE ORGANISER - POST
    // ========================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateOrganiser(CreateOrganiserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser != null)
        {
            ModelState.AddModelError("Email", "This email is already registered.");
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "Organiser");
            _logger.LogInformation($"Admin created new Organiser account: {model.Email}");
            TempData["SuccessMessage"] = $"Organiser account created successfully for {model.FullName}!";
            return RedirectToAction(nameof(ManageOrganisers));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    // ========================================
    // MANAGE ORGANISERS
    // ========================================
    [HttpGet]
    public async Task<IActionResult> ManageOrganisers()
    {
        var organisers = await _userManager.GetUsersInRoleAsync("Organiser");
        var organisersList = new List<OrganiserViewModel>();
        
        foreach (var organiser in organisers)
        {
            var eventCount = await _context.Events
                .Where(e => e.CreatedByUserId == organiser.Id)
                .CountAsync();
            
            organisersList.Add(new OrganiserViewModel
            {
                Id = organiser.Id,
                FullName = organiser.FullName,
                Email = organiser.Email,
                CreatedAt = organiser.CreatedAt,
                EventCount = eventCount
            });
        }

        return View(organisersList);
    }

    // ========================================
    // ALL EVENTS
    // ========================================
    [HttpGet]
    public async Task<IActionResult> AllEvents()
    {
        var events = await _context.Events
            .Include(e => e.CreatedBy)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

        return View(events);
    }

    // ========================================
    // DELETE EVENT
    // ========================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var eventToDelete = await _context.Events
            .Include(e => e.CreatedBy)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (eventToDelete == null)
        {
            TempData["ErrorMessage"] = "Event not found.";
            return RedirectToAction(nameof(AllEvents));
        }

        _context.Events.Remove(eventToDelete);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Admin deleted event: {eventToDelete.Title} (ID: {id})");
        TempData["SuccessMessage"] = $"Event '{eventToDelete.Title}' has been deleted successfully.";
        return RedirectToAction(nameof(AllEvents));
    }

    // ========================================
    // DELETE ORGANISER
    // ========================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteOrganiser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        
        if (user == null)
        {
            TempData["ErrorMessage"] = "Organiser not found.";
            return RedirectToAction(nameof(ManageOrganisers));
        }

        var userEvents = await _context.Events
            .Where(e => e.CreatedByUserId == id)
            .ToListAsync();
        
        _context.Events.RemoveRange(userEvents);

        var result = await _userManager.DeleteAsync(user);

        if (result.Succeeded)
        {
            _logger.LogInformation($"Admin deleted Organiser: {user.Email}");
            TempData["SuccessMessage"] = $"Organiser {user.FullName} and their {userEvents.Count} event(s) have been deleted.";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to delete organiser.";
        }

        return RedirectToAction(nameof(ManageOrganisers));
    }
}