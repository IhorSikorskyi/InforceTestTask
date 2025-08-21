    using InforceTestTask.DTOs.Requests;
    using InforceTestTask.DTOs.Responses;
    using InforceTestTask.Models;

namespace InforceTestTask.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);
    Task<AuthResponse?> LoginAsync(LoginRequest request);    
    Task<AuthResponse?> RefreshTokenAsync(RefreshTokenRequest request);
    Task<UserIdRoleResponse?> GetUserIdAsync(UserIdRoleRequest request);
}