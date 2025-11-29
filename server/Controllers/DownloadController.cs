using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace server.Controllers;

/// <summary>
/// Controller for downloading videos using yt-dlp to bypass anti-bot protection
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DownloadController : ControllerBase
{
    private readonly ILogger<DownloadController> _logger;
    private const string YtDlpPath = "yt-dlp.exe";

    public DownloadController(ILogger<DownloadController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Download video using yt-dlp
    /// </summary>
    /// <param name="url">The original video URL (e.g., TikTok video page URL)</param>
    /// <param name="quality">Optional quality preference (e.g., 1080p, 720p, or 'best')</param>
    /// <returns>Video file stream</returns>
    [HttpGet]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Get([FromQuery] string url, [FromQuery] string? quality = "best")
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return BadRequest(new { message = "URL is required" });
        }

        _logger.LogInformation("Downloading video from URL: {Url} with quality: {Quality}", url, quality);

        var tempFilePath = Path.Combine(Path.GetTempPath(), $"video_{Guid.NewGuid()}.mp4");

        try
        {
            // Use yt-dlp to download the video
            var formatSelection = quality?.ToLower() == "best" || string.IsNullOrWhiteSpace(quality) 
                ? "best[ext=mp4]/best" 
                : $"bestvideo[height<={quality?.Replace("p", "")}][ext=mp4]+bestaudio/best[ext=mp4]/best";

            var arguments = $"-f \"{formatSelection}\" -o \"{tempFilePath}\" --no-playlist --merge-output-format mp4 \"{url}\"";

            _logger.LogInformation("Running yt-dlp with arguments: {Arguments}", arguments);

            var processStartInfo = new ProcessStartInfo
            {
                FileName = YtDlpPath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processStartInfo };
            process.Start();

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                _logger.LogError("yt-dlp failed with exit code {ExitCode}. Error: {Error}", process.ExitCode, error);
                return StatusCode(500, new { message = "Failed to download video", detail = error });
            }

            _logger.LogInformation("Video downloaded successfully to: {FilePath}", tempFilePath);

            // Check if file exists
            if (!System.IO.File.Exists(tempFilePath))
            {
                _logger.LogError("Downloaded file not found at path: {FilePath}", tempFilePath);
                return StatusCode(500, new { message = "Downloaded file not found" });
            }

            // Get file info
            var fileInfo = new FileInfo(tempFilePath);
            _logger.LogInformation("File size: {FileSize} bytes", fileInfo.Length);

            // Read the file into memory stream (for automatic cleanup)
            var memoryStream = new MemoryStream();
            using (var fileStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read))
            {
                await fileStream.CopyToAsync(memoryStream);
            }
            memoryStream.Position = 0;

            // Delete temp file
            try
            {
                System.IO.File.Delete(tempFilePath);
                _logger.LogInformation("Temporary file deleted: {FilePath}", tempFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete temporary file: {FilePath}", tempFilePath);
            }

            return File(memoryStream, "video/mp4", "video.mp4");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error downloading video from URL: {Url}", url);
            
            // Cleanup temp file if it exists
            try
            {
                if (System.IO.File.Exists(tempFilePath))
                {
                    System.IO.File.Delete(tempFilePath);
                }
            }
            catch { }

            return StatusCode(500, new { message = "An error occurred while downloading the video", detail = ex.Message });
        }
    }
}
