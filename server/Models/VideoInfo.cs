namespace server.Models;

public class VideoInfo
{
    public string Title { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty; // TikTok, Facebook, Shopee
    public List<DownloadOption> DownloadOptions { get; set; } = new();
}

public class DownloadOption
{
    public string Quality { get; set; } = string.Empty; // HD, SD, Audio
    public string Url { get; set; } = string.Empty;
    public string Type { get; set; } = "video"; // video, audio
}
