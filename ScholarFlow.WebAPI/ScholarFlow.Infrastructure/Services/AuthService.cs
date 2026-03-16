using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ScholarFlow.Application.Common.Interfaces;
using ScholarFlow.Application.Common.Models;
using ScholarFlow.Domain.Entities;
using ScholarFlow.Domain.Enums;
using ScholarFlow.Domain.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ScholarFlow.Infrastructure.Services;

/// <summary>
/// JWT-based authentication service implementation
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly IApplicationDbContext _context;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IConfiguration configuration,
        IApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _context = context;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(
        string email,
        string password,
        string role,
        string? fullName = null,
        string? qualification = null,
        Guid? subjectId = null,
        string? phoneNumber = null)
    {
        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            return Result<AuthResponse>.Failure("User with this email already exists");
        }

        // Create new user
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result<AuthResponse>.Failure($"Registration failed: {errors}");
        }

        // Assign role
        var normalizedRole = string.Empty;
        if (!string.IsNullOrEmpty(role))
        {
            // Normalize role name (ensure uppercase)
            normalizedRole = role.ToUpper();
            
            // Check if role exists, if not create it
            if (!await _roleManager.RoleExistsAsync(normalizedRole))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid>(normalizedRole));
            }
            
            await _userManager.AddToRoleAsync(user, normalizedRole);
        }

        if (normalizedRole == "TEACHER")
        {
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(qualification) || !subjectId.HasValue || string.IsNullOrWhiteSpace(phoneNumber))
            {
                return Result<AuthResponse>.Failure("Teacher registration requires full name, qualification, subject and phone number");
            }

            var subjectExists = await _context.Subjects.AnyAsync(s => s.Id == subjectId.Value);
            if (!subjectExists)
            {
                return Result<AuthResponse>.Failure("Selected subject does not exist");
            }

            user.PhoneNumber = phoneNumber.Trim();
            await _userManager.UpdateAsync(user);

            var profile = new TeacherProfile
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                FullName = fullName,
                Qualification = qualification,
                SubjectId = subjectId.Value,
                PhoneNumber = phoneNumber,
                Bio = string.Empty,
                Status = TeacherRegistrationStatus.Pending
            };

            _context.TeacherProfiles.Add(profile);
            await _context.SaveChangesAsync();

            var teacherRoles = await _userManager.GetRolesAsync(user);
            var teacherRole = teacherRoles.FirstOrDefault() ?? "";

            return Result<AuthResponse>.Success(new AuthResponse
            {
                Token = string.Empty,
                RequiresApproval = true,
                ApprovalStatus = TeacherRegistrationStatus.Pending.ToString(),
                User = new UserDto
                {
                    Id = user.Id.ToString(),
                    Email = user.Email!,
                    UserName = user.UserName!,
                    Role = teacherRole
                }
            });
        }

        // Generate token with all user details
        var token = await GenerateJwtToken(user);
        var roles = await _userManager.GetRolesAsync(user);
        var userRole = roles.FirstOrDefault() ?? "";

        return Result<AuthResponse>.Success(new AuthResponse 
        { 
            Token = token,
            RequiresApproval = false,
            ApprovalStatus = null,
            User = new UserDto
            {
                Id = user.Id.ToString(),
                Email = user.Email!,
                UserName = user.UserName!,
                Role = userRole
            }
        });
    }

    public async Task<Result<AuthResponse>> LoginAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return Result<AuthResponse>.Failure("Invalid email or password");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
        if (!result.Succeeded)
        {
            return Result<AuthResponse>.Failure("Invalid email or password");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var userRole = roles.FirstOrDefault() ?? "";

        if (string.Equals(userRole, "TEACHER", StringComparison.OrdinalIgnoreCase))
        {
            var teacherProfile = await _context.TeacherProfiles
                .FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (teacherProfile == null)
            {
                return Result<AuthResponse>.Failure("Teacher profile is not found. Contact admin.");
            }

            if (teacherProfile.Status == TeacherRegistrationStatus.Pending)
            {
                return Result<AuthResponse>.Failure("Your account is pending admin approval.");
            }

            if (teacherProfile.Status == TeacherRegistrationStatus.Rejected)
            {
                var message = string.IsNullOrWhiteSpace(teacherProfile.RejectionReason)
                    ? "Your teacher registration was rejected."
                    : $"Your teacher registration was rejected: {teacherProfile.RejectionReason}";
                return Result<AuthResponse>.Failure(message);
            }
        }

        // Generate token with all user details
        var token = await GenerateJwtToken(user);
        string? teacherCode = null;

        if (string.Equals(userRole, "TEACHER", StringComparison.OrdinalIgnoreCase))
        {
            teacherCode = await _context.TeacherProfiles
                .Where(t => t.UserId == user.Id)
                .Select(t => t.TeacherCode)
                .FirstOrDefaultAsync();
        }

        return Result<AuthResponse>.Success(new AuthResponse 
        { 
            Token = token,
            RequiresApproval = false,
            ApprovalStatus = null,
            User = new UserDto
            {
                Id = user.Id.ToString(),
                Email = user.Email!,
                UserName = user.UserName!,
                Role = userRole,
                TeacherCode = teacherCode
            }
        });
    }

    private async Task<string> GenerateJwtToken(ApplicationUser user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secret = jwtSettings["Secret"]!;
        var issuer = jwtSettings["Issuer"]!;
        var audience = jwtSettings["Audience"]!;
        var expiryInMinutes = int.Parse(jwtSettings["ExpiryInMinutes"]!);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var roles = await _userManager.GetRolesAsync(user);
        var hasProfile = _context.StudentProfiles.Any(s => s.UserId == user.Id);

        // Add all user details as claims in the token
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("userId", user.Id.ToString()),
            new Claim("username", user.UserName!),
            new Claim("email", user.Email!),
            new Claim("isProfileComplete", hasProfile ? "true" : "false")
        };

        // Add roles to claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
            claims.Add(new Claim("role", role)); // Also add as custom claim for easier access
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryInMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
