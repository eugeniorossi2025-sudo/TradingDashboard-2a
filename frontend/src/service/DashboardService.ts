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
        try {
            const response = await apiClient.get('/api/Dashboard/pc-current-status');
            const data = unwrap<any>(response);
            return data?.tables || data?.rows || data || null;
        } catch (error) {
            const response = await apiClient.get('/api/Dashboard/data');
            const data = unwrap<any>(response);
            return data?.tables || data?.rows || data || null;
        }
    },

    // 🔹 GET CHART DATA
    async getChartData(): Promise<any[] | null> {
        try {
            const response = await apiClient.get('/api/Dashboard/margini-chart');
            return unwrap<any[]>(response);
        } catch (error) {
            const response = await apiClient.get('/api/Dashboard/chart');
            return unwrap<any[]>(response);
        }
    },

    // 🔹 RESET DASHBOARD
    async resetDashboard(): Promise<void> {
        throw new Error('Reset dashboard endpoint is not implemented in the current backend.');
    },

     // 🔹 RESET DASHBOARD
    async stopDashboard(): Promise<void> {
        throw new Error('Emergency stop endpoint is not implemented in the current backend.');
    },
    
    // 🔹 GET STATISTICS DATA
};
