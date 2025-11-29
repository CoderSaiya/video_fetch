# Docker Quick Start - Backend Only

Frontend is deployed on Vercel. This Docker config is for **backend server only**.

## Build and Run

```bash
# Build and start backend
docker-compose up -d

# View logs
docker-compose logs -f

# Stop
docker-compose down
```

## Access

- **Backend API:** http://localhost:8080
- **Swagger:** http://localhost:8080/swagger

## Push to Docker Hub

```bash
# Build
docker-compose build

# Tag and push
docker tag video-downloader-api codersaiya/video-downloader-api:latest
docker push codersaiya/video-downloader-api:latest
```

## Connect with Vercel Frontend

1. Deploy backend to production (Railway, Cloud Run, etc.)
2. Get backend URL (e.g., `https://api.example.com`)
3. Update Vercel environment variable: `NEXT_PUBLIC_API_URL=https://api.example.com`
4. Update backend CORS to allow Vercel domain

See [docs/DOCKER.md](./docs/DOCKER.md) for detailed instructions.
