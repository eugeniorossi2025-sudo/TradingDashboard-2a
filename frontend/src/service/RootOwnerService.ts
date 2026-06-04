import { apiClient } from '@/api/apiClient';

export interface RootOwnerMe {
    userId: number;
    username: string;
    isRootOwner: boolean;
}

export interface RootOwnerAuditRow {
    id: number;
    actorUserId?: number;
    actorUsername?: string;
    action: string;
    occurredAtUtc: string;
    ipAddress?: string;
    outcome: string;
    reason?: string;
}

export interface RootOwnerStatus {
    systemState: string;
    api: { enabled: boolean; reachable: boolean; statusCode: number; url: string; message?: string };
    database: { ok: boolean };
    activeBots: number;
    totalBotRows: number;
    activeMission?: {
        sessionId?: number;
        runtimeMode: string;
        startTimeUtc?: string;
        totalMargin: number;
        completed: boolean;
    };
    recentAudits: RootOwnerAuditRow[];
}

function unwrap<T>(response: { data: { success?: boolean; data?: T; message?: string; code?: string } }): T {
    const body = response.data;
    if (body?.success === false) {
        const err = new Error(body.message || 'Request failed') as Error & { code?: string; status?: number };
        err.code = body.code;
        throw err;
    }
    return body.data as T;
}

export const RootOwnerService = {
    async getMe(): Promise<RootOwnerMe> {
        const response = await apiClient.get('/api/root-owner/me');
        return unwrap<RootOwnerMe>(response);
    },

    async getStatus(): Promise<RootOwnerStatus> {
        const response = await apiClient.get('/api/root-owner/status');
        return unwrap<RootOwnerStatus>(response);
    },

    async pauseSystem(reason?: string) {
        const response = await apiClient.post('/api/root-owner/commands/pause-system', { reason });
        return unwrap(response);
    },

    async blackoutSystem(reason?: string) {
        const response = await apiClient.post('/api/root-owner/commands/blackout-system', { reason });
        return unwrap(response);
    },

    async reactivateSystem(reason?: string) {
        const response = await apiClient.post('/api/root-owner/commands/reactivate-system', { reason });
        return unwrap(response);
    },

    async stopAllBots(reason?: string) {
        const response = await apiClient.post('/api/root-owner/commands/stop-all-bots', { reason });
        return unwrap(response);
    },

    async stopActiveMission(reason?: string) {
        const response = await apiClient.post('/api/root-owner/commands/stop-active-mission', { reason });
        return unwrap(response);
    }
};
