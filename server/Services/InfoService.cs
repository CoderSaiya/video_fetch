using server.Models;

namespace server.Services;

public class InfoService : IInfoService
{
    private readonly IYtDlpService _ytDlpService;

    public InfoService(IYtDlpService ytDlpService)
    {
        _ytDlpService = ytDlpService;
    }

    public async Task<VideoInfo> GetVideoInfoAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("URL cannot be empty");
        }

        try 
        {
            return await _ytDlpService.GetVideoInfoAsync(url);
        }
        catch (Exception ex)
        {
            // Log error here
            throw new Exception($"Failed to fetch video info: {ex.Message}");
        }
    }
}
