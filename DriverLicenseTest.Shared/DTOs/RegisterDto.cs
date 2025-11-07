using System.ComponentModel.DataAnnotations;

namespace DriverLicenseTest.Shared.DTOs.User;

public class RegisterDto
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    [Compare("Password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [StringLength(200)]
    public string? FullName { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }
}
