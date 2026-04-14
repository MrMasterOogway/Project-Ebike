using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ProjectWebApp.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    //Added for DMIT2018
    [PersonalData]
    [Required]
    public string SocialInsuranceNumber { get; set; } = string.Empty;
    [Required]
    [StringLength(30)]
    public string FirstName { get; set; } = string.Empty;
    [Required]
    [StringLength(30)]
    public string LastName { get; set; } = string.Empty;
    [StringLength(40)]
    public string? Address { get; set; }
    [StringLength(20)]
    public string? City { get; set; }
    [StringLength(2, MinimumLength = 2)]
    public string? Province { get; set; }
    [StringLength(6, MinimumLength = 6)]
    public string? PostalCode { get; set; }
    [StringLength(15)]
    public string? ContactPhone { get; set; }
    public bool Textable { get; set; } = false;
}

