export interface DownloadOption {
    quality: string;
    url: string;
    type: string;
}

export interface VideoInfo {
    title: string;
    thumbnailUrl: string;
    platform: string;
    downloadOptions: DownloadOption[];
}
