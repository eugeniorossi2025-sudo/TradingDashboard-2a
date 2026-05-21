import { apiClient } from '@/api/apiClient';

// DTO Interfaces
export interface BotSessionEventRequest {
    type: 'START' | 'STOP' | 'HEARTBEAT';
    pcName: string;
    botVersion: string;
    sessionData?: PcStartSession;
}

export interface BotSessionEventResponse {
    success: boolean;
    message: string;
    data: any;
    errors: string[] | null;
    timestamp: string;
}

export interface PcStartSession {
    // Session Details
    ACCOUNT: string;
    BOT_VERSION: string;
    PC_NAME: string;
    
    // Balance Fields
    SALDO_INIZIALE: number;
    SALDO_ISTANTANEO: number;
    VALORE_GIOCATO: number;
    
    // Game State
    TAVOLO: string;
    STATO: string;
    COLORE: string;
    MARGINE: number;
    COLPO_MARTINGALA: string;
    VALUTAZIONE: string;
    MEDIA_ORA: number;
    ORE: string;
}

export interface ActiveSession {
    id: string;
    pcName: string;
    botVersion: string;
    startDateTime: string;
    lastHeartbeat: string;
    sessionProfit: number;
    currentBalance: number;
    initialBalance: number;
    isActive: boolean;
}

class BotSessionService {
    private baseUrl = '/api/botsession';

    /**
     * Send Bot Session Event (START, STOP, HEARTBEAT)
     */
    async sendSessionEvent(request: BotSessionEventRequest): Promise<BotSessionEventResponse> {
        try {
            const response = await apiClient.post<BotSessionEventResponse>(
                `${this.baseUrl}/event`,
                request
            );
            return response.data;
        } catch (error: any) {
            console.error('Error sending bot session event:', error);
            throw error;
        }
    }

    /**
     * Get all active bot sessions
     */
    async getActiveSessions(): Promise<ActiveSession[]> {
        try {
            const response = await apiClient.get(`${this.baseUrl}/active`);
            const sessions = response.data.data || [];
            return sessions;
        } catch (error: any) {
            console.error('Error fetching active sessions:', error);
            throw error;
        }
    }

    /**
     * Cleanup inactive sessions (sessions without heartbeat for > 5 minutes)
     */
    async cleanupInactiveSessions(): Promise<BotSessionEventResponse> {
        try {
            const response = await apiClient.post<BotSessionEventResponse>(
                `${this.baseUrl}/cleanup-inactive`
            );
            return response.data;
        } catch (error: any) {
            console.error('Error cleaning up inactive sessions:', error);
            throw error;
        }
    }

    /**
     * Ping endpoint to check service availability
     */
    async ping(): Promise<{ message: string; timestamp: string }> {
        try {
            const response = await apiClient.get(`${this.baseUrl}/ping`);
            return response.data;
        } catch (error: any) {
            console.error('Error pinging bot session service:', error);
            throw error;
        }
    }

    /**
     * Start a bot session
     */
    async startSession(sessionData: PcStartSession): Promise<BotSessionEventResponse> {
        const request: BotSessionEventRequest = {
            type: 'START',
            pcName: sessionData.PC_NAME,
            botVersion: sessionData.BOT_VERSION,
            sessionData: sessionData
        };
        return this.sendSessionEvent(request);
    }

    /**
     * Stop a bot session
     */
    async stopSession(pcName: string, botVersion: string): Promise<BotSessionEventResponse> {
        const request: BotSessionEventRequest = {
            type: 'STOP',
            pcName: pcName,
            botVersion: botVersion
        };
        return this.sendSessionEvent(request);
    }

    /**
     * Send heartbeat for an active session
     */
    async sendHeartbeat(sessionData: PcStartSession): Promise<BotSessionEventResponse> {
        const request: BotSessionEventRequest = {
            type: 'HEARTBEAT',
            pcName: sessionData.PC_NAME,
            botVersion: sessionData.BOT_VERSION,
            sessionData: sessionData
        };
        return this.sendSessionEvent(request);
    }
}

export default new BotSessionService();
