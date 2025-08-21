using System.Text;
using InforceTestTask.DTOs.Requests;
using InforceTestTask.DTOs.Responses;
using InforceTestTask.Models;
using InforceTestTask.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InforceTestTask.Services.Implementations;

public class UrlShortenerService(TestTaskAppDbContext dbContext) : IUrlShortenerService
{
    private const string Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz+/";
    private static readonly Random Rnd = new();
    private const string BaseUrl = "https://localhost:7033";
    public async Task<ShortUrlTableResponse?> CreateShortUrlAsync(ShortUrlTableRequest request)
    {
        if (string.IsNullOrEmpty(request.LongUrl) || !Uri.IsWellFormedUriString(request.LongUrl, UriKind.Absolute))
        {
            throw new ArgumentException("Invalid URL format.");
        }

        if (await dbContext.ShortURLs.AnyAsync(c => c.long_url == request.LongUrl))
        {
            throw new ArgumentException("This URL is exist.");
        }

        string urlCode = Encode();
        if (await dbContext.ShortURLs.AnyAsync(c => c.code.ToString() == urlCode))
        {
            do
            {
                urlCode = Encode();
            }
            while (await dbContext.ShortURLs.AnyAsync(c => c.code.ToString() == urlCode));
        }

        string? creatorName = await dbContext.Users.AsNoTracking()
            .Where(u => u.id == request.UserId)
            .Select(u => u.user_name)
            .FirstOrDefaultAsync();

        if (string.IsNullOrEmpty(creatorName))
        {
            throw new InvalidOperationException("User not found.");
        }

        var userEntity = await dbContext.Users.FindAsync(request.UserId);
        if (userEntity == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        var url = new ShortURLs();

        url.long_url = request.LongUrl; 
        url.short_url = BaseUrl + "/api/ShortUrl/" + urlCode;
        url.code = urlCode;
        url.created_by = creatorName;
        url.created_date = DateTime.Now;
        url.user = userEntity;

        dbContext.ShortURLs.Add(url);
        await dbContext.SaveChangesAsync();

        var response = new ShortUrlTableResponse
        {
            UrlId = url.id,
            LongUrl = url.long_url,
            ShortUrl = url.short_url,
            CreatedByUserId = request.UserId
        };

        return response;
    }

    public async Task<ShortUrlInfoResponse?> GetShortUrlInfoAsync(ShortUrlInfoRequest request)
    {
        var response = await dbContext.ShortURLs.AsNoTracking()
            .Where(s => s.id == request.UrlId)
            .Select(s => new ShortUrlInfoResponse
            {
                UrlId = s.id,
                LongUrl = s.long_url,
                ShortUrl = s.short_url,
                Code = s.code,
                CreatedBy = s.created_by,
                CreatedDate = s.created_date
            })
            .FirstOrDefaultAsync();

        if (response == null)
            return null;

        return response;
    }

    public async Task<string?> OpenShortUrlAsync(ShortUrlOpenRequest request)
    {
        if (string.IsNullOrEmpty(request.Code))
        {
            throw new ArgumentException("Code cannot be null or empty.");
        }

        var longUrl = await dbContext.ShortURLs
            .AsNoTracking()
            .Where(s => s.code == request.Code)
            .Select(s => s.long_url)
            .FirstOrDefaultAsync();

        return longUrl;
    }

    public async Task<bool> DeleteShortUrlAsync(ShortUrlDeleteRequest request)
    {
        var urlToDelete = await dbContext.ShortURLs
            .Include(s => s.user)
            .Where(s => s.id == request.UrlId)
            .FirstOrDefaultAsync();

        var user = await dbContext.Users.FindAsync(request.UserId);

        if (urlToDelete == null)
            throw new ArgumentException("Code cannot be null or empty.");
        if (user == null)
            throw new ArgumentException("User not found");
        if (urlToDelete != null && user != null && urlToDelete.user.id != request.UserId && user.role != "Admin")
                throw new ArgumentException("No permission");
        if (urlToDelete == null || user == null || (urlToDelete.user.id != request.UserId && user.role != "Admin"))
            return false;

        dbContext.ShortURLs.Remove(urlToDelete);
        await dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<List<ShortUrlTableResponse>> GetAllShortUrlsAsync()
    {
        var result = await dbContext.ShortURLs.AsNoTracking()
            .Select(s => new ShortUrlTableResponse
            {
                UrlId = s.id,
                LongUrl = s.long_url,
                ShortUrl = s.short_url,
                CreatedByUserId = s.user.id
            }).ToListAsync();
        return result;
    }

    private static string Encode()
    {
        int randNumber = Rnd.Next(1, 10);
        var code = new StringBuilder();
        while (randNumber > 0)
        {
            int remainder = randNumber % Alphabet.Length;
            code.Insert(0,Alphabet[remainder]);
            randNumber /= Alphabet.Length;
        }
        return code.ToString();
    }
}