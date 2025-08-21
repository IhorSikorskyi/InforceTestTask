namespace InforceTestTask.Models;

public class Users
{
    public int id { get; set; }
    public string user_name { get; set; } = string.Empty;
    public string email { get; set; } = string.Empty;
    public string hashed_password { get; set; } = string.Empty;
    public string role { get; set; } = "User";
    public string? refresh_token { get; set; }
    public DateTime? refresh_token_expiry_time { get; set; }

    public List<ShortURLs> short_urls_list { get; set; } = new();
}