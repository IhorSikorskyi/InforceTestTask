namespace InforceTestTask.DTOs.Responses;

public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

public class UserIdRoleResponse
{
    public int UserId { get; set; } = 0;
    public string Role { get; set; } = "User";
}