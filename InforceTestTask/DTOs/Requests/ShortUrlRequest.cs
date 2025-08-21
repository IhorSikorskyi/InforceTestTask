namespace InforceTestTask.DTOs.Requests;

public class ShortUrlTableRequest
{
    public int UserId { get; set; }
    public string LongUrl { get; set; } = string.Empty;
}
public class ShortUrlInfoRequest
{
    public int UrlId { get; set; }
}

public class ShortUrlDeleteRequest
{
    public int UrlId { get; set; }
    public int UserId { get; set; }
}

public class ShortUrlOpenRequest
{
    public string Code { get; set; } = string.Empty;
}