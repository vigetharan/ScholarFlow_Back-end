using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ScholarFlow.Application.Common.Interfaces;
using ScholarFlow.Application.Common.Models;
using ScholarFlow.Domain.Entities;
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

    public async Task<Result<AuthResponse>> RegisterAsync(string email, string password, string role)
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
        if (!string.IsNullOrEmpty(role))
        {
            // Normalize role name (ensure uppercase)
            var normalizedRole = role.ToUpper();
            
            // Check if role exists, if not create it
            if (!await _roleManager.RoleExistsAsync(normalizedRole))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid>(normalizedRole));
            }
            
            await _userManager.AddToRoleAsync(user, normalizedRole);
        }

        // Generate token with all user details
        var token = await GenerateJwtToken(user);
        var roles = await _userManager.GetRolesAsync(user);
        var userRole = roles.FirstOrDefault() ?? "";

        return Result<AuthResponse>.Success(new AuthResponse 
        { 
            Token = token,
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

        // Generate token with all user details
        var token = await GenerateJwtToken(user);
        var roles = await _userManager.GetRolesAsync(user);
        var userRole = roles.FirstOrDefault() ?? "";

        return Result<AuthResponse>.Success(new AuthResponse 
        { 
            Token = token,
            User = new UserDto
            {
                Id = user.Id.ToString(),
                Email = user.Email!,
                UserName = user.UserName!,
                Role = userRole
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
