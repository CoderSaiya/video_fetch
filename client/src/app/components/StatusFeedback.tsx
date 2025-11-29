import styles from "./StatusFeedback.module.css";

interface StatusFeedbackProps {
    isLoading: boolean;
    error: string | null;
}

export default function StatusFeedback({ isLoading, error }: StatusFeedbackProps) {
    if (isLoading) {
        return (
            <div className={styles.container}>
                <div className={styles.spinner}></div>
                <p style={{ marginTop: 10, color: "#666" }}>Fetching video info...</p>
            </div>
        );
    }

    if (error) {
        return (
            <div className={styles.container}>
                <div className={styles.error}>{error}</div>
            </div>
        );
    }

    return null;
}
