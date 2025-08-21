using InforceTestTask.DTOs.Requests;
using InforceTestTask.DTOs.Responses;
using InforceTestTask.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InforceTestTask.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShortUrlController(IUrlShortenerService urlShortenerService) : ControllerBase
    {
        [Authorize]
        [HttpPost("create")]
        public async Task<ActionResult<ShortUrlTableResponse>> CreateShortUrl([FromBody] ShortUrlTableRequest request)
        {
            try
            {
                var result = await urlShortenerService.CreateShortUrlAsync(request);
                if (result == null)
                    return BadRequest("Failed to create short URL.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("info/{urlId}")]
        public async Task<ActionResult<ShortUrlInfoResponse>> GetShortUrlInfo(int urlId)
        {
            var result = await urlShortenerService.GetShortUrlInfoAsync(
                new ShortUrlInfoRequest { UrlId = urlId }
            );

            if (result == null)
                return NotFound("Short URL not found or access denied.");
            return Ok(result);
        }

        [Authorize]
        [HttpPost("remove")]
        public async Task<ActionResult<bool>> DeleteShortUrl([FromBody] ShortUrlDeleteRequest request)
        {
            var result = await urlShortenerService.DeleteShortUrlAsync(request);
            if (!result)
                return NotFound("Short URL not found or access denied.");
            return Ok(result);
        }

        [HttpGet("{code}")]
        public async Task<IActionResult> RedirectToLongUrl(string code)
        {
            var longUrl = await urlShortenerService.OpenShortUrlAsync(new ShortUrlOpenRequest { Code = code });
            if (longUrl == null)
                return NotFound("Short URL not found.");

            return Redirect(longUrl);
        }
        
        [HttpGet("all")]
        public async Task<ActionResult<List<ShortUrlTableResponse>>> GetAllShortUrls()
        {
            var result = await urlShortenerService.GetAllShortUrlsAsync();
            return Ok(result);
        }
    }
}