using server.Models;

namespace server.Services;

public interface IInfoService
{
    Task<VideoInfo> GetVideoInfoAsync(string url);
}
