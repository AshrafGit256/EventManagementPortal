using System.ComponentModel.DataAnnotations;

namespace EventManagementPortal.ViewModels;

/// <summary>
/// ViewModel for creating a new event
/// </summary>
public class CreateEventViewModel
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    [Display(Name = "Event Title")]
    public string Title { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Description is required")]
    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Location is required")]
    [StringLength(300, ErrorMessage = "Location cannot exceed 300 characters")]
    [Display(Name = "Location")]
    public string Location { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Event date is required")]
    [Display(Name = "Event Date and Time")]
    public DateTime EventDate { get; set; } = DateTime.Now.AddDays(7);
}

/// <summary>
/// ViewModel for editing an event
/// </summary>
public class EditEventViewModel
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    [Display(Name = "Event Title")]
    public string Title { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Description is required")]
    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Location is required")]
    [StringLength(300, ErrorMessage = "Location cannot exceed 300 characters")]
    [Display(Name = "Location")]
    public string Location { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Event date is required")]
    [Display(Name = "Event Date and Time")]
    public DateTime EventDate { get; set; }
    
    public string CreatedByUserId { get; set; } = string.Empty;
}