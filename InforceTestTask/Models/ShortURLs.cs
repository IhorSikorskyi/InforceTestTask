namespace InforceTestTask.Models;

public class ShortURLs
{
    public int id { get; set; }
    public string long_url { get; set; } = string.Empty;
    public string short_url { get; set; } = string.Empty;
    public string code { get; set; } = string.Empty;
    public string created_by { get; set; } = string.Empty;
    public DateTime created_date { get; set; } = DateTime.Now;

    public Users user { get; set; } = new();
}