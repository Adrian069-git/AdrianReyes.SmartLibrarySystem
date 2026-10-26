using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Resend;
using SmartLibrarySystem.Infrastructure.Data;
using SmartLibrarySystem.Infrastructure.Helpers;

public class ForgotPasswordModel : PageModel
{
    private readonly SmartLibraryDbContext _dbContext;
    private readonly ForgotPasswordTokenService _tokenService;
    private readonly IConfiguration _configuration;
    private readonly IResend _resend;
    private readonly ILogger<ForgotPasswordModel> _logger;

    public ForgotPasswordModel(
        SmartLibraryDbContext dbContext,
        ForgotPasswordTokenService tokenService,
        IConfiguration configuration,
        IResend resend,
        ILogger<ForgotPasswordModel> logger)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
        _configuration = configuration;
        _resend = resend;
        _logger = logger;
    }

    [BindProperty]
    public ForgotPasswordDto ForgotPasswordDto { get; set; } = new();

    public string? SuccessMessage { get; private set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        // UserName is also the email in the existing Invite/Login workflow.
        // Email is used first, with UserName as a compatibility fallback.
        var email = ForgotPasswordDto.EmailAddress!.Trim();

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u =>
                (u.Email != null && u.Email.ToLower() == email.ToLower()) ||
                (u.UserName != null && u.UserName.ToLower() == email.ToLower()));

        if (user != null)
        {
            var recipient = !string.IsNullOrWhiteSpace(user.Email)
                ? user.Email
                : user.UserName;

            if (!string.IsNullOrWhiteSpace(recipient))
            {
                try
                {
                    var token = _tokenService.CreateResetToken(user.Id);
                    var resetUrl =
                        $"{Request.Scheme}://{Request.Host}/account/verify-forgot-password?token={Uri.EscapeDataString(token)}";

                    var fromAddress = _configuration["Resend:From"]
                        ?? "onboarding@resend.dev";

                    await _resend.EmailSendAsync(new EmailMessage
                    {
                        From = fromAddress,
                        To = recipient,
                        Subject = "SmartLibrarySystem - Reset your password",
                        HtmlBody = $@"
                            <p>Hello <strong>{user.FirstName} {user.LastName}</strong>,</p>
                            <p>You requested to reset your password.</p>
                            <p>
                                <a href=""{resetUrl}"">Reset your password</a>
                            </p>
                            <p>This password reset link will expire in 5 minutes.</p>
                            <p>If you did not request this, you can safely ignore this email.</p>"
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to send password reset email.");
                    // Do not reveal account existence to the user.
                }
            }
        }

        // Deliberately identical for existing and non-existing emails.
        SuccessMessage =
            "We have sent you an email with link to reset your password.";

        return Page();
    }
}

public class ForgotPasswordDto
{
    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string? EmailAddress { get; set; }
}
