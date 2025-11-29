"use client";

import { VideoInfo } from "../types";
import styles from "./DownloadOptions.module.css";

interface DownloadOptionsProps {
    data: VideoInfo;
    originalUrl: string;
}

export default function DownloadOptions({ data, originalUrl }: DownloadOptionsProps) {
    const handleCopyLink = (url: string) => {
        navigator.clipboard.writeText(url);
        alert("Link copied to clipboard!");
    };

    const handleDownload = (quality: string | null) => {
        const apiUrl = process.env.NEXT_PUBLIC_API_URL || 'https://localhost:8084';
        const downloadUrl = `${apiUrl}/api/Download?url=${encodeURIComponent(originalUrl)}${quality ? `&quality=${quality}` : ''}`;

        // Trigger download
        window.location.href = downloadUrl;
    };

    const getDownloadLabel = (option: any) => {
        if (option.quality) {
            return `Download ${option.quality} (${option.type})`;
        }
        return `Download ${option.type}`;
    };

    const getQualityParam = (quality: string | null) => {
        if (!quality) return null;
        // Extract numeric value if format is like "1080x1920" or "720x1280"
        const match = quality.match(/(\d+)x(\d+)/);
        if (match) {
            const height = match[2]; // Use height for quality (e.g., 1920 -> 1920p)
            return height + 'p';
        }
        return 'best';
    };

    return (
        <div className={styles.container}>
            <div className={styles.header}>
                {/* eslint-disable-next-line @next/next/no-img-element */}
                <img
                    src={data.thumbnailUrl}
                    alt={data.title}
                    className={styles.thumbnail}
                />
                <div className={styles.info}>
                    <span className={styles.platform}>{data.platform}</span>
                    <h3 className={styles.title}>{data.title}</h3>
                </div>
            </div>

            <div className={styles.options}>
                {data.downloadOptions.map((option, index) => (
                    <div key={index} className={styles.buttonGroup}>
                        <button
                            className={styles.downloadBtn}
                            onClick={() => handleDownload(getQualityParam(option.quality))}
                        >
                            {getDownloadLabel(option)}
                        </button>
                        <button
                            className={`${styles.downloadBtn} ${styles.copyBtn}`}
                            onClick={() => handleCopyLink(option.url)}
                        >
                            Copy Link
                        </button>
                    </div>
                ))}
            </div>
        </div>
    );
}
