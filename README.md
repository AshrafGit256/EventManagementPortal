# Event Management Portal

## Technologies Used
- ASP.NET Core MVC (.NET 10)
- Entity Framework Core
- SQL Server
- ASP.NET Identity
- Bootstrap 5
- C#

## Database Setup
1. Update connection string in `appsettings.json` if needed
2. Run migrations:
```
   dotnet ef database update
```

## Default Admin Credentials
**Email:** admin@eventportal.com  
**Password:** Admin@123

## Features Implemented
✅ Role-based authentication (Admin & Organiser)  
✅ Admin can create Organiser accounts  
✅ Admin can view and delete all events  
✅ Organiser can create/edit/delete their own events  
✅ Full CRUD operations for events  
✅ Web API for guest registration  
✅ Public guest registration form  
✅ Proper authorization and security  

## How to Run
```bash
dotnet run
```
Then open: http://localhost:5200

## Project Structure
- Controllers/ - MVC and API controllers
- Models/ - Database entities
- Views/ - Razor views
- ViewModels/ - Data transfer objects for views
- DTOs/ - API data transfer objects
- Data/ - Database context