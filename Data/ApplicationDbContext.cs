using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EventManagementPortal.Models;

namespace EventManagementPortal.Data;

// DB Context manages database connection and operations
// Inherits from IdentityDbContext to get all Identity tables (Users, Roles e.t.c)
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) //Constructor (recieves configuration options from Program.cs)
    {

    }

    // DbSets represent tables in the database
    // Each DbSet<T> becomes a table named after the property (Events, Guests)

    public DbSet<Event> Events { get; set; } //Events table stores all events created by Organizers

    public DbSet<Guest> Guests { get; set; } //Guests table stores guest registration for events

    protected override void OnModelCreating(ModelBuilder modelBuilder) //Used to configure relationships , constratints and seed data
    {
        base.OnModelCreating(modelBuilder); //Call base method to configure Identity tables


        //Configure Event Entity
        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable("Events"); //Set the table name

            entity.HasOne(e => e.CreatedBy)                 //Each event has one creator
                .WithMany(u => u.CreatedEvents)             //Each user has many events
                .HasForeignKey(e => e.CreatedByUserId)      //Foreign Key property
                .OnDelete(DeleteBehavior.Restrict);         //If event is deleted, delete its guests too

            entity.HasMany(e => e.Guests)           //Each event has many guests
                .WithOne(g => g.Event)              //Each guest belongs to one event
                .HasForeignKey(g => g.EventId)      //Foreign key 
                .OnDelete(DeleteBehavior.Cascade);  //If event is deleted, delete its guests too

            //Create index on CreatedByUserId for faster queries
            entity.HasIndex(e => e.CreatedByUserId);

            //Create index on EventDate for faster data-based queries 
            entity.HasIndex(e => e.EventDate);
        });


        //Configure guest entity
        modelBuilder.Entity<Guest>(entity =>
        {
            entity.ToTable("Guests"); //Set table name

            entity.HasIndex(g => g.EventId); //Create an index on EventId for faster queries when fetching guests

            entity.HasIndex(g => g.Email);  //Create index on Email to quickly check if guest is already registered
        });


        //Configure Application User
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.HasIndex(u => u.FullName);
        });
    }
}