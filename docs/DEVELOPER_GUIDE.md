# Developer Guide

## Getting Started

This guide helps developers set up, understand, and contribute to the Video Downloader project.

## Prerequisites

### Required Software

- **Node.js** 18+ ([Download](https://nodejs.org/))
- **.NET SDK** 10+ ([Download](https://dotnet.microsoft.com/download))
- **Git** ([Download](https://git-scm.com/))
- **Code Editor** (VS Code, Rider, etc.)

### Optional Tools

- **Postman** - API testing
- **Docker** - Containerization
- **Azure CLI** - Deployment

---

## Setup Instructions

### 1. Clone the Repository

```bash
git clone https://github.com/CoderSaiya/video_fetch.git
cd video_fetch
```

### 2. Frontend Setup

```bash
cd client
npm install

# Create environment file
cp .env.example .env.local

# Edit .env.local with your configuration
# NEXT_PUBLIC_API_URL=https://localhost:8084

# Start development server
npm run dev
```

Frontend runs at: `http://localhost:3000`

### 3. Backend Setup

```bash
cd server

# Download yt-dlp
curl -L https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe -o yt-dlp.exe

# Restore dependencies
dotnet restore

# Run the API
dotnet run
```

Backend runs at: `https://localhost:8084`

### 4. Verify Setup

1. Open `http://localhost:3000`
2. Paste a TikTok URL
3. Click "Fetch Info"
4. Verify video information displays
5. Try downloading

---

## Project Structure

```
video_fetch/
├── client/                    # Next.js frontend
│   ├── src/
│   │   └── app/
│   │       ├── components/    # React components
│   │       ├── page.tsx       # Main page
│   │       └── types.ts       # TypeScript types
│   ├── public/               # Static assets
│   ├── .env.local            # Local environment (git-ignored)
│   ├── .env.example          # Environment template
│   └── package.json          # Dependencies
│
├── server/                   # .NET backend
│   ├── Controllers/          # API endpoints
│   ├── Services/             # Business logic
│   ├── Models/               # Data models
│   ├── Program.cs            # App configuration
│   └── yt-dlp.exe           # Video extraction tool
│
├── docs/                     # Documentation
│   ├── ARCHITECTURE.md
│   ├── API.md
│   ├── FRONTEND.md
│   ├── BACKEND.md
│   ├── USER_GUIDE.md
│   └── DEVELOPER_GUIDE.md
│
└── README.md                # Project overview
```

---

## Development Workflow

### Branch Strategy

```
main           # Production-ready code
├── develop    # Development branch
├── feature/*  # New features
├── bugfix/*   # Bug fixes
└── hotfix/*   # Urgent production fixes
```

### Workflow Example

```bash
# Create feature branch
git checkout -b feature/add-youtube-support

# Make changes
# ... code ...

# Commit
git add .
git commit -m "feat: add YouTube download support"

# Push
git push origin feature/add-youtube-support

# Create Pull Request on GitHub
```

### Commit Message Convention

```
feat: Add new feature
fix: Fix bug
docs: Update documentation
style: Code formatting
refactor: Code restructuring
test: Add tests
chore: Build/config changes
```

---

## Adding a New Feature

### Example: Add Download History

#### 1. Frontend Changes

**Create type** (`types.ts`):
```typescript
export interface DownloadHistory {
  id: string;
  videoUrl: string;
  title: string;
  downloadedAt: Date;
}
```

**Create component** (`HistoryList.tsx`):
```tsx
export default function HistoryList() {
  const [history, setHistory] = useState<DownloadHistory[]>([]);
  
  // Load from localStorage
  useEffect(() => {
    const saved = localStorage.getItem('downloadHistory');
    if (saved) {
      setHistory(JSON.parse(saved));
    }
  }, []);
  
  return (
    <div>
      {history.map(item => (
        <div key={item.id}>
          <h4>{item.title}</h4>
          <span>{item.downloadedAt}</span>
        </div>
      ))}
    </div>
  );
}
```

**Update main page** (`page.tsx`):
```typescript
// Save to history after download
const saveToHistory = (videoInfo: VideoInfo) => {
  const history = JSON.parse(localStorage.getItem('downloadHistory') || '[]');
  history.push({
    id: Date.now().toString(),
    videoUrl: originalUrl,
    title: videoInfo.title,
    downloadedAt: new Date()
  });
  localStorage.setItem('downloadHistory', JSON.stringify(history));
};
```

#### 2. Test Changes

```bash
npm run dev
# Test in browser
```

#### 3. Documentation

Update `USER_GUIDE.md`:
```markdown
## Download History

View your recent downloads in the History tab.
```

#### 4. Commit

```bash
git add .
git commit -m "feat: add download history feature"
```

---

## Testing

### Frontend Testing

#### Manual Testing Checklist

- [ ] URL input validation
- [ ] Fetch video info
- [ ] Display video metadata
- [ ] Download different qualities
- [ ] Copy link functionality
- [ ] Error handling
- [ ] Mobile responsive
- [ ] Cross-browser compatibility

#### Future: Automated Tests

```bash
# Install testing library
npm install --save-dev @testing-library/react @testing-library/jest-dom

# Create test file
# DownloadOptions.test.tsx
```

### Backend Testing

#### Manual Testing with Swagger

1. Navigate to `https://localhost:8084/swagger`
2. Test `/api/Info` endpoint
3. Test `/api/Download` endpoint

#### Future: Unit Tests

```csharp
// YtDlpServiceTests.cs
[Fact]
public async Task GetVideoInfoAsync_ValidUrl_ReturnsVideoInfo()
{
    // Arrange
    var service = new YtDlpService();
    var url = "https://www.tiktok.com/@tiktok/video/123";
    
    // Act
    var result = await service.GetVideoInfoAsync(url);
    
    // Assert
    Assert.NotNull(result);
    Assert.NotEmpty(result.Title);
}
```

---

## Debugging

### Frontend Debugging

**VS Code Launch Configuration** (`.vscode/launch.json`):
```json
{
  "type": "chrome",
  "request": "launch",
  "name": "Next.js: debug",
  "url": "http://localhost:3000",
  "webRoot": "${workspaceFolder}/client"
}
```

**Browser DevTools:**
- F12 to open
- Check Console for errors
- Network tab for API calls
- React DevTools extension

### Backend Debugging

**VS Code / Rider:**
- Set breakpoints in code
- Press F5 to start debugging
- Step through code execution

**Logs:**
```csharp
_logger.LogInformation("Debug info: {Data}", data);
```

View in console or Application Insights.

---

## Performance Optimization

### Frontend

1. **Code Splitting**
```tsx
const HeavyComponent = dynamic(() => import('./HeavyComponent'), {
  loading: () => <p>Loading...</p>
});
```

2. **Image Optimization**
- Use Next.js Image component
- Lazy load thumbnails

3. **API Caching**
```typescript
// Future: Use SWR
import useSWR from 'swr';

const { data, error } = useSWR(`/api/Info/${url}`, fetcher);
```

### Backend

1. **Async Operations**
- All I/O operations are async
- Use `await` properly

2. **Memory Management**
```csharp
using var memoryStream = new MemoryStream();
// Auto-disposed after use
```

3. **Response Caching** (Future)
```csharp
[ResponseCache(Duration = 3600)]
public async Task<IActionResult> GetVideoInfo(...)
```

---

## Deployment

### Frontend (Vercel)

```bash
# Install Vercel CLI
npm i -g vercel

# Deploy
cd client
vercel --prod
```

**Environment Variables:**
- Add `NEXT_PUBLIC_API_URL` in Vercel dashboard

### Backend (Azure)

```bash
# Login
az login

# Create app
az webapp create \
  --resource-group VideoDownloader \
  --name video-api \
  --runtime "DOTNET|10.0"

# Deploy
cd server
dotnet publish -c Release
zip -r publish.zip publish/
az webapp deploy --src-path publish.zip
```

---

## Common Issues

### Issue: yt-dlp not found

**Solution:**
```bash
# Download to server directory
cd server
curl -L https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe -o yt-dlp.exe
```

### Issue: CORS errors

**Solution:** Update `Program.cs`:
```csharp
builder.WithOrigins("http://localhost:3000", "https://your-app.vercel.app")
```

### Issue: Port already in use

**Solution:**
```bash
# Frontend
PORT=3001 npm run dev

# Backend (change in launchSettings.json)
```

---

## Code Style Guidelines

### TypeScript/React

- Use functional components
- Use TypeScript interfaces
- Descriptive variable names
- Comments for complex logic

```tsx
// Good
const handleDownload = async (quality: string) => {
  // Implementation
};

// Avoid
const hd = async (q: string) => {
  // Implementation
};
```

### C#

- Follow Microsoft conventions
- Use async/await
- Dependency injection
- XML documentation

```csharp
/// <summary>
/// Downloads a video with specified quality
/// </summary>
/// <param name="url">Video URL</param>
/// <param name="quality">Quality preference</param>
public async Task<IActionResult> Download(string url, string quality)
{
    // Implementation
}
```

---

## Contributing

### Pull Request Process

1. Fork the repository
2. Create feature branch
3. Make changes
4. Write/update tests
5. Update documentation
6. Submit PR with description

### PR Template

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Testing
How has this been tested?

## Checklist
- [ ] Code follows style guidelines
- [ ] Documentation updated
- [ ] No new warnings
- [ ] Tests added/updated
```

---

## Resources

### Documentation
- [Architecture](./ARCHITECTURE.md)
- [API Reference](./API.md)
- [Frontend Guide](./FRONTEND.md)
- [Backend Guide](./BACKEND.md)

### External Resources
- [Next.js Docs](https://nextjs.org/docs)
- [.NET Docs](https://docs.microsoft.com/dotnet)
- [yt-dlp](https://github.com/yt-dlp/yt-dlp)

### Community
- GitHub Issues
- Discussions tab
- Discord (if available)

---

## License

MIT License - see LICENSE file for details
