using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartLibrarySystem.Infrastructure.Data;
using SmartLibrarySystem.Infrastructure.Helpers;

[Authorize]
public class MyProfileModel : PageModel
{
    private readonly SmartLibraryDbContext _db;
    private readonly IWebHostEnvironment _environment;

    public MyProfileModel(SmartLibraryDbContext db, IWebHostEnvironment environment)
    {
        _db = db;
        _environment = environment;
    }

    public User? Profile { get; private set; }
    public string ProfileImage { get; private set; } = "/users/default.png";
    public string? SuccessMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = GetUserId();
        if (userId == null)
            return RedirectToPage("/Account/Login");

        Profile = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId.Value);
        if (Profile == null)
            return NotFound("User profile not found.");

        ProfileImage = ProfileImageHelper.GetImagePath(_environment.WebRootPath, Profile.Id);
        SuccessMessage = TempData["ProfileSuccess"] as string;
        return Page();
    }

    private Guid? GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue("UserId");
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
