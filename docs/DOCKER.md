# Docker Deployment Guide - Backend Server

## Overview

Deploy the Video Downloader backend API using Docker. Frontend is deployed separately on Vercel.

**Deployment Options:**
- **Local Development:** Run backend on localhost
- **Cloud Platforms:** Railway, Google Cloud Run, Azure, etc.
- **Cloudflare Tunnel:** Expose local backend securely without public IP (FREE!)

## Prerequisites

- Docker Engine 20.10+
- 1GB+ available RAM
- 5GB+ available disk space
- (Optional) Cloudflare account for tunnel deployment

## Quick Start

```bash
# Navigate to project directory
cd video_fetch

# Build and start backend
docker-compose -f docker/compose.yml up -d

# View logs
docker-compose -f docker/compose.yml logs -f

# Access API at http://localhost:8080
# Swagger UI at http://localhost:8080/swagger
```

---

## Architecture

### Standard Deployment
```
Vercel (Frontend)  →  Docker Container (Backend API)
     ↓                        ↓
  Next.js              .NET + yt-dlp
```

### With Cloudflare Tunnel (Recommended)
```
Vercel Frontend (HTTPS)
        ↓
Cloudflare Network (CDN + SSL + DDoS Protection)
        ↓
Cloudflare Tunnel (cloudflared container)
        ↓
Backend Container (HTTP :8080)
```

**Benefits of Cloudflare Tunnel:**
- ✅ No public IP needed
- ✅ No port forwarding
- ✅ Automatic HTTPS
- ✅ DDoS protection
- ✅ Completely FREE

📖 **Setup Guide:** [CLOUDFLARE_TUNNEL.md](./CLOUDFLARE_TUNNEL.md)

---

## Backend Docker Image

**Dockerfile:** `server/Dockerfile`

**Features:**
- Multi-stage build
- yt-dlp pre-installed
- FFmpeg for video processing
- Health checks enabled
- Production optimized

**Size:** ~500MB

---

## Configuration

### CORS Setup

Update `server/Program.cs` to allow Vercel frontend:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.WithOrigins(
                "http://localhost:3000",           // Local development
                "https://your-app.vercel.app"      // Production
            )
            .AllowAnyMethod()
            .AllowAnyHeader();
        });
});
```

### Environment Variables

Set in `docker/compose.yml`:

```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Production
  - ASPNETCORE_URLS=http://+:8080
```

---

## Build and Run

### Using Docker Compose

```bash
# Build and start
docker-compose -f docker/compose.yml up -d

# Rebuild
docker-compose -f docker/compose.yml build --no-cache

# Restart
docker-compose -f docker/compose.yml restart

# Stop
docker-compose -f docker/compose.yml down
```

### Manual Docker Build

```bash
cd server

# Build image
docker build -t video-downloader-api .

# Run container
docker run -d \
  -p 8080:8080 \
  --name video-api \
  video-downloader-api
```

---

## Deployment to Production

### Option 1: Cloudflare Tunnel (Recommended for Easy Setup)

**Best for:** Quick deployment without server configuration, free hosting, development/testing

```bash
# 1. Create .env file
cd docker
cp .env.example .env

# 2. Add your Cloudflare tunnel token to .env
# Get token from: https://one.dash.cloudflare.com/

# 3. Start services
docker-compose -f compose.yml up -d

# 4. Configure public hostname in Cloudflare Dashboard
# Point to: backend:8080
```

**Your API is now live at:** `https://api.yourdomain.com`

📖 **Full Guide:** [CLOUDFLARE_TUNNEL.md](./CLOUDFLARE_TUNNEL.md)

---

### Option 2: Docker Hub

```bash
# Build
docker-compose -f docker/compose.yml build

# Tag
docker tag video-downloader-api codersaiya/video-downloader-api:latest

# Push
docker push codersaiya/video-downloader-api:latest
```

**On production server:**
```bash
docker pull codersaiya/video-downloader-api:latest
docker run -d -p 8080:8080 codersaiya/video-downloader-api:latest
```

### Option 3: Cloud Platforms

#### Railway
1. Connect GitHub repository
2. Select `server` as root directory
3. Auto-deploy from Dockerfile

#### Google Cloud Run
```bash
# Deploy directly
gcloud run deploy video-api \
  --source ./server \
  --platform managed \
  --region asia-southeast1
```

#### Azure Container Instances
```bash
az container create \
  --resource-group VideoDownloader \
  --name video-api \
  --image codersaiya/video-downloader-api:latest \
  --dns-name-label video-api \
  --ports 8080
```

---

## Connect with Vercel Frontend

### 1. Deploy Backend to Production

Get your backend URL (e.g., `https://api.example.com`)

### 2. Update Vercel Environment Variable

In Vercel dashboard:
- **Variable:** `NEXT_PUBLIC_API_URL`
- **Value:** `https://api.example.com` (your Docker backend URL)

### 3. Update Backend CORS

```csharp
builder.WithOrigins("https://your-app.vercel.app")
```

### 4. Redeploy Both Services

```bash
# Backend
docker-compose -f docker/compose.yml up -d --build

# Frontend (automatic on Vercel after env change)
```

---

## Monitoring & Logs

### View Logs

```bash
# Real-time logs
docker-compose -f docker/compose.yml logs -f

# Last 100 lines
docker-compose -f docker/compose.yml logs --tail=100

# Specific time
docker-compose -f docker/compose.yml logs --since 30m
```

### Container Health

```bash
# Check status
docker ps

# Health check
curl http://localhost:8080/swagger

# Container stats
docker stats video-downloader-api
```

---

## Troubleshooting

### Container Won't Start

```bash
# Check logs
docker-compose -f docker/compose.yml logs backend

# Verify image
docker images | grep video-downloader

# Rebuild
docker-compose -f docker/compose.yml build --no-cache
```

### Port Already in Use

```bash
# Change port in docker/compose.yml
ports:
  - "8081:8080"
```

### yt-dlp Not Working

```bash
# Enter container
docker-compose -f docker/compose.yml exec backend /bin/bash

# Test yt-dlp
yt-dlp --version

# Update yt-dlp
yt-dlp -U
```

### CORS Errors

Ensure backend CORS includes Vercel domain:
```csharp
builder.WithOrigins("https://your-app.vercel.app")
```

---

## Maintenance

### Update Backend

```bash
# Pull latest code
git pull origin main

# Rebuild and restart
docker-compose -f docker/compose.yml up -d --build
```

### Clean Up

```bash
# Stop container
docker-compose -f docker/compose.yml down

# Remove images
docker rmi video-downloader-api

# Clean system
docker system prune -a
```

---

## Performance Tips

1. **Resource Limits**
```yaml
deploy:
  resources:
    limits:
      cpus: '1.0'
      memory: 1G
```

2. **Volume for Temp Files**
```yaml
volumes:
  - /tmp/video-downloads:/tmp/video-downloads
```

3. **Enable Logging**
```yaml
logging:
  driver: "json-file"
  options:
    max-size: "10m"
    max-file: "3"
```

---

## Security

1. **Use HTTPS in Production**
   - Place behind reverse proxy (nginx, Caddy)
   - Or use cloud platform HTTPS

2. **Regular Updates**
```bash
# Update base image
docker pull mcr.microsoft.com/dotnet/aspnet:8.0
docker-compose -f docker/compose.yml build --no-cache
```

3. **Scan for Vulnerabilities**
```bash
docker scan video-downloader-api
```

---

## Complete Deployment Workflow

```bash
# 1. Build backend Docker image
docker-compose -f docker/compose.yml build

# 2. Push to Docker Hub
docker tag video-downloader-api codersaiya/video-downloader-api:latest
docker push codersaiya/video-downloader-api:latest

# 3. Deploy on production server
# (Railway, Google Cloud Run, Azure, etc.)

# 4. Get backend URL (e.g., https://api.example.com)

# 5. Update Vercel env variable
# NEXT_PUBLIC_API_URL=https://api.example.com

# 6. Update backend CORS with Vercel URL

# 7. Test the integration!
```

---

## Support

For issues:
- Check [TROUBLESHOOTING.md](./TROUBLESHOOTING.md)
- [GitHub Issues](https://github.com/CoderSaiya/video_fetch/issues)
