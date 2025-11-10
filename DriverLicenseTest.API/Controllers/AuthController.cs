using Application.Email;
using DriverLicenseTest.Application.Interfaces;
using DriverLicenseTest.Domain.Entities;
using DriverLicenseTest.Shared.DTOs;
using DriverLicenseTest.Shared.DTOs.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
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
            PhoneNumber = model.PhoneNumber,
            PhoneNumberConfirmed = true,
            TwoFactorEnabled = false
        };
        user.PasswordHash = hasher.HashPassword(user, model.Password);

        var defaultRole = await _unitOfWork.Roles.GetOneAsync(r => r.Name == "User");

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
        try
        {
            var hasher = new PasswordHasher<AspNetUser>();
            var normalizedUser = model.Username.ToUpperInvariant();

            // Get user with roles included
            var user = await _unitOfWork.Users.GetOneAsync(
                filter: u => u.NormalizedUserName == normalizedUser,
                include: u => u.Include(x => x.Roles)  // Include roles navigation property
            );

            if (user == null)
                return Unauthorized(new MessageResponse { Message = "Invalid credentials." });

            if (!user.EmailConfirmed)
                return Unauthorized(new MessageResponse { Message = "Email not confirmed." });

            var verify = hasher.VerifyHashedPassword(user, user.PasswordHash ?? "", model.Password);
            if (verify == PasswordVerificationResult.Failed)
                return Unauthorized(new MessageResponse { Message = "Invalid credentials." });


            var jwt = await CreateAuthToken(user);

            return Ok(new LoginResponse
            {
                Phone = user.PhoneNumber,
                UserId = user.Id,
                Email = user.Email!,
                FullName = user.UserName!,
                MemberSince = user.CreatedAt.ToString("yyyy-MM-dd"),
                Token = jwt
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new MessageResponse { Message = $"Login failed: {ex.Message}" });
        }
    }
    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _unitOfWork.Users.GetByIdAsync(userId);

        if (user == null)
            return NotFound(new MessageResponse { Message = "User not found." });

        var profile = new UserProfileDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            FullName = user.FullName,
        };

        return Ok(profile);
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(UserProfileDto model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _unitOfWork.Users.GetByIdAsync(userId);

        if (user == null)
            return NotFound(new MessageResponse { Message = "User not found." });

        // Check if username is being changed and if it's already taken
        if (user.UserName != model.UserName)
        {
            var exists = await _unitOfWork.Users.GetOneAsync(
                u => u.NormalizedUserName == model.UserName.ToUpperInvariant() && u.Id != userId);
            if (exists != null)
                return BadRequest(new MessageResponse { Message = "Username is already taken." });

            user.UserName = model.UserName;
            user.NormalizedUserName = model.UserName.ToUpperInvariant();
        }

        user.PhoneNumber = model.PhoneNumber;
        user.FullName = model.FullName;
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new MessageResponse { Message = "Profile updated successfully." });
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _unitOfWork.Users.GetByIdAsync(userId);

        if (user == null)
            return NotFound(new MessageResponse { Message = "User not found." });

        var hasher = new PasswordHasher<AspNetUser>();
        var verifyResult = hasher.VerifyHashedPassword(user, user.PasswordHash ?? "", model.CurrentPassword);

        if (verifyResult == PasswordVerificationResult.Failed)
            return BadRequest(new MessageResponse { Message = "Current password is incorrect." });

        user.PasswordHash = hasher.HashPassword(user, model.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new MessageResponse { Message = "Password changed successfully." });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto model)
    {
        var user = await _unitOfWork.Users.GetOneAsync(u => u.NormalizedEmail == model.Email.ToUpperInvariant());
        if (user == null)
            return Ok(new MessageResponse { Message = "If the email exists, a password reset link will be sent." });

        var token = CreateEmailToken(user.Id, TimeSpan.FromHours(1));
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var callbackUrl = Url.Action("ResetPassword", "Auth", new { token = encodedToken }, Request.Scheme);

        await _emailSender.SendConfirmationEmailAsync(
            user.Email!,
            "Reset your password",
            $"Please reset your password by <a href='{callbackUrl}'>clicking here</a>."
        );

        return Ok(new MessageResponse { Message = "If the email exists, a password reset link will be sent." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
    {
        var raw = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
        var principal = ValidateToken(raw, requirePurpose: "reset-password");
        if (principal == null)
            return BadRequest(new MessageResponse { Message = "Invalid or expired token." });

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var user = await _unitOfWork.Users.GetOneAsync(u => u.Id == userId &&
                                                           u.NormalizedEmail == model.Email.ToUpperInvariant());

        if (user == null)
            return BadRequest(new MessageResponse { Message = "Invalid user or email." });

        var hasher = new PasswordHasher<AspNetUser>();
        user.PasswordHash = hasher.HashPassword(user, model.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new MessageResponse { Message = "Password has been reset successfully." });
    }

    private async Task<string> CreateAuthToken(AspNetUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Create base claims
        var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, user.Id),
        new(ClaimTypes.Name, user.UserName ?? string.Empty),
        new(ClaimTypes.Email, user.Email ?? string.Empty)
    };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
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

    private sealed class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Phone { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string MemberSince { get; set; }
    }
    private sealed class MessageResponse { public string Message { get; set; } = string.Empty; }
}