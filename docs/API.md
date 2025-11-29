# API Documentation

## Base URL

- **Development:** `https://localhost:8084`
- **Production:** `https://your-api-domain.com`

## Endpoints

### 1. Get Video Information

Retrieves metadata and download options for a video URL.

#### Request

```http
POST /api/Info
Content-Type: application/json

{
  "url": "https://www.tiktok.com/@username/video/123456789"
}
```

#### Request Parameters

| Field | Type   | Required | Description                    |
|-------|--------|----------|--------------------------------|
| url   | string | Yes      | Full URL of the video to fetch |

#### Response (200 OK)

```json
{
  "title": "Video Title",
  "thumbnailUrl": "https://...",
  "platform": "TikTok",
  "downloadOptions": [
    {
      "quality": "1080x1920",
      "url": "https://...",
      "type": "video"
    },
    {
      "quality": "720x1280",
      "url": "https://...",
      "type": "video"
    },
    {
      "quality": "576x1024",
      "url": "https://...",
      "type": "video"
    }
  ]
}
```

#### Response Schema

```typescript
interface VideoInfo {
  title: string;           // Video title
  thumbnailUrl: string;    // Thumbnail image URL
  platform: string;        // Platform name (TikTok, Facebook, etc.)
  downloadOptions: DownloadOption[];
}

interface DownloadOption {
  quality: string | null;  // Quality string (e.g., "1080x1920", "720p")
  url: string;             // Direct download URL
  type: string;            // Media type ("video" or "audio")
}
```

#### Error Responses

**400 Bad Request**
```json
{
  "message": "URL is required"
}
```

**500 Internal Server Error**
```json
{
  "message": "yt-dlp failed: [error details]"
}
```

#### Example Usage

**cURL**
```bash
curl -X POST https://localhost:8084/api/Info \
  -H "Content-Type: application/json" \
  -d '{"url":"https://www.tiktok.com/@user/video/123"}'
```

**JavaScript (Fetch)**
```javascript
const response = await fetch('https://localhost:8084/api/Info', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
  },
  body: JSON.stringify({
    url: 'https://www.tiktok.com/@user/video/123'
  })
});

const data = await response.json();
console.log(data);
```

---

### 2. Download Video

Downloads a video using yt-dlp with optional quality selection.

#### Request

```http
GET /api/Download?url={videoUrl}&quality={quality}
```

#### Query Parameters

| Parameter | Type   | Required | Description                                  |
|-----------|--------|----------|----------------------------------------------|
| url       | string | Yes      | Original video page URL (URL-encoded)        |
| quality   | string | No       | Quality preference (e.g., "1080p", "720p", "best") |

#### Response (200 OK)

- **Content-Type:** `video/mp4`
- **Content-Disposition:** `attachment; filename="video.mp4"`
- **Body:** Binary video stream

#### Error Responses

**400 Bad Request**
```json
{
  "message": "URL is required"
}
```

**500 Internal Server Error**
```json
{
  "message": "Failed to download video",
  "detail": "[yt-dlp error details]"
}
```

#### Example Usage

**Browser (Direct Download)**
```html
<a href="https://localhost:8084/api/Download?url=https%3A%2F%2Fwww.tiktok.com%2F%40user%2Fvideo%2F123&quality=1080p">
  Download Video
</a>
```

**JavaScript**
```javascript
const url = 'https://www.tiktok.com/@user/video/123';
const quality = '1080p';
const downloadUrl = `https://localhost:8084/api/Download?url=${encodeURIComponent(url)}&quality=${quality}`;

window.location.href = downloadUrl;
```

**cURL**
```bash
curl -o video.mp4 "https://localhost:8084/api/Download?url=https%3A%2F%2Fwww.tiktok.com%2F%40user%2Fvideo%2F123&quality=1080p"
```

---

## Quality Options

| Quality | Description                    | Use Case                |
|---------|--------------------------------|-------------------------|
| best    | Highest available quality      | Best quality (default)  |
| 1920p   | 1920p resolution               | Full HD                 |
| 1080p   | 1080p resolution               | HD                      |
| 720p    | 720p resolution                | Standard HD             |
| 480p    | 480p resolution                | Mobile/Low bandwidth    |

---

## Supported Platforms

Based on yt-dlp capabilities, the API supports:

- ✅ TikTok
- ✅ Facebook
- ✅ Instagram
- ✅ Twitter/X
- ✅ YouTube
- ✅ And 1000+ more sites

For a complete list, see [yt-dlp supported sites](https://github.com/yt-dlp/yt-dlp/blob/master/supportedsites.md).

---

## Rate Limiting

**Current:** No rate limiting implemented

**Recommended for Production:**
- 10 requests per minute per IP
- 100 requests per hour per IP

---

## CORS Policy

The API allows cross-origin requests from:
- Development: `http://localhost:3000`
- Production: Your configured frontend domain

**Headers Allowed:**
- `Content-Type`
- `Authorization` (future)

**Methods Allowed:**
- GET
- POST
- OPTIONS

---

## Error Codes

| Code | Meaning                        | Common Causes                          |
|------|--------------------------------|----------------------------------------|
| 200  | Success                        | Request processed successfully         |
| 400  | Bad Request                    | Missing/invalid parameters             |
| 403  | Forbidden                      | CORS policy violation                  |
| 500  | Internal Server Error          | yt-dlp failure, file system error      |
| 503  | Service Unavailable            | Server overload                        |

---

## Response Headers

### Common Headers

```http
Content-Type: application/json; charset=utf-8
Access-Control-Allow-Origin: https://your-frontend.vercel.app
Access-Control-Allow-Methods: GET, POST, OPTIONS
```

### Download Endpoint Headers

```http
Content-Type: video/mp4
Content-Disposition: attachment; filename="video.mp4"
Content-Length: 12345678
```

---

## Authentication

**Current:** No authentication required

**Future Plans:**
- API key authentication
- OAuth 2.0 for user accounts
- JWT tokens for session management

---

## Versioning

**Current Version:** v1 (implicit)

**Future Versioning Strategy:**
- URL-based: `/api/v2/Info`
- Header-based: `Accept: application/vnd.api+json;version=2`

---

## Best Practices

### 1. URL Encoding
Always URL-encode the video URL parameter:
```javascript
const encodedUrl = encodeURIComponent(videoUrl);
```

### 2. Error Handling
Implement proper error handling:
```javascript
try {
  const response = await fetch('/api/Info', {...});
  if (!response.ok) {
    const error = await response.json();
    console.error('API Error:', error.message);
  }
} catch (error) {
  console.error('Network Error:', error);
}
```

### 3. Timeouts
Set appropriate timeouts for download requests:
```javascript
const controller = new AbortController();
const timeoutId = setTimeout(() => controller.abort(), 60000); // 60s

fetch('/api/Download?...', { signal: controller.signal })
  .finally(() => clearTimeout(timeoutId));
```

---

## Testing

### Test URLs

**TikTok:**
```
https://www.tiktok.com/@tiktok/video/7016451564454763781
```

**Facebook:**
```
https://www.facebook.com/watch/?v=123456789
```

### Postman Collection

Import the following collection for testing:

```json
{
  "info": {
    "name": "Video Downloader API",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "Get Video Info",
      "request": {
        "method": "POST",
        "header": [
          {
            "key": "Content-Type",
            "value": "application/json"
          }
        ],
        "body": {
          "mode": "raw",
          "raw": "{\"url\":\"https://www.tiktok.com/@tiktok/video/123\"}"
        },
        "url": {
          "raw": "{{baseUrl}}/api/Info",
          "host": ["{{baseUrl}}"],
          "path": ["api", "Info"]
        }
      }
    }
  ]
}
```
