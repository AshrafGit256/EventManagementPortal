using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EventManagementPortal.Data;
using EventManagementPortal.Models;

var builder = WebApplication.CreateBuilder(args);

//Configure service Dependence
builder.Services.AddControllersWithViews(); // Add MVC services (Controllers and Views)


//Configure Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>           // Register the database context with SQL Server
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));


//Configure ASP.NET identit
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>  // Register Identity services for authentication and authorization
{
    // Password requirements
    options.Password.RequireDigit = true;              // Must contain at least one digit (0-9)
    options.Password.RequireLowercase = true;          // Must contain lowercase letter (a-z)
    options.Password.RequireUppercase = true;          // Must contain uppercase letter (A-Z)
    options.Password.RequireNonAlphanumeric = true;    // Must contain special character (!@#$%)
    options.Password.RequiredLength = 6;               // Minimum 6 characters

    // User requirements
    options.User.RequireUniqueEmail = true;            // Email must be unique

    // Lockout settings (optional - prevents brute force attacks)
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);  // Lock for 5 minutes
    options.Lockout.MaxFailedAccessAttempts = 5;       // After 5 failed login attempts
    options.Lockout.AllowedForNewUsers = true;

    // Sign-in settings
    options.SignIn.RequireConfirmedEmail = false;      // Don't require email confirmation
    options.SignIn.RequireConfirmedAccount = false;    // Don't require account confirmation
})
.AddEntityFrameworkStores<ApplicationDbContext>()      // Use EF Core for storing user data
.AddDefaultTokenProviders();                           // Add token providers for password reset, etc.


//Configure authentification cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";              // Redirect here if not logged in
    options.LogoutPath = "/Account/Logout";            // Logout URL
    options.AccessDeniedPath = "/Account/AccessDenied"; // Redirect here if access denied
    options.ExpireTimeSpan = TimeSpan.FromHours(24);   // Cookie expires after 24 hours
    options.SlidingExpiration = true;                  // Reset expiration time on each request
});

var app = builder.Build();

// ========================================
// SEED DATABASE WITH ROLES AND ADMIN USER
// ========================================

using (var scope = app.Services.CreateScope())        // This runs once when the application starts, Creates Admin and Organiser roles and Creates a default admin user
{
    var services = scope.ServiceProvider;
    try
    {
        // Get required services
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Call the seed method
        await SeedData(userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}


//Configure Http request pipeline
if (!app.Environment.IsDevelopment()) // Configure error handling
{
    // In production, show friendly error page
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    // In development, show detailed error page
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();        // Redirect HTTP to HTTPS
app.UseStaticFiles();             // Enable serving static files (CSS, JS, images)

app.UseRouting();                 // Enable routing

// CRITICAL: Authentication must come before Authorization
app.UseAuthentication();          // Enable authentication (who are you?)
app.UseAuthorization();           // Enable authorization (what can you do?)

// Configure default route pattern
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();



//Database seeding method
async Task SeedData(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
{
    //Create roles

    if (!await roleManager.RoleExistsAsync("Admin"))            // Check if Admin role exists, create if not
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
        Console.WriteLine("Admin role created");
    }


    if (!await roleManager.RoleExistsAsync("Organiser"))       // Check if Organiser role exists, create if not
    {
        await roleManager.CreateAsync(new IdentityRole("Organiser"));
        Console.WriteLine("Organiser role created");
    }


    //Create default AdminUser
    var adminEmail = "admin@eventportal.com";                  // Check if admin user already exists
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {

        adminUser = new ApplicationUser                        // Create new admin user
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "System Administrator",
            EmailConfirmed = true,  // Skip email confirmation
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(adminUser, "Admin@123");     // Create user with password

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");               // Assign Admin role to the user
            Console.WriteLine("Default admin user created");
            Console.WriteLine($"  Email: {adminEmail}");
            Console.WriteLine("  Password: Admin@123");
        }
        else
        {
            Console.WriteLine("Failed to create admin user:");
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"  - {error.Description}");
            }
        }
    }
    else
    {
        Console.WriteLine("Admin user already exists");
    }
}
