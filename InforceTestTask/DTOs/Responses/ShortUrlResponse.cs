namespace InforceTestTask.DTOs.Responses;

public class ShortUrlTableResponse
{
    public int UrlId { get; set; }
    public string LongUrl { get; set; } = string.Empty;
    public string ShortUrl { get; set; } = string.Empty;
    public int CreatedByUserId { get; set; } = 0;
}

public class ShortUrlInfoResponse
{
    public int UrlId { get; set; }
    public string LongUrl { get; set; } = string.Empty;
    public string ShortUrl { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
}