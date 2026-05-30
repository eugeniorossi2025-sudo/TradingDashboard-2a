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

    // 🔹 GET CHART DATA
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
};
