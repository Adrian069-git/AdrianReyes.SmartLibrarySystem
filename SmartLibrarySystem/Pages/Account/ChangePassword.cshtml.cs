using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartLibrarySystem.Infrastructure.Data;
using SmartLibrarySystem.Infrastructure.Helpers;
using SmartLibrarySystem.Infrastructure.Models;

[Authorize]
public class ChangePasswordModel : PageModel
{
    private readonly SmartLibraryDbContext _db;
    private readonly IWebHostEnvironment _environment;

    public ChangePasswordModel(SmartLibraryDbContext db, IWebHostEnvironment environment)
    {
        _db = db;
        _environment = environment;
    }

    [BindProperty]
    public ChangePasswordDto Input { get; set; } = new();

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

        if (string.IsNullOrWhiteSpace(Input.CurrentPassword))
            ModelState.AddModelError("Input.CurrentPassword", "Current Password is required.");
        if (string.IsNullOrWhiteSpace(Input.NewPassword))
            ModelState.AddModelError("Input.NewPassword", "New Password is required.");
        if (string.IsNullOrWhiteSpace(Input.ConfirmNewPassword))
            ModelState.AddModelError("Input.ConfirmNewPassword", "Confirm Password is required.");

        if (!ModelState.IsValid) return Page();

        if (Input.NewPassword != Input.ConfirmNewPassword)
        {
            ModelState.AddModelError("Input.ConfirmNewPassword", "Passwords do not match.");
            return Page();
        }

        var passwordRecord = await _db.UserLoginInfos.FirstOrDefaultAsync(x =>
            x.UserId == user.Id && x.Key != null && x.Key.ToLower() == "password");

        if (passwordRecord == null || string.IsNullOrWhiteSpace(passwordRecord.Value) ||
            !BCrypt.Net.BCrypt.Verify(Input.CurrentPassword, passwordRecord.Value))
        {
            ModelState.AddModelError(string.Empty, "Current password is incorrect.");
            return Page();
        }

        var passwordErrors = PasswordValidator.Validate(Input.NewPassword!);
        if (passwordErrors.Any())
        {
            foreach (var error in passwordErrors)
                ModelState.AddModelError("Input.NewPassword", error);
            return Page();
        }

        passwordRecord.Value = BCrypt.Net.BCrypt.HashPassword(Input.NewPassword);
        await _db.SaveChangesAsync();

        TempData["ProfileSuccess"] = "Password changed successfully.";
        return RedirectToPage("/Account/Profile");
    }

    private void LoadUser(User user)
    {
        Input.Username = user.UserName;
        Input.ProfileImage = ProfileImageHelper.GetImagePath(_environment.WebRootPath, user.Id);
    }

    private Guid? GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("UserId");
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
