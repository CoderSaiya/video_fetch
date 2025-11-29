using server.Models;

namespace server.Services;

public interface IYtDlpService
{
    Task<VideoInfo> GetVideoInfoAsync(string url);
}
