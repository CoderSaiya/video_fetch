"use client";

import { useState } from "react";
import UrlInput from "./components/UrlInput";
import DownloadOptions from "./components/DownloadOptions";
import StatusFeedback from "./components/StatusFeedback";
import { VideoInfo } from "./types";

export default function Home() {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [videoInfo, setVideoInfo] = useState<VideoInfo | null>(null);
  const [originalUrl, setOriginalUrl] = useState<string>("");

  const handleUrlSubmit = async (url: string) => {
    setIsLoading(true);
    setError(null);
    setVideoInfo(null);
    setOriginalUrl(url);

    try {
      const apiUrl = process.env.NEXT_PUBLIC_API_URL || 'https://localhost:8084';
      const response = await fetch(`${apiUrl}/api/Info`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ url }),
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || "Failed to fetch video info");
      }

      const data: VideoInfo = await response.json();
      setVideoInfo(data);
    } catch (err: any) {
      setError(err.message || "An unexpected error occurred");
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <>
      <header style={{ padding: "20px", borderBottom: "1px solid #eee" }}>
        <h1 style={{ fontSize: "20px", fontWeight: "bold", color: "var(--primary-color)" }}>
          Video Downloader
        </h1>
      </header>

      <UrlInput onUrlSubmit={handleUrlSubmit} isLoading={isLoading} />

      <StatusFeedback isLoading={isLoading} error={error} />

      {videoInfo && <DownloadOptions data={videoInfo} originalUrl={originalUrl} />}

      <footer style={{ padding: "20px", textAlign: "center", fontSize: "12px", color: "#999", marginTop: "auto" }}>
        &copy; 2025 Video Downloader
      </footer>
    </>
  );
}
