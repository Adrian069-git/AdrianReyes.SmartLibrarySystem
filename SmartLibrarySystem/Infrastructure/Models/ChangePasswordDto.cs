using System.ComponentModel.DataAnnotations;

namespace SmartLibrarySystem.Infrastructure.Models;

public class ChangePasswordDto
{
    public string? Username { get; set; }
    public string? ProfileImage { get; set; }

    [Required(ErrorMessage = "Current Password is required.")]
    [DataType(DataType.Password)]
    public string? CurrentPassword { get; set; }

    [Required(ErrorMessage = "New Password is required.")]
    [DataType(DataType.Password)]
    public string? NewPassword { get; set; }

    [Required(ErrorMessage = "Confirm Password is required.")]
    [DataType(DataType.Password)]
    public string? ConfirmNewPassword { get; set; }
}
