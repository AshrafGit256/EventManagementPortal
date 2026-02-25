using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EventManagementPortal.Models;

namespace EventManagementPortal.Data;

/// <summary>
/// Database context - manages database connection and operations
/// Inherits from IdentityDbContext to get all Identity tables (Users, Roles, etc.)
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    // Constructor - receives configuration options from Program.cs
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets represent tables in the database
    // Each DbSet<T> becomes a table named after the property (Events, Guests)

    /// <summary>
    /// Events table - stores all events created by organisers
    /// </summary>
    public DbSet<Event> Events { get; set; }

    /// <summary>
    /// Guests table - stores guest registrations for events
    /// </summary>
    public DbSet<Guest> Guests { get; set; }

    /// <summary>
    /// Called when the model is being created
    /// Used to configure relationships, constraints, and seed data
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Call base method to configure Identity tables
        base.OnModelCreating(modelBuilder);

        // ========================================
        // CONFIGURE EVENT ENTITY
        // ========================================
        modelBuilder.Entity<Event>(entity =>
        {
            // Set the table name
            entity.ToTable("Events");

            // Configure the relationship between Event and ApplicationUser
            // One user (Organiser) can create many events
            entity.HasOne(e => e.CreatedBy)               // Each event has one creator
                .WithMany(u => u.CreatedEvents)            // Each user has many events
                .HasForeignKey(e => e.CreatedByUserId)     // Foreign key property
                .OnDelete(DeleteBehavior.Restrict);        // Don't allow deleting user if they have events

            // Configure the relationship between Event and Guests
            // One event can have many guests
            entity.HasMany(e => e.Guests)                  // Each event has many guests
                .WithOne(g => g.Event)                     // Each guest belongs to one event
                .HasForeignKey(g => g.EventId)             // Foreign key property
                .OnDelete(DeleteBehavior.Cascade);         // If event is deleted, delete its guests too

            // Create index on CreatedByUserId for faster queries
            entity.HasIndex(e => e.CreatedByUserId);

            // Create index on EventDate for faster date-based queries
            entity.HasIndex(e => e.EventDate);
        });

        // ========================================
        // CONFIGURE GUEST ENTITY
        // ========================================
        modelBuilder.Entity<Guest>(entity =>
        {
            // Set the table name
            entity.ToTable("Guests");

            // Create index on EventId for faster queries when fetching guests for an event
            entity.HasIndex(g => g.EventId);

            // Create index on Email to quickly check if guest is already registered
            entity.HasIndex(g => g.Email);
        });

        // ========================================
        // CONFIGURE APPLICATION USER
        // ========================================
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            // Add index on FullName for searching users
            entity.HasIndex(u => u.FullName);
        });
    }
}