using System.Diagnostics;
using System.Text.Json;
using server.Models;

namespace server.Services;

public class YtDlpService : IYtDlpService
{
    // Auto-detect yt-dlp binary based on OS (Windows: yt-dlp.exe, Linux/Docker: yt-dlp)
    private static readonly string YtDlpPath = OperatingSystem.IsWindows() ? "yt-dlp.exe" : "yt-dlp";

    public async Task<VideoInfo> GetVideoInfoAsync(string url)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = YtDlpPath,
            Arguments = $"--dump-json \"{url}\"",
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
            throw new Exception($"yt-dlp failed: {error}");
        }

        return ParseYtDlpOutput(output, url);
    }

    private VideoInfo ParseYtDlpOutput(string json, string originalUrl)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        var videoInfo = new VideoInfo
        {
            Title = root.GetProperty("title").GetString() ?? "Unknown Title",
            ThumbnailUrl = root.GetProperty("thumbnail").GetString() ?? "",
            Platform = root.TryGetProperty("extractor_key", out var extractor) ? extractor.GetString() ?? "Unknown" : "Unknown"
        };

        // Extract formats
        if (root.TryGetProperty("formats", out var formats))
        {
            foreach (var format in formats.EnumerateArray())
            {
                // Filter for relevant formats (e.g., mp4, audio)
                // This is a simplified logic, yt-dlp formats can be complex
                var ext = format.TryGetProperty("ext", out var e) ? e.GetString() : "";
                var url = format.TryGetProperty("url", out var u) ? u.GetString() : "";
                var formatId = format.TryGetProperty("format_id", out var f) ? f.GetString() : "";
                var resolution = format.TryGetProperty("resolution", out var r) ? r.GetString() : "Unknown";

                if (!string.IsNullOrEmpty(url))
                {
                    if (ext == "mp4")
                    {
                        videoInfo.DownloadOptions.Add(new DownloadOption
                        {
                            Quality = resolution,
                            Url = url,
                            Type = "video"
                        });
                    }
                    else if (ext == "m4a" || ext == "mp3")
                    {
                        videoInfo.DownloadOptions.Add(new DownloadOption
                        {
                            Quality = "Audio",
                            Url = url,
                            Type = "audio"
                        });
                    }
                }
            }
        }
        
        // Fallback if no formats found (some extractors provide direct url in 'url' field)
        if (videoInfo.DownloadOptions.Count == 0 && root.TryGetProperty("url", out var directUrl))
        {
             videoInfo.DownloadOptions.Add(new DownloadOption
             {
                 Quality = "Default",
                 Url = directUrl.GetString() ?? "",
                 Type = "video"
             });
        }

        return videoInfo;
    }
}
