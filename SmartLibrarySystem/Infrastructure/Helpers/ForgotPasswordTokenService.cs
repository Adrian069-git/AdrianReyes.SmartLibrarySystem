using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace SmartLibrarySystem.Infrastructure.Helpers;

public class ForgotPasswordTokenService
{
    private readonly IConfiguration _configuration;

    public ForgotPasswordTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string CreateResetToken(Guid userId)
    {
        var secretKey = _configuration["Jwt:ResetSecret"];
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];

        if (string.IsNullOrWhiteSpace(secretKey))
            throw new InvalidOperationException("Jwt:ResetSecret is not configured.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("userId", userId.ToString()),
            new Claim("purpose", "forgot-password")
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public Guid? ValidateResetToken(string token)
    {
        var secretKey = _configuration["Jwt:ResetSecret"];
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];

        if (string.IsNullOrWhiteSpace(secretKey) ||
            string.IsNullOrWhiteSpace(issuer) ||
            string.IsNullOrWhiteSpace(audience))
        {
            return null;
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            tokenHandler.ValidateToken(
                token,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                },
                out SecurityToken validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken)
                return null;

            var purposeClaim = jwtToken.Claims
                .FirstOrDefault(c => c.Type == "purpose");

            if (purposeClaim?.Value != "forgot-password")
                return null;

            var userIdClaim = jwtToken.Claims
                .FirstOrDefault(c => c.Type == "userId");

            if (userIdClaim == null ||
                !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return null;
            }

            return userId;
        }
        catch
        {
            return null;
        }
    }
}
