# Video Downloader

A web application to download videos from TikTok, Facebook, and other social media platforms.

## Features

- 🎥 Download videos from multiple platforms (TikTok, Facebook, Shopee Video)
- 📱 Responsive mobile-friendly design
- 🎬 Multiple quality options
- 📋 Copy download links to clipboard
- ⚡ Fast downloads using yt-dlp

## Tech Stack

**Frontend:**
- Next.js 15
- TypeScript
- CSS Modules

**Backend:**
- .NET 10 Web API
- yt-dlp for video extraction

## Setup

### Prerequisites

- Node.js 18+ 
- .NET 10 SDK
- yt-dlp.exe (place in server directory)

### Installation

1. **Clone the repository:**
   ```bash
   git clone https://github.com/CoderSaiya/video_fetch.git
   cd video_fetch
   ```

2. **Setup Frontend:**
   ```bash
   cd client
   npm install
   cp .env.example .env.local
   # Edit .env.local with your backend URL
   npm run dev
   ```

3. **Setup Backend:**
   ```bash
   cd server
   # Make sure yt-dlp.exe is in this directory
   dotnet restore
   dotnet run
   ```

### Environment Variables

Create `.env.local` in the `client` directory:

```env
NEXT_PUBLIC_API_URL=https://localhost:8084
```

For production deployment, see [`deployment_guide.md`](./docs/deployment_guide.md)

## Usage

1. Open http://localhost:3000
2. Paste a video URL from TikTok, Facebook, or other supported platform
3. Click "Fetch Info"
4. Select quality and download

## Deployment

See the [Deployment Guide](./docs/deployment_guide.md) for instructions on deploying to Vercel and hosting the backend.

## License

MIT
