using System.ComponentModel.DataAnnotations;
using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartLibrarySystem.Infrastructure.Data;
using SmartLibrarySystem.Infrastructure.Helpers;

public class VerifyForgotPasswordModel : PageModel
{
    private readonly SmartLibraryDbContext _dbContext;
    private readonly ForgotPasswordTokenService _tokenService;

    public VerifyForgotPasswordModel(
        SmartLibraryDbContext dbContext,
        ForgotPasswordTokenService tokenService)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
    }

    [BindProperty]
    public ResetPasswordDto ResetPasswordDto { get; set; } = new();

    public bool IsTokenValid { get; private set; }

    public async Task<IActionResult> OnGetAsync(string? token)
    {
        var userId = ValidateToken(token);

        if (userId == null)
        {
            ModelState.AddModelError(
                string.Empty,
                "Invalid or expired password reset link.");
            IsTokenValid = false;
            return Page();
        }

        var userExists = await _dbContext.Users
            .AnyAsync(u => u.Id == userId.Value);

        if (!userExists)
        {
            ModelState.AddModelError(
                string.Empty,
                "Invalid or expired password reset link.");
            IsTokenValid = false;
            return Page();
        }

        ResetPasswordDto.Token = token;
        ResetPasswordDto.UserId = userId.Value;
        IsTokenValid = true;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Never trust the hidden UserId by itself. Revalidate the JWT
        // and get the UserId again from the token.
        var userId = ValidateToken(ResetPasswordDto.Token);

        if (userId == null)
        {
            ModelState.AddModelError(
                string.Empty,
                "Invalid or expired password reset link.");
            IsTokenValid = false;
            return Page();
        }

        ResetPasswordDto.UserId = userId.Value;
        IsTokenValid = true;

        if (!ModelState.IsValid)
            return Page();

        var passwordErrors =
            PasswordValidator.Validate(ResetPasswordDto.NewPassword!);

        if (passwordErrors.Any())
        {
            foreach (var error in passwordErrors)
                ModelState.AddModelError("NewPassword", error);

            return Page();
        }

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId.Value);

        if (user == null)
        {
            ModelState.AddModelError(
                string.Empty,
                "Invalid or expired password reset link.");
            return Page();
        }

        var passwordRecord = await _dbContext.UserLoginInfos
            .FirstOrDefaultAsync(x =>
                x.UserId == userId.Value &&
                x.Key != null &&
                x.Key.ToLower() == "password");

        var hashedPassword =
            BCrypt.Net.BCrypt.HashPassword(ResetPasswordDto.NewPassword);

        if (passwordRecord == null)
        {
            passwordRecord = new UserLoginInfo(
                userId.Value,
                "password",
                hashedPassword);

            await _dbContext.UserLoginInfos.AddAsync(passwordRecord);
        }
        else
        {
            passwordRecord.Value = hashedPassword;
        }

        await _dbContext.SaveChangesAsync();

        TempData["PasswordResetSuccess"] =
            "Your password has been reset successfully. You may now log in.";

        return RedirectToPage("/Account/Login");
    }

    private Guid? ValidateToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        return _tokenService.ValidateResetToken(token);
    }
}

public class ResetPasswordDto
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public string? Token { get; set; }

    [Required(ErrorMessage = "New password is required.")]
    [DataType(DataType.Password)]
    public string? NewPassword { get; set; }

    [Required(ErrorMessage = "Confirm new password is required.")]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
    public string? ConfirmNewPassword { get; set; }
}
