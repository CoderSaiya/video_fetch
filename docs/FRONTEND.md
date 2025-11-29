# Frontend Documentation

## Overview

The frontend is built with Next.js 15 and TypeScript, providing a responsive interface for downloading videos from social media platforms.

## Project Structure

```
client/
├── src/
│   ├── app/
│   │   ├── components/          # React components
│   │   │   ├── UrlInput.tsx
│   │   │   ├── DownloadOptions.tsx
│   │   │   └── StatusFeedback.tsx
│   │   ├── page.tsx            # Main page
│   │   ├── layout.tsx          # Root layout
│   │   ├── globals.css         # Global styles
│   │   └── types.ts            # TypeScript types
├── public/                     # Static assets
├── .env.local                  # Local environment variables
├── .env.example               # Environment template
├── next.config.js             # Next.js configuration
├── package.json               # Dependencies
└── tsconfig.json              # TypeScript config
```

## Components

### 1. UrlInput Component

**File:** `src/app/components/UrlInput.tsx`

**Purpose:** Input field for video URLs with validation and submission.

**Props:**
```typescript
interface UrlInputProps {
  onUrlSubmit: (url: string) => Promise<void>;
  isLoading: boolean;
}
```

**Features:**
- URL validation
- Loading state handling
- Disabled state during fetch
- Responsive design

**Usage:**
```tsx
<UrlInput 
  onUrlSubmit={handleUrlSubmit} 
  isLoading={isLoading} 
/>
```

---

### 2. DownloadOptions Component

**File:** `src/app/components/DownloadOptions.tsx`

**Purpose:** Displays video information and download options.

**Props:**
```typescript
interface DownloadOptionsProps {
  data: VideoInfo;
  originalUrl: string;
}
```

**Features:**
- Video thumbnail display
- Multiple quality options
- Download button for each quality
- Copy link to clipboard
- Platform badge

**Usage:**
```tsx
<DownloadOptions 
  data={videoInfo} 
  originalUrl={originalUrl} 
/>
```

---

### 3. StatusFeedback Component

**File:** `src/app/components/StatusFeedback.tsx`

**Purpose:** Shows loading spinners and error messages.

**Props:**
```typescript
interface StatusFeedbackProps {
  isLoading: boolean;
  error: string | null;
}
```

**Features:**
- Loading spinner animation
- Error message display
- Conditional rendering

**Usage:**
```tsx
<StatusFeedback 
  isLoading={isLoading} 
  error={error} 
/>
```

---

## Pages

### Main Page (`page.tsx`)

**Route:** `/`

**Features:**
- URL input form
- Video information display
- Download options
- Error handling
- Loading states

**State Management:**
```typescript
const [isLoading, setIsLoading] = useState(false);
const [error, setError] = useState<string | null>(null);
const [videoInfo, setVideoInfo] = useState<VideoInfo | null>(null);
const [originalUrl, setOriginalUrl] = useState<string>("");
```

**API Integration:**
```typescript
const handleUrlSubmit = async (url: string) => {
  const apiUrl = process.env.NEXT_PUBLIC_API_URL || 'https://localhost:8084';
  const response = await fetch(`${apiUrl}/api/Info`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ url }),
  });
  // ... handle response
};
```

---

## Styling

### CSS Modules

Each component has its own CSS module for scoped styling:

**Example: `DownloadOptions.module.css`**
```css
.container {
  background: white;
  border-radius: 8px;
  padding: 20px;
}

.downloadBtn {
  background: var(--primary-color);
  color: white;
  padding: 12px 24px;
  border-radius: 4px;
}
```

### Global Styles

**File:** `globals.css`

**Features:**
- CSS variables for theming
- Reset styles
- Typography
- Layout utilities

**CSS Variables:**
```css
:root {
  --primary-color: #00d4ff;
  --text-color: #333;
  --background: #f5f5f5;
  --border-color: #ddd;
}
```

---

## TypeScript Types

**File:** `types.ts`

```typescript
export interface DownloadOption {
  quality: string | null;
  url: string;
  type: string;
}

export interface VideoInfo {
  title: string;
  thumbnailUrl: string;
  platform: string;
  downloadOptions: DownloadOption[];
}
```

---

## Environment Variables

### Required Variables

**File:** `.env.local`

```env
NEXT_PUBLIC_API_URL=https://localhost:8084
```

> **Note:** Variables prefixed with `NEXT_PUBLIC_` are exposed to the browser.

---

## Data Flow

```
User Input → UrlInput.onSubmit() → page.handleUrlSubmit() →
Fetch API → Backend → Response → setVideoInfo() →
DownloadOptions.render() → Display Results
```

---

## Error Handling

### Network Errors
```typescript
try {
  const response = await fetch(...);
  if (!response.ok) {
    const errorData = await response.json().catch(() => ({}));
    throw new Error(errorData.message || "Failed to fetch");
  }
} catch (err: any) {
  setError(err.message || "An unexpected error occurred");
}
```

### User Feedback
- Loading spinner during API calls
- Error messages displayed prominently
- Success states with visual feedback

---

## Responsive Design

### Breakpoints
```css
/* Mobile First */
.container {
  /* Mobile styles */
}

@media (min-width: 768px) {
  /* Tablet styles */
}

@media (min-width: 1024px) {
  /* Desktop styles */
}
```

### Mobile Optimizations
- Touch-friendly buttons (min 44px)
- Responsive images
- Flexible layouts
- Mobile-friendly modals

---

## Performance Optimization

### Image Optimization
- Next.js automatic image optimization
- Lazy loading for thumbnails
- Proper alt tags for accessibility

### Code Splitting
- Automatic route-based code splitting
- Dynamic imports for heavy components

### Caching
- SWR for data fetching (future enhancement)
- Static asset caching via Vercel CDN

---

## Development

### Running Locally

```bash
cd client
npm install
npm run dev
```

### Building for Production

```bash
npm run build
npm start
```

### Linting

```bash
npm run lint
```

---

## Testing

### Manual Testing Checklist

- [ ] Enter valid TikTok URL
- [ ] Enter invalid URL
- [ ] Test download on mobile
- [ ] Test copy link functionality
- [ ] Test error states
- [ ] Test loading states
- [ ] Test responsive design

### Future: Automated Tests

```typescript
// Example test structure
describe('DownloadOptions', () => {
  it('should render download buttons', () => {
    // Test implementation
  });
  
  it('should handle download click', () => {
    // Test implementation
  });
});
```

---

## Accessibility

### ARIA Labels
```tsx
<button aria-label="Download video in 1080p">
  Download 1080p
</button>
```

### Keyboard Navigation
- All interactive elements accessible via keyboard
- Tab order logical and intuitive
- Focus indicators visible

### Screen Reader Support
- Proper heading hierarchy
- Alternative text for images
- Loading state announcements

---

## Browser Support

- Chrome/Edge: Latest 2 versions
- Firefox: Latest 2 versions
- Safari: Latest 2 versions
- Mobile Safari: iOS 14+
- Chrome Mobile: Latest

---

## Common Issues & Solutions

### Issue: Environment variable not working
**Solution:** Ensure it starts with `NEXT_PUBLIC_` and restart dev server.

### Issue: CORS errors
**Solution:** Check backend CORS configuration matches frontend origin.

### Issue: Download not starting
**Solution:** Verify backend is running and API URL is correct.

---

## Future Enhancements

1. **Loading Progress**
   - Show download percentage
   - Estimated time remaining

2. **Batch Downloads**
   - Multiple video queue
   - Progress tracking

3. **History**
   - Save downloaded videos
   - Local storage persistence

4. **Themes**
   - Dark mode support
   - Custom color schemes

5. **PWA Features**
   - Offline support
   - Install prompt
   - Background sync
