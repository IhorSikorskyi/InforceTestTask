using InforceTestTask.DTOs.Requests;
using InforceTestTask.DTOs.Responses;

namespace InforceTestTask.Services.Interfaces;

public interface IUrlShortenerService
{
    Task<ShortUrlTableResponse?> CreateShortUrlAsync(ShortUrlTableRequest request);
    Task<ShortUrlInfoResponse?> GetShortUrlInfoAsync(ShortUrlInfoRequest request);
    Task<string?> OpenShortUrlAsync(ShortUrlOpenRequest request);
    Task<bool> DeleteShortUrlAsync(ShortUrlDeleteRequest request);
    Task<List<ShortUrlTableResponse>> GetAllShortUrlsAsync();
}