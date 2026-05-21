import { getApiConfiguration } from '@/api/apiClient';
import { LogApi } from '@/api/client/api/log-api';

export const LogService = {
    // 🔹 GET LOGS CON FILTRI E PAGINAZIONE
    async getLogs(from?: string, to?: string, pc?: string, action?: number, description?: string, page: number = 1, pageSize: number = 10): Promise<any> {
        const logApi = new LogApi(getApiConfiguration());
        const response = await logApi.apiLogGet(from, to, pc, action, description, page, pageSize);
        return response.data;
    },

    // 🔹 RESET LOGS
    async resetLogs(): Promise<void> {
        const logApi = new LogApi(getApiConfiguration());
        await logApi.apiLogDelete();
    }
};

