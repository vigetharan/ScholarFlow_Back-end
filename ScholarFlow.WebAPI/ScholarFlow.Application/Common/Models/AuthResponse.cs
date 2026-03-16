namespace ScholarFlow.Application.Common.Models;

/// <summary>
/// JWT authentication response - Token contains all user information
/// </summary>
public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = null!;
    public bool RequiresApproval { get; set; }
    public string? ApprovalStatus { get; set; }
}

/// <summary>
/// User response model for authentication
/// </summary>
public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? TeacherCode { get; set; }
}
