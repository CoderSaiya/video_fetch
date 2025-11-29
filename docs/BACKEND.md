# Backend Documentation

## Overview

The backend is a .NET 10 Web API that handles video information retrieval and downloading using yt-dlp.

## Project Structure

```
server/
├── Controllers/
│   ├── InfoController.cs         # Video info endpoint
│   └── DownloadController.cs     # Download endpoint
├── Services/
│   ├── IYtDlpService.cs         # yt-dlp interface
│   ├── YtDlpService.cs          # yt-dlp implementation
│   ├── IInfoService.cs          # Info service interface
│   └── InfoService.cs           # Info implementation
├── Models/
│   └── VideoInfo.cs             # Data models
├── Properties/
│   └── launchSettings.json      # Launch configuration
├── Program.cs                   # App startup & config
├── server.csproj                # Project file
└── yt-dlp.exe                   # Video extraction tool
```

## Controllers

### InfoController

**File:** [`Controllers/InfoController.cs`](file:///c:/Users/admin/Documents/WorkSpace/ProjectNET/git/video_fetch/server/Controllers/InfoController.cs)

**Route:** `/api/Info`

**Purpose:** Retrieves video metadata and download options.

**Implementation:**
```csharp
[ApiController]
[Route("api/[controller]")]
public class InfoController : ControllerBase
{
    private readonly IInfoService _infoService;
    
    [HttpPost]
    [ProducesResponseType(typeof(VideoInfo), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetVideoInfo([FromBody] VideoRequest request)
    {
        var videoInfo = await _infoService.GetVideoInfoAsync(request.Url);
        return Ok(videoInfo);
    }
}
```

---

### DownloadController

**File:** [`Controllers/DownloadController.cs`](file:///c:/Users/admin/Documents/WorkSpace/ProjectNET/git/video_fetch/server/Controllers/DownloadController.cs)

**Route:** `/api/Download`

**Purpose:** Downloads videos using yt-dlp.

**Implementation:**
```csharp
[HttpGet]
public async Task<IActionResult> Get([FromQuery] string url, [FromQuery] string? quality = "best")
{
    // Download video using yt-dlp
    var tempFilePath = Path.Combine(Path.GetTempPath(), $"video_{Guid.NewGuid()}.mp4");
    
    // Execute yt-dlp
    // Stream to client
    // Cleanup temp file
    
    return File(memoryStream, "video/mp4", "video.mp4");
}
```

**Key Features:**
- Quality selection support
- Temporary file management
- Memory stream for efficient transfer
- Automatic cleanup

---

## Services

### YtDlpService

**File:** [`Services/YtDlpService.cs`](file:///c:/Users/admin/Documents/WorkSpace/ProjectNET/git/video_fetch/server/Services/YtDlpService.cs)

**Interface:** `IYtDlpService`

**Purpose:** Wrapper for yt-dlp operations.

**Methods:**

#### GetVideoInfoAsync
```csharp
public async Task<VideoInfo> GetVideoInfoAsync(string url)
{
    var processStartInfo = new ProcessStartInfo
    {
        FileName = "yt-dlp.exe",
        Arguments = $"--dump-json \"{url}\"",
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };
    
    // Execute and parse JSON output
}
```

**Features:**
- JSON metadata extraction
- Format parsing
- Error handling
- Platform detection

---

### InfoService

**File:** [`Services/InfoService.cs`](file:///c:/Users/admin/Documents/WorkSpace/ProjectNET/git/video_fetch/server/Services/InfoService.cs)

**Interface:** `IInfoService`

**Purpose:** Business logic for video information.

**Implementation:**
```csharp
public class InfoService : IInfoService
{
    private readonly IYtDlpService _ytDlpService;
    
    public async Task<VideoInfo> GetVideoInfoAsync(string url)
    {
        return await _ytDlpService.GetVideoInfoAsync(url);
    }
}
```

---

## Models

### VideoInfo

**File:** [`Models/VideoInfo.cs`](file:///c:/Users/admin/Documents/WorkSpace/ProjectNET/git/video_fetch/server/Models/VideoInfo.cs)

```csharp
public class VideoInfo
{
    public string Title { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public List<DownloadOption> DownloadOptions { get; set; } = new();
}

public class DownloadOption
{
    public string Quality { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Type { get; set; } = "video";
}
```

**JSON Serialization:**
- Uses camelCase naming (configured in Program.cs)
- Nullable reference types enabled

---

## Configuration

### Program.cs

**File:** [`Program.cs`](file:///c:/Users/admin/Documents/WorkSpace/ProjectNET/git/video_fetch/server/Program.cs)

**Key Configurations:**

#### 1. JSON Serialization
```csharp
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = 
            System.Text.Json.JsonNamingPolicy.CamelCase;
    });
```

#### 2. CORS Policy
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});
```

> **Production:** Replace `AllowAnyOrigin()` with specific frontend domain.

#### 3. Dependency Injection
```csharp
builder.Services.AddHttpClient();
builder.Services.AddScoped<IYtDlpService, YtDlpService>();
builder.Services.AddScoped<IInfoService, InfoService>();
```

#### 4. Swagger/OpenAPI
```csharp
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});
```

---

## yt-dlp Integration

### Installation

Download yt-dlp.exe and place in server directory:
```bash
curl -L https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe -o yt-dlp.exe
```

### Usage Patterns

#### Get Video Info
```bash
yt-dlp --dump-json "https://www.tiktok.com/@user/video/123"
```

#### Download Video
```bash
yt-dlp -f "best[ext=mp4]" -o "output.mp4" "https://..."
```

#### Format Selection
```bash
# Best quality
yt-dlp -f "best[ext=mp4]/best"

# Specific resolution
yt-dlp -f "bestvideo[height<=1080]+bestaudio/best"
```

---

## Error Handling

### Exception Types

```csharp
try
{
    var videoInfo = await _ytDlpService.GetVideoInfoAsync(url);
    return Ok(videoInfo);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Failed to get video info for URL: {Url}", url);
    return StatusCode(500, new { message = "Failed to fetch video info" });
}
```

### Common Errors

1. **yt-dlp not found**
   - Ensure yt-dlp.exe is in server directory
   - Check PATH environment variable

2. **Invalid URL**
   - Validate URL format
   - Check platform support

3. **Network errors**
   - Timeout configuration
   - Retry logic

4. **File system errors**
   - Disk space check
   - Permission issues

---

## Logging

### ILogger Integration

```csharp
private readonly ILogger<DownloadController> _logger;

_logger.LogInformation("Downloading video from URL: {Url}", url);
_logger.LogWarning("Failed to download: {StatusCode}", statusCode);
_logger.LogError(ex, "Error downloading from URL: {Url}", url);
```

### Log Levels
- **Information:** Normal operations
- **Warning:** Recoverable issues
- **Error:** Failures requiring attention

---

## Performance

### Optimization Strategies

1. **Async/Await**
   - All I/O operations are async
   - Non-blocking API calls

2. **Memory Management**
   - Stream-based file transfer
   - Automatic temp file cleanup
   - Using blocks for IDisposable

3. **Process Management**
   - CreateNoWindow for yt-dlp
   - Proper process disposal
   - Timeout handling

---

## Security

### Best Practices

1. **Input Validation**
```csharp
if (string.IsNullOrWhiteSpace(url))
{
    return BadRequest(new { message = "URL is required" });
}
```

2. **CORS Configuration**
```csharp
// Production: Restrict to specific origins
builder.WithOrigins("https://your-frontend.vercel.app")
```

3. **File Cleanup**
```csharp
try
{
    if (File.Exists(tempFilePath))
    {
        File.Delete(tempFilePath);
    }
}
catch (Exception ex)
{
    _logger.LogWarning(ex, "Failed to delete temp file");
}
```

---

## Testing

### Unit Tests (Future)

```csharp
[Fact]
public async Task GetVideoInfo_ValidUrl_ReturnsVideoInfo()
{
    // Arrange
    var mockService = new Mock<IInfoService>();
    var controller = new InfoController(mockService.Object);
    
    // Act
    var result = await controller.GetVideoInfo(new VideoRequest 
    { 
        Url = "https://..." 
    });
    
    // Assert
    Assert.IsType<OkObjectResult>(result);
}
```

---

## Deployment

### Prerequisites

1. .NET 10 Runtime
2. yt-dlp.exe in deployment package
3. HTTPS certificate

### Azure App Service

```bash
# Publish
dotnet publish -c Release -o ./publish

# Deploy to Azure
az webapp deploy --resource-group <group> --name <app> --src-path ./publish.zip
```

### Docker (Future)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY publish/ .
# Install yt-dlp
RUN curl -L https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp -o /usr/local/bin/yt-dlp
RUN chmod a+rx /usr/local/bin/yt-dlp
ENTRYPOINT ["dotnet", "server.dll"]
```

---

## Monitoring

### Health Checks (Future)

```csharp
builder.Services.AddHealthChecks()
    .AddCheck("yt-dlp", () =>
    {
        // Check if yt-dlp is accessible
    });
```

### Application Insights

```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

---

## API Documentation

### Swagger UI

Access at: `https://localhost:8084/swagger`

**Features:**
- Interactive API testing
- Request/response schemas
- Authentication (when implemented)

---

## Future Enhancements

1. **Caching Layer**
   - Redis for video metadata
   - Reduce yt-dlp calls

2. **Background Jobs**
   - Queue-based downloads
   - Hangfire integration

3. **Rate Limiting**
   - AspNetCoreRateLimit
   - Per-IP throttling

4. **Authentication**
   - JWT tokens
   - API keys

5. **File Storage**
   - Azure Blob Storage
   - S3 integration
