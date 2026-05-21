import { getApiConfiguration } from '@/api/apiClient';
import { DashboardApi } from '@/api/client/api/dashboard-api';
import type {
    ChartDataPoint,
    PcCurrentStatus
} from '@/api/client/models';

export interface DashboardData {
    tableData: PcCurrentStatus[];
    chartData: ChartDataPoint[];
}

export const DashboardService = {
    async getDashboardData(): Promise<PcCurrentStatus[] | null> {
        const dashboardApi = new DashboardApi(getApiConfiguration());
        const response = await dashboardApi.apiDashboardPcCurrentStatusGet();
        
        if (response.data) {
            return response.data;
        }
        return null;
    },

    // 🔹 GET CHART DATA
    async getChartData(): Promise<ChartDataPoint[] | null> {
        const dashboardApi = new DashboardApi(getApiConfiguration());
        const response = await dashboardApi.apiDashboardMarginiChartGet();
        if (response.data) {
            return response.data;
        }
        return null;
    },

    // 🔹 RESET DASHBOARD
    async resetDashboard(): Promise<void> {
        const dashboardApi = new DashboardApi(getApiConfiguration());
        // Chiamata POST di reset, endpoint da implementare lato backend se non esiste
        await dashboardApi.apiDashboardResetTablesPost();
    },

     // 🔹 RESET DASHBOARD
    async stopDashboard(): Promise<void> {
        const dashboardApi = new DashboardApi(getApiConfiguration());
        // Chiamata POST di reset, endpoint da implementare lato backend se non esiste
        await dashboardApi.apiDashboardEmergencyStopPost();
    },
    
    // 🔹 GET STATISTICS DATA
};
