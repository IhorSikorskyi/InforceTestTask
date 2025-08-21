namespace InforceTestTask.DTOs.Requests;

public class RefreshTokenRequest
{
    public int Id { get; set; }
    public string AccessToken { get; set; } = string.Empty;
}