# Docker Quick Start - Backend Only

Frontend is deployed on Vercel. This Docker config is for **backend server only**.

## Build and Run

```bash
# Build and start backend
docker-compose -f docker/compose.yml up -d

# View logs
docker-compose -f docker/compose.yml logs -f

# Stop
docker-compose -f docker/compose.yml down
```

## Access

- **Backend API:** http://localhost:8080
- **Swagger:** http://localhost:8080/swagger

## Cloudflare Tunnel (Optional)

Expose your backend to the internet securely without opening ports:

### Quick Setup

1. **Get Tunnel Token**
   - Visit [Cloudflare Zero Trust](https://one.dash.cloudflare.com/)
   - Create a tunnel and copy the token

2. **Configure Environment**
   ```bash
   cd docker
   cp .env.example .env
   # Edit .env and paste your token
   ```

3. **Start Services**
   ```bash
   docker-compose -f docker/compose.yml up -d
   ```

4. **Configure Public Hostname**
   - In Cloudflare Dashboard, add public hostname
   - Point to: `backend:8080`
   - Your API will be at: `https://api.yourdomain.com`

📖 **Full Guide:** See [docs/CLOUDFLARE_TUNNEL.md](./docs/CLOUDFLARE_TUNNEL.md) for detailed setup.

## Push to Docker Hub

```bash
# Build
docker-compose -f docker/compose.yml build

# Tag and push
docker tag video-downloader-api codersaiya/video-downloader-api:latest
docker push codersaiya/video-downloader-api:latest
```

## Connect with Vercel Frontend

1. Deploy backend to production (Railway, Cloud Run, etc.) **OR** use Cloudflare Tunnel
2. Get backend URL (e.g., `https://api.example.com`)
3. Update Vercel environment variable: `NEXT_PUBLIC_API_URL=https://api.example.com`
4. Update backend CORS to allow Vercel domain

See [docs/DOCKER.md](./docs/DOCKER.md) for detailed instructions.
