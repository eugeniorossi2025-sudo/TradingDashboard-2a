import { apiClient } from '@/api/apiClient';

export interface DashboardData {
    tableData: any[];
    chartData: any[];
}

function unwrap<T>(response: { data: T | { data?: T } }): T {
    const body = response.data as T | { data?: T };
    if (body && typeof body === 'object' && 'data' in body) {
        return (body as { data?: T }).data as T;
    }
    return body as T;
}

export const DashboardService = {
    async getDashboardData(): Promise<any[] | null> {
        const response = await apiClient.get('/api/Dashboard/data');
        const data = unwrap<any>(response);
        return data?.tables || data?.rows || data || null;
    },

    async getChartData(): Promise<any[] | null> {
        const response = await apiClient.get('/api/Dashboard/chart');
        return unwrap<any[]>(response);
    },

    async resetDashboard(): Promise<any> {
        const response = await apiClient.post('/api/decider/reset');
        return unwrap<any>(response);
    },

    async stopDashboard(): Promise<void> {
        await apiClient.post('/api/decider/emergency-stop');
    },

    async getMarginiChart(limit = 200): Promise<any[]> {
        const response = await apiClient.get(`/api/Dashboard/margini-chart?limit=${limit}`);
        return unwrap<any[]>(response) ?? [];
    },

    async getTelemetry(): Promise<any | null> {
        const response = await apiClient.get('/api/Dashboard/telemetry');
        return unwrap<any>(response) ?? null;
    },

    async getSecurityFilterDetail(computer: string): Promise<any | null> {
        const response = await apiClient.get(`/api/Dashboard/security-filter/${encodeURIComponent(computer)}`);
        return unwrap<any>(response) ?? null;
    },

    async getPlayerRace5Filter(): Promise<{ enabled: boolean }> {
        const response = await apiClient.get('/api/player-race-5/filter');
        return unwrap<{ enabled: boolean }>(response);
    },

    async setPlayerRace5Filter(enabled: boolean): Promise<{ enabled: boolean }> {
        const response = await apiClient.put('/api/player-race-5/filter', { enabled });
        return unwrap<{ enabled: boolean }>(response);
    },

    async getPlayerRace5Ac3(): Promise<{ enabled: boolean }> {
        const response = await apiClient.get('/api/player-race-5/ac3');
        return unwrap<{ enabled: boolean }>(response);
    },

    async setPlayerRace5Ac3(enabled: boolean): Promise<{ enabled: boolean }> {
        const response = await apiClient.put('/api/player-race-5/ac3', { enabled });
        return unwrap<{ enabled: boolean }>(response);
    },

    async getPlayerRace8Filter(): Promise<{ enabled: boolean }> {
        const response = await apiClient.get('/api/player-race-8/filter');
        return unwrap<{ enabled: boolean }>(response);
    },

    async setPlayerRace8Filter(enabled: boolean): Promise<{ enabled: boolean }> {
        const response = await apiClient.put('/api/player-race-8/filter', { enabled });
        return unwrap<{ enabled: boolean }>(response);
    },

    async getPlayerRace8Ac3(): Promise<{ enabled: boolean }> {
        const response = await apiClient.get('/api/player-race-8/ac3');
        return unwrap<{ enabled: boolean }>(response);
    },

    async setPlayerRace8Ac3(enabled: boolean): Promise<{ enabled: boolean }> {
        const response = await apiClient.put('/api/player-race-8/ac3', { enabled });
        return unwrap<{ enabled: boolean }>(response);
    },

    async getSpotResetThreshold(): Promise<{ threshold: number }> {
        const response = await apiClient.get('/api/spot-reset/threshold');
        return unwrap<{ threshold: number }>(response);
    },

    async setSpotResetThreshold(threshold: number): Promise<{ threshold: number }> {
        const response = await apiClient.put('/api/spot-reset/threshold', { threshold });
        return unwrap<{ threshold: number }>(response);
    },

    async getSpotCyclePbHands(): Promise<{ hands: number }> {
        const response = await apiClient.get('/api/spot-reset/cycle-pb-hands');
        return unwrap<{ hands: number }>(response);
    },

    async setSpotCyclePbHands(hands: number): Promise<{ hands: number }> {
        const response = await apiClient.put('/api/spot-reset/cycle-pb-hands', { hands });
        return unwrap<{ hands: number }>(response);
    },

    async getSpotL6PerBot(): Promise<{ enabled: boolean }> {
        const response = await apiClient.get('/api/spot-l6-per-bot');
        return unwrap<{ enabled: boolean }>(response);
    },

    async setSpotL6PerBot(enabled: boolean): Promise<{ enabled: boolean }> {
        const response = await apiClient.put('/api/spot-l6-per-bot', { enabled });
        return unwrap<{ enabled: boolean }>(response);
    },

    async getSecurityFilter(): Promise<{
        enabled: boolean;
        maxAvgSeconds: number;
        veryFastSeconds: number;
        minScore: number;
    }> {
        const response = await apiClient.get('/api/security-filter');
        return unwrap<{
            enabled: boolean;
            maxAvgSeconds: number;
            veryFastSeconds: number;
            minScore: number;
        }>(response);
    },

    async setSecurityFilterEnabled(enabled: boolean): Promise<{ enabled: boolean }> {
        const response = await apiClient.put('/api/security-filter/enabled', { enabled });
        return unwrap<{ enabled: boolean }>(response);
    },

    async saveSecurityFilterParameters(params: {
        maxAvgSeconds: number;
        veryFastSeconds: number;
        minScore: number;
    }): Promise<{ maxAvgSeconds: number; veryFastSeconds: number; minScore: number }> {
        const response = await apiClient.put('/api/security-filter/parameters', params);
        return unwrap<{ maxAvgSeconds: number; veryFastSeconds: number; minScore: number }>(response);
    },
};
