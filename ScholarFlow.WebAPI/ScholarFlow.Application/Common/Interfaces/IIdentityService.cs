using ScholarFlow.Domain.Entities.Base;
using ScholarFlow.Application.Common.Models;

namespace ScholarFlow.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<string> GetUserNameAsync(string userId);

    // Common identity methods
    Task<bool> IsInRoleAsync(string userId, string role);
    
    Task<bool> AuthorizeAsync(string userId, string policyName);

    // Note: Ensure your 'Result' class is available in Common.Models
    Task<(Result<string> Result, string UserId)> CreateUserAsync(string userName, string password);

    Task<Result<bool>> DeleteUserAsync(string userId);
}