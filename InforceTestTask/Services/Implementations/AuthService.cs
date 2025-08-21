using InforceTestTask.DTOs.Requests;
using InforceTestTask.DTOs.Responses;
using InforceTestTask.Models;
using InforceTestTask.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace InforceTestTask.Services.Implementations;

public class AuthService(TestTaskAppDbContext dbContext, IConfiguration configuration) : IAuthService
{
    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        if (await dbContext.Users.AnyAsync(u => u.user_name == request.UserName))
        {
            return null;
        }

        if (request.Password != request.ConfirmPassword)
        {
            return null;
        }

        var user = new Users();
        user.user_name = request.UserName;
        user.email = request.Email;
        user.role = request.Role;
        user.hashed_password = HashPassword(user, request.Password);

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var response = await CreateTokenResponse(user);
        return response;
    }
    
    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.user_name == request.UserName);
        if (user == null)
        {
            return null;
        }
        var verify = new PasswordHasher<Users>().VerifyHashedPassword(user, user.hashed_password, request.Password);
        if (verify == PasswordVerificationResult.Failed)
        {
            return null;
        }
        var response = await CreateTokenResponse(user);
        return response;
    }
    
    public async Task<AuthResponse?> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var user = await ValidateRefreshTokenASync(request.Id, request.AccessToken);
        if (user == null)
        {
            return null;
        }

        var response = await CreateTokenResponse(user);

        return response;
    }

    private async Task<AuthResponse> CreateTokenResponse(Users user)
    {
        var response = new AuthResponse
        {
            AccessToken = CreateToken(user),
            RefreshToken = await GenerateAndSaveRefreshTokenAsync(user),
        };
        return response;
    }

    public async Task<UserIdRoleResponse?> GetUserIdAsync(UserIdRoleRequest request)
    {
        var result = await dbContext.Users.AsNoTracking()
            .Where(u => u.user_name == request.UserName)
            .Select(s=> new UserIdRoleResponse
            {
                UserId = s.id,
                Role = s.role
            }).FirstOrDefaultAsync();
        
        return result;
    }

    private static string HashPassword(Users user, string password)
    {
        string hashedPassword = new PasswordHasher<Users>().HashPassword(user, password);
        return hashedPassword;
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
    
    private async Task<string> GenerateAndSaveRefreshTokenAsync(Users user)
    {
        var refreshToken = GenerateRefreshToken();
        user.refresh_token = refreshToken;
        user.refresh_token_expiry_time = DateTime.UtcNow.AddDays(7);
        await dbContext.SaveChangesAsync();
        return refreshToken;
    }
    
    private string CreateToken(Users user)
    {
        var claims = new List<Claim>
        {
            new (ClaimTypes.Name, user.user_name),
            new (ClaimTypes.NameIdentifier, user.id.ToString()),
            new (ClaimTypes.Email, user.email),
            new (ClaimTypes.Role, user.role)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration.GetValue<string>("AppSettings:AccessToken")!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = configuration.GetValue<string>("AppSettings:Issuer"),
            Audience = configuration.GetValue<string>("AppSettings:Audience"),
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddMinutes(10),
            SigningCredentials = creds
        };

        var tokenHandler = new JsonWebTokenHandler();
        string jwt = tokenHandler.CreateToken(tokenDescriptor);

        return jwt;
    }
    
    private async Task<Users?> ValidateRefreshTokenASync(int userId, string refreshToken)
    {
        var user = await dbContext.Users.FindAsync(userId);
        if (user == null || user.refresh_token != refreshToken 
                         || user.refresh_token_expiry_time <= DateTime.UtcNow)
        {
            return null;
        }

        return user;
    }
}