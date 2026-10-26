using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SmartLibrarySystem.Infrastructure.Data;
using SmartLibrarySystem.Infrastructure.Helpers;
using SmartLibrarySystem.Infrastructure.Models;

[Authorize]
public class UpdateAvatarModel : PageModel
{
    private readonly SmartLibraryDbContext _db;
    private readonly IWebHostEnvironment _environment;

    public UpdateAvatarModel(SmartLibraryDbContext db, IWebHostEnvironment environment)
    {
        _db = db;
        _environment = environment;
    }

    [BindProperty]
    public UserAvatarDto Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = GetUserId();
        if (userId == null) return RedirectToPage("/Account/Login");

        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId.Value);
        if (user == null) return NotFound();

        LoadUser(user);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = GetUserId();
        if (userId == null) return RedirectToPage("/Account/Login");

        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId.Value);
        if (user == null) return NotFound();

        LoadUser(user);

        if (Input.AvatarImage == null || Input.AvatarImage.Length == 0)
        {
            ModelState.AddModelError("Input.AvatarImage", "Avatar Image is required.");
            return Page();
        }

        if (Input.AvatarImage.Length > 5 * 1024 * 1024)
        {
            ModelState.AddModelError("Input.AvatarImage", "Avatar image must be 5 MB or smaller.");
            return Page();
        }

        var usersFolder = Path.Combine(_environment.WebRootPath, "users");
        Directory.CreateDirectory(usersFolder);
        var outputPath = Path.Combine(usersFolder, $"{user.Id}.png");

        try
        {
            await using var inputStream = Input.AvatarImage.OpenReadStream();
            using var image = await Image.LoadAsync(inputStream);
            await image.SaveAsPngAsync(outputPath);
        }
        catch
        {
            ModelState.AddModelError("Input.AvatarImage", "Only image files are allowed.");
            return Page();
        }

        return RedirectToPage("/Account/Profile");
    }

    private void LoadUser(User user)
    {
        Input.Username = user.UserName;
        Input.FirstName = user.FirstName;
        Input.LastName = user.LastName;
        Input.ProfileImage = ProfileImageHelper.GetImagePath(_environment.WebRootPath, user.Id);
    }

    private Guid? GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("UserId");
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
