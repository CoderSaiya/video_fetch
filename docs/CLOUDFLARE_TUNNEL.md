# Cloudflare Tunnel Setup Guide

## Overview

Cloudflare Tunnel provides secure, encrypted tunnels between your backend server and Cloudflare's network without exposing public IP addresses or opening inbound ports. This allows you to:

- ✅ Expose your backend API to the internet securely
- ✅ No need for public IP or port forwarding
- ✅ Automatic HTTPS with Cloudflare SSL
- ✅ DDoS protection and CDN benefits
- ✅ Free for personal use

---

## Prerequisites

- Cloudflare account (free tier works)
- Domain name added to Cloudflare
- Docker and Docker Compose installed

---

## Step-by-Step Setup

### 1. Create Cloudflare Tunnel

1. **Login to Cloudflare Dashboard**
   - Go to [Cloudflare Zero Trust](https://one.dash.cloudflare.com/)

2. **Navigate to Tunnels**
   - Click `Networks` → `Tunnels`
   - Click `Create a tunnel`

3. **Name Your Tunnel**
   - Enter a name (e.g., `video-downloader-backend`)
   - Click `Save tunnel`

4. **Copy Tunnel Token**
   - After creation, you'll see a token that starts with `eyJ...`
   - **IMPORTANT:** Copy this token, you'll need it later
   - Keep it secure - treat it like a password

### 2. Configure Environment Variable

Create `.env` file in the `docker` directory:

```bash
cd docker
cp .env.example .env
```

Edit `.env` and paste your tunnel token:

```env
CLOUDFLARE_TUNNEL_TOKEN=eyJhIjoiYjc4ZGE5OGY3YmQzNDU2N2E4NDM5ZmRjMjNlZjc5YzEiLCJ0IjoiNTI...
```

### 3. Configure Public Hostname

Back in Cloudflare Dashboard:

1. **Add Public Hostname**
   - In the tunnel configuration, go to `Public Hostname` tab
   - Click `Add a public hostname`

2. **Configure Subdomain**
   - **Subdomain:** `api` (or any name you prefer)
   - **Domain:** Select your domain from dropdown
   - **Path:** Leave empty
   - **Type:** `HTTP`
   - **URL:** `backend:8080` (or `http://backend:8080`)

3. **Save Hostname**
   - Click `Save hostname`
   - Your API will be accessible at: `https://api.yourdomain.com`

### 4. Start Docker Services

```bash
# Navigate to docker directory
cd docker

# Start all services (backend + cloudflared)
docker-compose -f compose.yml up -d

# Check logs
docker-compose -f compose.yml logs -f cloudflared
```

### 5. Verify Connection

```bash
# Check tunnel status
docker-compose -f compose.yml logs cloudflared

# You should see:
# "Registered tunnel connection"
# "Started"
```

Test your public URL:
```bash
curl https://api.yourdomain.com/swagger
```

---

## Architecture

```
Internet Users
      ↓
Cloudflare Network (HTTPS)
      ↓
Cloudflare Tunnel (cloudflared)
      ↓
Backend Container (HTTP :8080)
```

**Benefits:**
- All traffic is encrypted via Cloudflare
- No open ports on your server
- Automatic SSL/TLS certificates
- Built-in DDoS protection

---

## Configuration Options

### Using Config File (Alternative Method)

Instead of using token, you can use a config file:

**docker/cloudflare/config.yml:**
```yaml
tunnel: YOUR_TUNNEL_ID
credentials-file: /etc/cloudflared/cert.json

ingress:
  - hostname: api.yourdomain.com
    service: http://backend:8080
  - service: http_status:404
```

**Update compose.yml:**
```yaml
cloudflared:
  image: cloudflare/cloudflared:latest
  command: tunnel --config /etc/cloudflared/config.yml run
  volumes:
    - ./cloudflare/config.yml:/etc/cloudflared/config.yml
    - ./cloudflare/cert.json:/etc/cloudflared/cert.json
```

### Custom Tunnel Metrics Port

Monitor tunnel health:
```yaml
environment:
  - TUNNEL_METRICS=0.0.0.0:3000
ports:
  - "3000:3000"  # Expose metrics
```

Visit `http://localhost:3000/metrics` for Prometheus metrics.

---

## Connect Frontend with Tunneled Backend

### Update Vercel Environment Variables

1. Go to Vercel Dashboard
2. Select your frontend project
3. Go to `Settings` → `Environment Variables`
4. Update `NEXT_PUBLIC_API_URL`:
   ```
   NEXT_PUBLIC_API_URL=https://api.yourdomain.com
   ```
5. Redeploy your frontend

### Update Backend CORS

Edit `server/Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.WithOrigins(
                "http://localhost:3000",              // Local dev
                "https://your-app.vercel.app",        // Vercel frontend
                "https://api.yourdomain.com"          // Tunnel URL (for testing)
            )
            .AllowAnyMethod()
            .AllowAnyHeader();
        });
});
```

Rebuild and restart:
```bash
docker-compose -f docker/compose.yml up -d --build
```

---

## Troubleshooting

### Tunnel Not Connecting

**Check logs:**
```bash
docker-compose -f docker/compose.yml logs cloudflared
```

**Common issues:**
- **Invalid token:** Double-check your `CLOUDFLARE_TUNNEL_TOKEN` in `.env`
- **Token expired:** Regenerate token in Cloudflare Dashboard
- **Network issues:** Ensure Docker container can reach internet

**Solution:**
```bash
# Restart tunnel
docker-compose -f docker/compose.yml restart cloudflared
```

### "Bad Gateway" or 502 Error

**Cause:** Backend container is not accessible from cloudflared

**Check:**
```bash
# Enter cloudflared container
docker exec -it video-downloader-tunnel sh

# Test backend connection
wget -O- http://backend:8080/swagger
```

**Solution:**
- Ensure both services are on the same Docker network
- Check backend is healthy: `docker ps`
- Verify `backend:8080` is correct in tunnel config

### Backend Can't Reach Tunnel

**Check network:**
```bash
docker network inspect video-downloader-network
```

Both `backend` and `cloudflared` should be listed.

### Logs Show "Unauthorized"

**Cause:** Invalid or expired tunnel token

**Solution:**
1. Go to Cloudflare Dashboard
2. Regenerate tunnel token
3. Update `.env` file
4. Restart: `docker-compose -f docker/compose.yml restart cloudflared`

---

## Security Best Practices

### 1. Protect Your .env File

Add to `.gitignore`:
```
docker/.env
```

Never commit your tunnel token to Git!

### 2. Use Cloudflare Access (Optional)

Add authentication layer:
1. Go to Cloudflare Zero Trust Dashboard
2. Navigate to `Access` → `Applications`
3. Create application for your API subdomain
4. Configure authentication (Google, GitHub, email OTP, etc.)

### 3. Rate Limiting

Configure in Cloudflare Dashboard:
- Go to your domain → Security → WAF
- Create rate limiting rule for API endpoints

### 4. IP Restrictions

Limit access to specific IPs:
- Cloudflare Dashboard → Security → WAF
- Create firewall rule to allow only specific IPs

---

## Monitoring

### Check Tunnel Status

```bash
# View real-time logs
docker-compose -f docker/compose.yml logs -f cloudflared

# Check health
docker-compose -f docker/compose.yml ps
```

Healthy tunnel shows:
```
NAME                     STATUS
video-downloader-tunnel  Up (healthy)
```

### Cloudflare Analytics

View tunnel metrics:
1. Cloudflare Dashboard → Zero Trust → Networks → Tunnels
2. Click on your tunnel name
3. View traffic analytics

---

## Running Without Tunnel (Optional)

If you want to disable Cloudflare Tunnel:

```bash
# Stop only tunnel service
docker-compose -f docker/compose.yml stop cloudflared

# Start only backend
docker-compose -f docker/compose.yml up -d backend
```

Or comment out the `cloudflared` service in `compose.yml`.

---

## Cost

Cloudflare Tunnel is **completely FREE** for:
- Unlimited tunnels
- Unlimited traffic
- Unlimited bandwidth

Perfect for development, staging, and production use!

---

## Additional Resources

- [Cloudflare Tunnel Documentation](https://developers.cloudflare.com/cloudflare-one/connections/connect-apps/)
- [Cloudflare Zero Trust Dashboard](https://one.dash.cloudflare.com/)
- [Tunnel Configuration Options](https://developers.cloudflare.com/cloudflare-one/connections/connect-apps/configuration/)

---

## Quick Reference Commands

```bash
# Start everything
docker-compose -f docker/compose.yml up -d

# View tunnel logs
docker-compose -f docker/compose.yml logs -f cloudflared

# Restart tunnel
docker-compose -f docker/compose.yml restart cloudflared

# Stop tunnel only
docker-compose -f docker/compose.yml stop cloudflared

# Remove everything
docker-compose -f docker/compose.yml down
```
