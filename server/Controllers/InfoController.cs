using Microsoft.AspNetCore.Mvc;
using server.Models;
using server.Services;

namespace server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InfoController : ControllerBase
{
    private readonly IInfoService _infoService;

    public InfoController(IInfoService infoService)
    {
        _infoService = infoService;
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(VideoInfo), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetInfo([FromBody] UrlRequest request)
    {
        try
        {
            var result = await _infoService.GetVideoInfoAsync(request.Url);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal Server Error", details = ex.Message });
        }
    }
}

public class UrlRequest
{
    public string Url { get; set; } = string.Empty;
}
