/**
 * Service centralizzato per la gestione del token JWT
 * Supporta sessionStorage e localStorage ("Ricordami")
 */


const TOKEN_KEY = 'authToken';
const USER_KEY = 'currentUser';
const REDIRECT_KEY = 'redirectAfterLogin';
const REMEMBER_KEY = 'rememberMe';

export const TokenService = {
    /**
     * Salva il token in sessionStorage o localStorage
     * @param token string
     * @param rememberMe boolean (se true usa localStorage)
     */
    setToken(token: string, rememberMe = false): void {
        if (rememberMe) {
            localStorage.setItem(TOKEN_KEY, token);
            localStorage.setItem(REMEMBER_KEY, 'true');
            sessionStorage.removeItem(TOKEN_KEY);
        } else {
            sessionStorage.setItem(TOKEN_KEY, token);
            localStorage.removeItem(TOKEN_KEY);
            localStorage.removeItem(REMEMBER_KEY);
        }
    },

    /**
     * Ottieni il token da localStorage o sessionStorage
     */
    getToken(): string | null {
        return localStorage.getItem(TOKEN_KEY) || sessionStorage.getItem(TOKEN_KEY);
    },

    /**
     * Rimuovi il token da entrambi
     */
    removeToken(): void {
        sessionStorage.removeItem(TOKEN_KEY);
        localStorage.removeItem(TOKEN_KEY);
        localStorage.removeItem(REMEMBER_KEY);
    },

    /**
     * Verifica se esiste un token
     */
    hasToken(): boolean {
        return !!this.getToken();
    },

    /**
     * Salva i dati utente in sessionStorage o localStorage
     * @param user any
     * @param rememberMe boolean (se true usa localStorage)
     */
    setUser(user: any, rememberMe = false): void {
        if (rememberMe) {
            localStorage.setItem(USER_KEY, JSON.stringify(user));
            localStorage.setItem(REMEMBER_KEY, 'true');
            sessionStorage.removeItem(USER_KEY);
        } else {
            sessionStorage.setItem(USER_KEY, JSON.stringify(user));
            localStorage.removeItem(USER_KEY);
            localStorage.removeItem(REMEMBER_KEY);
        }
    },

    /**
     * Ottieni i dati utente da localStorage o sessionStorage
     */
    getUser(): any | null {
        const userJson = localStorage.getItem(USER_KEY) || sessionStorage.getItem(USER_KEY);
        if (userJson) {
            try {
                return JSON.parse(userJson);
            } catch (error) {
                console.error('Error parsing user from storage:', error);
                return null;
            }
        }
        return null;
    },

    /**
     * Rimuovi i dati utente da entrambi
     */
    removeUser(): void {
        sessionStorage.removeItem(USER_KEY);
        localStorage.removeItem(USER_KEY);
        localStorage.removeItem(REMEMBER_KEY);
    },

    /**
     * Salva il redirect path (sempre in sessionStorage)
     */
    setRedirectPath(path: string): void {
        sessionStorage.setItem(REDIRECT_KEY, path);
    },

    /**
     * Ottieni e rimuovi il redirect path (sempre da sessionStorage)
     */
    getAndClearRedirectPath(): string | null {
        const path = sessionStorage.getItem(REDIRECT_KEY);
        if (path) {
            sessionStorage.removeItem(REDIRECT_KEY);
        }
        return path;
    },

    /**
     * Pulisci tutti i dati di autenticazione da entrambi gli storage
     */
    clearAll(): void {
        this.removeToken();
        this.removeUser();
        sessionStorage.removeItem(REDIRECT_KEY);
    },

    /**
     * Ottieni header Authorization per API calls
     */
    getAuthHeader(): string | null {
        const token = this.getToken();
        return token ? `Bearer ${token}` : null;
    },

    /**
     * Verifica se è attivo il rememberMe
     */
    isRememberMe(): boolean {
        return localStorage.getItem(REMEMBER_KEY) === 'true';
    }
};
