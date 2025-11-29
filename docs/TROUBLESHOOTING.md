# Troubleshooting Guide

## Common Issues & Solutions

### Frontend Issues

#### 1. Environment Variable Not Working

**Symptom:** API calls fail, `undefined` in console

**Causes:**
- Environment variable not prefixed with `NEXT_PUBLIC_`
- Dev server not restarted after `.env.local` changes
- Typo in variable name

**Solutions:**
```bash
# 1. Check variable name in .env.local
NEXT_PUBLIC_API_URL=https://localhost:8084

# 2. Restart dev server
# Ctrl+C to stop
npm run dev

# 3. Verify in code
console.log(process.env.NEXT_PUBLIC_API_URL);
```

---

#### 2. CORS Error

**Symptom:** 
```
Access to fetch at 'https://localhost:8084/api/Info' from origin 'http://localhost:3000' 
has been blocked by CORS policy
```

**Causes:**
- Backend CORS not configured
- Frontend origin not allowed
- Backend not running

**Solutions:**

**Backend (`Program.cs`):**
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.WithOrigins("http://localhost:3000")
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

// Use CORS
app.UseCors("AllowAll");
```

**Verify backend is running:** `https://localhost:8084/swagger`

---

#### 3. npm Install Fails

**Symptom:** Package installation errors

**Solutions:**
```bash
# Clear cache
npm cache clean --force

# Delete node_modules
rm -rf node_modules package-lock.json

# Reinstall
npm install

# If still fails, update Node.js
node -v  # Should be 18+
```

---

#### 4. Build Errors

**Symptom:** `npm run build` fails

**Common Issues:**
```typescript
// TypeScript errors
// Fix: Add proper types
const data: VideoInfo = await response.json();

// Import errors
// Fix: Check import paths
import { VideoInfo } from "../types";  // Correct
import { VideoInfo } from "types";     // Wrong
```

---

### Backend Issues

#### 1. yt-dlp.exe Not Found

**Symptom:**
```
System.ComponentModel.Win32Exception: The system cannot find the file specified
```

**Solutions:**
```bash
# Download yt-dlp to server directory
cd server
curl -L https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe -o yt-dlp.exe

# Verify file exists
ls yt-dlp.exe

# Make executable (Linux/Mac)
chmod +x yt-dlp.exe
```

---

#### 2. Port Already in Use

**Symptom:**
```
Unable to bind to https://localhost:8084 - port already in use
```

**Solutions:**

**Option 1: Kill process**
```bash
# Windows
netstat -ano | findstr :8084
taskkill /PID <process_id> /F

# Linux/Mac
lsof -ti:8084 | xargs kill
```

**Option 2: Change port**

Edit `launchSettings.json`:
```json
{
  "profiles": {
    "https": {
      "applicationUrl": "https://localhost:8085"
    }
  }
}
```

Update frontend `.env.local`:
```env
NEXT_PUBLIC_API_URL=https://localhost:8085
```

---

#### 3. SSL Certificate Error

**Symptom:**
```
The SSL connection could not be established
```

**Solutions:**
```bash
# Trust development certificate
dotnet dev-certs https --trust

# If still fails, remove and recreate
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

---

#### 4. yt-dlp Fails

**Symptom:**
```
yt-dlp failed: ERROR: Unsupported URL
```

**Causes:**
- Invalid URL format
- Unsupported platform
- Video is private/deleted
- yt-dlp outdated

**Solutions:**
```bash
# Update yt-dlp
yt-dlp.exe -U

# Test URL directly
yt-dlp.exe --dump-json "https://www.tiktok.com/@user/video/123"

# Check supported sites
yt-dlp.exe --list-extractors
```

---

#### 5. Temp File Access Denied

**Symptom:**
```
System.UnauthorizedAccessException: Access to the path is denied
```

**Solutions:**
```csharp
// Use user temp directory
var tempPath = Path.GetTempPath();
var tempFile = Path.Combine(tempPath, $"video_{Guid.NewGuid()}.mp4");

// Ensure cleanup
try
{
    File.Delete(tempFile);
}
catch (Exception ex)
{
    _logger.LogWarning(ex, "Failed to delete temp file");
}
```

---

### API Issues

#### 1. Timeout Errors

**Symptom:** Request takes too long, eventually fails

**Causes:**
- Large video file
- Slow internet
- Server under load

**Solutions:**

**Frontend - Increase timeout:**
```typescript
const controller = new AbortController();
const timeoutId = setTimeout(() => controller.abort(), 120000); // 2 minutes

fetch(url, { signal: controller.signal })
  .finally(() => clearTimeout(timeoutId));
```

**Backend - Increase timeout:**
```csharp
httpClient.Timeout = TimeSpan.FromMinutes(5);
```

---

#### 2. 403 Forbidden on Download

**Symptom:** Download URL returns 403

**Cause:** This is expected - direct URLs are blocked by platforms

**Solution:** Already implemented - we use yt-dlp to download instead of direct URLs

**Verify:** Check `DownloadController` uses yt-dlp:
```csharp
// Should use yt-dlp download, not direct URL
var arguments = $"-f \"{formatSelection}\" -o \"{tempFilePath}\" \"{url}\"";
```

---

#### 3. 500 Internal Server Error

**Symptom:** API returns 500 error

**Debugging:**
```csharp
// Check logs
_logger.LogError(ex, "Error details");

// Check response body
{
  "message": "...",
  "detail": "..."  // Contains specific error
}
```

**Common Causes:**
- yt-dlp execution failed
- File system errors
- Network issues
- Invalid input

---

### Deployment Issues

#### 1. Vercel Build Fails

**Symptom:** Deployment fails during build

**Solutions:**
```bash
# Test build locally
npm run build

# Check for errors
# Fix TypeScript errors
# Fix import paths
# Update dependencies
```

**Common Issues:**
```json
// package.json - ensure correct Node version
{
  "engines": {
    "node": ">=18.0.0"
  }
}
```

---

#### 2. Azure Deployment Fails

**Symptom:** Backend won't start on Azure

**Checklist:**
- [ ] yt-dlp.exe included in publish
- [ ] Correct .NET runtime installed
- [ ] Environment variables set
- [ ] CORS configured for production domain

**Solutions:**
```bash
# Include yt-dlp in publish
# Add to .csproj
<ItemGroup>
  <None Include="yt-dlp.exe" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>

# Verify publish folder
dotnet publish -c Release
ls publish/yt-dlp.exe
```

---

#### 3. Environment Variables Missing

**Symptom:** App can't connect to backend in production

**Solutions:**

**Vercel:**
1. Go to project settings
2. Environment Variables tab
3. Add `NEXT_PUBLIC_API_URL`
4. Redeploy

**Azure:**
```bash
# Set app settings
az webapp config appsettings set \
  --resource-group MyResourceGroup \
  --name MyApp \
  --settings ASPNETCORE_ENVIRONMENT=Production
```

---

### Performance Issues

#### 1. Slow API Response

**Symptoms:** Info endpoint takes >10 seconds

**Diagnosis:**
```csharp
// Add timing logs
var stopwatch = Stopwatch.StartNew();
var result = await _ytDlpService.GetVideoInfoAsync(url);
stopwatch.Stop();
_logger.LogInformation("yt-dlp took {Ms}ms", stopwatch.ElapsedMilliseconds);
```

**Solutions:**
- Cache video info
- Use faster hosting
- Optimize yt-dlp arguments
- Add CDN

---

#### 2. Memory Issues

**Symptom:** Server crashes with large videos

**Solutions:**
```csharp
// Stream directly instead of loading to memory
return File(fileStream, "video/mp4");

// Or chunk the download
const int bufferSize = 81920;
await fileStream.CopyToAsync(outputStream, bufferSize);
```

---

## Debugging Tips

### Frontend Debugging

**Browser Console:**
```javascript
// Check state
console.log({ isLoading, error, videoInfo });

// Check environment
console.log(process.env.NEXT_PUBLIC_API_URL);

// Network tab
// Check request URL, headers, response
```

**React DevTools:**
- Install browser extension
- Inspect component props/state
- Profile performance

---

### Backend Debugging

**Logging:**
```csharp
// Detailed logging
_logger.LogDebug("Processing URL: {Url}", url);
_logger.LogInformation("yt-dlp args: {Args}", arguments);
_logger.LogWarning("Slow response: {Ms}ms", elapsed);
_logger.LogError(ex, "Failed to process");
```

**Swagger Testing:**
1. Navigate to `/swagger`
2. Try out endpoints
3. Check request/response
4. Verify error messages

**VS Code/Rider:**
- Set breakpoints
- Step through code
- Inspect variables
- Watch expressions

---

## Health Checks

### Quick System Check

```bash
# Frontend
curl http://localhost:3000
# Should return HTML

# Backend health
curl https://localhost:8084/swagger
# Should return Swagger UI

# Backend API
curl -X POST https://localhost:8084/api/Info \
  -H "Content-Type: application/json" \
  -d '{"url":"https://www.tiktok.com/@tiktok/video/123"}'
# Should return JSON
```

---

## Getting Help

### Before Asking for Help

1. **Check this guide** for your specific error
2. **Search GitHub Issues** for similar problems
3. **Review logs** for detailed error messages
4. **Test with different videos** to isolate the issue
5. **Verify setup** (all dependencies installed, servers running)

### When Reporting Issues

Include:
```markdown
**Environment:**
- OS: Windows 11
- Node: v18.17.0
- .NET: 10.0.0
- Browser: Chrome 120

**Steps to Reproduce:**
1. Go to '...'
2. Click on '...'
3. See error

**Expected Behavior:**
Should download the video

**Actual Behavior:**
Shows error "..."

**Logs:**
```
[paste relevant logs]
```

**Screenshots:**
[attach if relevant]
```

---

## Additional Resources

- [Architecture Documentation](./ARCHITECTURE.md)
- [API Documentation](./API.md)
- [Developer Guide](./DEVELOPER_GUIDE.md)
- [yt-dlp Issues](https://github.com/yt-dlp/yt-dlp/issues)
- [Next.js Troubleshooting](https://nextjs.org/docs/messages)
