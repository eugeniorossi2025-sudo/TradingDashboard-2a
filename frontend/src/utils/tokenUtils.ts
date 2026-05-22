/**
 * Utility per gestione JWT Token
 */

export interface DecodedToken {
    sub?: string;
    unique_name?: string;
    nameid?: string;
    isAdmin?: string | boolean;
    role?: string | string[];
    roles?: string | string[];
    permissions?: string | string[];
    exp?: number;
    iss?: string;
    aud?: string;
}

/**
 * Decodifica un JWT token
 */
export function decodeJWT(token: string): DecodedToken | null {
    try {
        const base64Url = token.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(
            atob(base64)
                .split('')
                .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
                .join('')
        );
        return JSON.parse(jsonPayload);
    } catch (error) {
        console.error('Error decoding JWT:', error);
        return null;
    }
}

/**
 * Verifica se il token è scaduto
 */
export function isTokenExpired(token: string): boolean {
    const decoded = decodeJWT(token);
    if (!decoded || !decoded.exp) {
        return true;
    }

    // exp è in secondi, Date.now() è in millisecondi
    const expirationDate = new Date(decoded.exp * 1000);
    const now = new Date();

    return now >= expirationDate;
}

/**
 * Ottieni il tempo rimanente prima della scadenza (in millisecondi)
 */
export function getTokenTimeRemaining(token: string): number {
    const decoded = decodeJWT(token);
    if (!decoded || !decoded.exp) {
        return 0;
    }

    const expirationDate = new Date(decoded.exp * 1000);
    const now = new Date();
    const timeRemaining = expirationDate.getTime() - now.getTime();

    return Math.max(0, timeRemaining);
}

/**
 * Formatta il tempo rimanente in modo leggibile
 */
export function formatTimeRemaining(milliseconds: number): string {
    if (milliseconds <= 0) {
        return 'Scaduto';
    }

    const seconds = Math.floor(milliseconds / 1000);
    const minutes = Math.floor(seconds / 60);
    const hours = Math.floor(minutes / 60);

    if (hours > 0) {
        return `${hours}h ${minutes % 60}m`;
    } else if (minutes > 0) {
        return `${minutes}m ${seconds % 60}s`;
    } else {
        return `${seconds}s`;
    }
}

/**
 * Verifica se il token sta per scadere (entro 5 minuti)
 */
export function isTokenExpiringSoon(token: string, thresholdMinutes: number = 5): boolean {
    const timeRemaining = getTokenTimeRemaining(token);
    const thresholdMs = thresholdMinutes * 60 * 1000;
    
    return timeRemaining > 0 && timeRemaining <= thresholdMs;
}

/**
 * Ottieni informazioni sulla scadenza del token
 */
export function getTokenExpirationInfo(token: string): {
    isExpired: boolean;
    isExpiringSoon: boolean;
    timeRemaining: number;
    timeRemainingFormatted: string;
    expirationDate: Date | null;
} {
    const decoded = decodeJWT(token);
    const isExpired = isTokenExpired(token);
    const isExpiringSoon = isTokenExpiringSoon(token);
    const timeRemaining = getTokenTimeRemaining(token);
    const timeRemainingFormatted = formatTimeRemaining(timeRemaining);
    
    let expirationDate: Date | null = null;
    if (decoded?.exp) {
        expirationDate = new Date(decoded.exp * 1000);
    }

    return {
        isExpired,
        isExpiringSoon,
        timeRemaining,
        timeRemainingFormatted,
        expirationDate
    };
}
