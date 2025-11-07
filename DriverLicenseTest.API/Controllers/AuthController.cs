using Application.Email;
using DriverLicenseTest.Application.Interfaces;
using DriverLicenseTest.Domain.Entities;
using DriverLicenseTest.Shared.DTOs.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailSender;

    public AuthController(
        IUnitOfWork unitOfWork,
        IConfiguration config,
        IEmailService emailSender)
    {
        _unitOfWork = unitOfWork;
        _configuration = config;
        _emailSender = emailSender;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto model)
    {
        var normalizedUser = model.Username.ToUpperInvariant();
        var normalizedEmail = model.Email.ToUpperInvariant();
        var hasher = new PasswordHasher<AspNetUser>();

        var exists = await _unitOfWork.Users.GetOneAsync(
            u => u.NormalizedUserName == normalizedUser || u.NormalizedEmail == normalizedEmail);
        if (exists != null) return BadRequest(new MessageResponse { Message = "Username or email already exists." });

        var user = new AspNetUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = model.Username,
            NormalizedUserName = normalizedUser,
            Email = model.Email,
            NormalizedEmail = normalizedEmail,
            EmailConfirmed = false,
            SecurityStamp = Guid.NewGuid().ToString("N"),
            ConcurrencyStamp = Guid.NewGuid().ToString("N"),
            CreatedAt = DateTime.UtcNow,
            AccessFailedCount = 0,
            LockoutEnabled = false,
            PhoneNumberConfirmed = false,
            TwoFactorEnabled = false
        };
        user.PasswordHash = hasher.HashPassword(user, model.Password);

        await _unitOfWork.Users.AddAsync(user);

        var confirmToken = CreateEmailToken(user.Id, TimeSpan.FromHours(24));
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(confirmToken));
        var callbackUrl = Url.Action(nameof(ConfirmEmail), "Auth", new { token = encodedToken }, Request.Scheme);

        await _emailSender.SendConfirmationEmailAsync(
            user.Email!,
            "Confirm your email",
            $"Please confirm your account by <a href='{callbackUrl}'>clicking here</a>."
        );

        return Ok(new MessageResponse { Message = "Registered. Please check your email to confirm the account." });
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(string token)
    {
        var raw = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        var principal = ValidateToken(raw, requirePurpose: "confirm-email");
        if (principal == null) return BadRequest(new MessageResponse { Message = "Invalid or expired token." });

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var user = await _unitOfWork.Users.GetOneAsync(u => u.Id == userId);
        if (user == null) return BadRequest(new MessageResponse { Message = "Invalid user." });

        if (!user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();
        }

        return Ok(new MessageResponse { Message = "Email confirmed." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto model)
    {
        var hasher = new PasswordHasher<AspNetUser>();
        var normalizedUser = model.Username.ToUpperInvariant();
        var user = await _unitOfWork.Users.GetOneAsync(u => u.NormalizedUserName == normalizedUser);
        if (user == null) return Unauthorized(new MessageResponse { Message = "Invalid credentials." });

        if (!user.EmailConfirmed) return Unauthorized(new MessageResponse { Message = "Email not confirmed." });

        var verify = hasher.VerifyHashedPassword(user, user.PasswordHash ?? "", model.Password);
        if (verify == PasswordVerificationResult.Failed)
            return Unauthorized(new MessageResponse { Message = "Invalid credentials." });

        var jwt = CreateAuthToken(user);
        return Ok(new TokenResponse { Token = jwt });
    }

    private string CreateEmailToken(string userId, TimeSpan lifetime)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim("purpose", "confirm-email")
        };
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.Add(lifetime),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private ClaimsPrincipal? ValidateToken(string token, string requirePurpose)
    {
        var handler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        try
        {
            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.Zero,
                ValidateLifetime = true
            }, out _);

            var purpose = principal.FindFirstValue("purpose");
            return purpose == requirePurpose ? principal : null;
        }
        catch
        {
            return null;
        }
    }

    private string CreateAuthToken(AspNetUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]  
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty)
        };
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    private sealed class TokenResponse { public string Token { get; set; } = string.Empty; }
    private sealed class MessageResponse { public string Message { get; set; } = string.Empty; }
}