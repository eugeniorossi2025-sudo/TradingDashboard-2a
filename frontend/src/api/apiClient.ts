import { TokenService } from '@/service/TokenService';
import axios, { AxiosInstance, InternalAxiosRequestConfig } from 'axios';
import { Configuration } from './client/configuration';

// Base URL del backend
const BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5299';

// Istanza axios configurata
export const apiClient: AxiosInstance = axios.create({
    baseURL: BASE_URL,
    timeout: 30000,
    headers: {
        'Content-Type': 'application/json',
    },
});

// Interceptor per aggiungere il token JWT ad ogni richiesta
apiClient.interceptors.request.use(
    (config: InternalAxiosRequestConfig) => {
        const authHeader = TokenService.getAuthHeader();
        if (authHeader && config.headers) {
            config.headers.Authorization = authHeader;
        }

        return config;
    },
    (error) => {
        return Promise.reject(error);
    }
);

// Interceptor per gestire errori di autenticazione e autorizzazione
apiClient.interceptors.response.use(
    (response) => response,
    (error) => {
        if (error.response?.status === 401) {
            // 401 Unauthorized: Token scaduto o non valido
            console.warn('Token scaduto o non valido - Redirect al login');
            const currentPath = window.location.pathname + window.location.search;
            if (TokenService.getToken() && !String(error.config?.url || '').includes('/api/admin/access-events')) {
                apiClient.post('/api/admin/access-events', {
                    eventType: 'SESSION_TIMEOUT',
                    page: currentPath
                }).catch(() => {});
            }
            
            TokenService.clearAll();
            
            // Salva l'URL corrente per il redirect post-login
            if (currentPath !== '/auth/login' && !currentPath.includes('/auth/')) {
                TokenService.setRedirectPath(currentPath);
            }
            
            // Mostra messaggio se possibile
            if (window.location.pathname !== '/auth/login') {
                // Aggiungi parametro per mostrare messaggio di sessione scaduta
                window.location.href = '/auth/login?expired=true';
            }
        } else if (error.response?.status === 403) {
            // 403 Forbidden: Autenticato ma senza permessi
            console.warn('Access denied: 403 Forbidden', error.response.data);
            
            // Redirect alla pagina di accesso negato se non siamo già lì
            if (window.location.pathname !== '/auth/access-denied') {
                window.location.href = '/auth/access-denied';
            }
        }
        return Promise.reject(error);
    }
);

// Configurazione per il client OpenAPI generato
export const getApiConfiguration = (): Configuration => {
    const token = TokenService.getToken();
    return new Configuration({
        basePath: BASE_URL,
        apiKey: token ? `Bearer ${token}` : undefined,
    });
};
