import { TokenService } from '@/service/TokenService';
import * as signalR from '@microsoft/signalr';

export class SignalRService {
    private connection: signalR.HubConnection | null = null;
    private baseUrl: string;

    constructor() {
        this.baseUrl = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7203';
    }

    /**
     * Inizializza la connessione SignalR
     */
    async startConnection(hubPath: string = '/dashboardHub'): Promise<void> {
        const token = TokenService.getToken();

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(`${this.baseUrl}${hubPath}`, {
                accessTokenFactory: () => token || '',
                skipNegotiation: false,
                transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.ServerSentEvents | signalR.HttpTransportType.LongPolling,
            })
            .withAutomaticReconnect({
                nextRetryDelayInMilliseconds: (retryContext) => {
                    if (retryContext.previousRetryCount === 0) return 0;
                    if (retryContext.previousRetryCount === 1) return 2000;
                    if (retryContext.previousRetryCount === 2) return 10000;
                    return 30000;
                },
            })
            .configureLogging(signalR.LogLevel.Information)
            .build();

        // Event handlers per il ciclo di vita della connessione
        this.connection.onreconnecting((error) => {
            console.warn('SignalR reconnecting...', error);
        });

        this.connection.onreconnected((connectionId) => {
            console.log('SignalR reconnected:', connectionId);
        });

        this.connection.onclose((error) => {
            console.log('SignalR connection closed:', error);
        });

        try {
            await this.connection.start();
        } catch (error) {
            throw error;
        }
    }

    /**
     * Registra un listener per un evento SignalR
     */
    on(eventName: string, callback: (...args: any[]) => void): void {
        if (!this.connection) {
            console.error('SignalR connection not initialized');
            return;
        }
        this.connection.on(eventName, callback);
    }

    /**
     * Rimuove un listener per un evento SignalR
     */
    off(eventName: string, callback?: (...args: any[]) => void): void {
        if (!this.connection) {
            console.error('SignalR connection not initialized');
            return;
        }
        if (callback) {
            this.connection.off(eventName, callback);
        } else {
            this.connection.off(eventName);
        }
    }

    /**
     * Invia un messaggio al server via SignalR
     */
    async invoke(methodName: string, ...args: any[]): Promise<any> {
        if (!this.connection) {
            throw new Error('SignalR connection not initialized');
        }

        try {
            return await this.connection.invoke(methodName, ...args);
        } catch (error) {
            console.error(`Error invoking ${methodName}:`, error);
            throw error;
        }
    }

    /**
     * Invia un messaggio al server senza aspettare risposta
     */
    async send(methodName: string, ...args: any[]): Promise<void> {
        if (!this.connection) {
            throw new Error('SignalR connection not initialized');
        }

        try {
            await this.connection.send(methodName, ...args);
        } catch (error) {
            console.error(`Error sending ${methodName}:`, error);
            throw error;
        }
    }

    /**
     * Chiude la connessione SignalR
     */
    async stopConnection(): Promise<void> {
        if (this.connection) {
            try {
                await this.connection.stop();
                console.log('SignalR connection stopped');
            } catch (error) {
                console.error('Error stopping SignalR connection:', error);
            }
            this.connection = null;
        }
    }

    /**
     * Verifica se la connessione è attiva
     */
    isConnected(): boolean {
        return this.connection?.state === signalR.HubConnectionState.Connected;
    }

    /**
     * Ottiene lo stato della connessione
     */
    getConnectionState(): signalR.HubConnectionState | null {
        return this.connection?.state || null;
    }
}

// Export singleton instance
export const signalRService = new SignalRService();
