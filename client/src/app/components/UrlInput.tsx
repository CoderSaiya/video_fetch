"use client";

import { useState } from "react";
import styles from "./UrlInput.module.css";

interface UrlInputProps {
    onUrlSubmit: (url: string) => void;
    isLoading: boolean;
}

export default function UrlInput({ onUrlSubmit, isLoading }: UrlInputProps) {
    const [url, setUrl] = useState("");
    const [error, setError] = useState("");

    const handlePaste = async () => {
        try {
            const text = await navigator.clipboard.readText();
            setUrl(text);
            setError("");
        } catch (err) {
            console.error("Failed to read clipboard", err);
        }
    };

    const handleSubmit = () => {
        if (!url.trim()) {
            setError("Please enter a URL");
            return;
        }
        // Basic validation
        if (!url.startsWith("http")) {
            setError("Please enter a valid URL starting with http/https");
            return;
        }

        setError("");
        onUrlSubmit(url);
    };

    return (
        <div className={styles.container}>
            <div className={styles.inputWrapper}>
                <input
                    type="text"
                    className={styles.input}
                    placeholder="Paste video link here..."
                    value={url}
                    onChange={(e) => {
                        setUrl(e.target.value);
                        if (error) setError("");
                    }}
                    disabled={isLoading}
                />
                <button
                    className={styles.pasteButton}
                    onClick={handlePaste}
                    disabled={isLoading}
                >
                    PASTE
                </button>
            </div>
            {error && <p className={styles.error}>{error}</p>}
            <button
                className={styles.submitButton}
                onClick={handleSubmit}
                disabled={isLoading || !url}
            >
                {isLoading ? "Processing..." : "Get Video"}
            </button>
        </div>
    );
}
