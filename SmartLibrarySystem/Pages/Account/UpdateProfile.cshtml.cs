
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartLibrarySystem.Infrastructure.Data;
using SmartLibrarySystem.Infrastructure.Helpers;


[Authorize]
public class UpdateProfileModel : PageModel
{
    private readonly SmartLibraryDbContext _db;
    private readonly IWebHostEnvironment _environment;

    public UpdateProfileModel(SmartLibraryDbContext db, IWebHostEnvironment environment)
    {
        _db = db;
        _environment = environment;
    }

    [BindProperty]
    public UpdateProfileDto Input { get; set; } = new();

    public string ProfileImage { get; private set; } = "/users/default.png";
    public string? Username { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = GetUserId();
        if (userId == null) return RedirectToPage("/Account/Login");

        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId.Value);
        if (user == null) return NotFound("User profile not found.");

        Username = user.UserName;
        Input.FirstName = user.FirstName;
        Input.LastName = user.LastName;
        Input.DateOfBirth = user.DateOfBirth;
        ProfileImage = ProfileImageHelper.GetImagePath(_environment.WebRootPath, user.Id);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = GetUserId();
        if (userId == null) return RedirectToPage("/Account/Login");

        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId.Value);
        if (user == null) return NotFound("User profile not found.");

        Username = user.UserName;
        ProfileImage = ProfileImageHelper.GetImagePath(_environment.WebRootPath, user.Id);

        if (string.IsNullOrWhiteSpace(Input.FirstName) ||
            string.IsNullOrWhiteSpace(Input.LastName) ||
            Input.DateOfBirth == default)
        {
            ModelState.AddModelError(string.Empty, "All fields are required.");
            return Page();
        }

        if (CalculateAge(Input.DateOfBirth) <= 17)
        {
            ModelState.AddModelError(string.Empty, "User must be more than 17 years old.");
            return Page();
        }

        user.FirstName = Input.FirstName.Trim();
        user.LastName = Input.LastName.Trim();
        user.DateOfBirth = Input.DateOfBirth;
        await _db.SaveChangesAsync();

        var roleRecord = await _db.UserLoginInfos.FirstOrDefaultAsync(r =>
            r.UserId == user.Id && r.Key != null && r.Key.ToLower() == "role");
        var roleValue = roleRecord?.Value?.Trim() ?? "User";
        roleValue = roleValue.Equals("Admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "User";

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("UserId", user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName ?? user.FirstName),
            new Claim(ClaimTypes.Role, roleValue),
            new Claim("FirstName", user.FirstName),
            new Claim("LastName", user.LastName),
            new Claim("FullName", $"{user.FirstName} {user.LastName}".Trim()),
            new Claim("DateOfBirth", user.DateOfBirth.ToString("yyyy-MM-dd"))
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        TempData["ProfileSuccess"] = "Profile updated successfully.";
        return RedirectToPage("/Account/MyProfile");
    }

    private Guid? GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("UserId");
        return Guid.TryParse(value, out var id) ? id : null;
    }

    private static int CalculateAge(DateTime dob)
    {
        var today = DateTime.Today;
        var age = today.Year - dob.Year;
        if (dob.Date > today.AddYears(-age)) age--;
        return age;
    }
}

public class UpdateProfileDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
}
