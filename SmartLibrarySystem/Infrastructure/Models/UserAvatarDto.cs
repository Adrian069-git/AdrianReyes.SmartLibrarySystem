using Microsoft.AspNetCore.Http;

namespace SmartLibrarySystem.Infrastructure.Models;

public class UserAvatarDto
{
    public string? Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ProfileImage { get; set; }
    public IFormFile? AvatarImage { get; set; }
}
