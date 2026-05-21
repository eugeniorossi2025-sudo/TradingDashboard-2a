import { apiClient } from '@/api/apiClient';

export interface Device {
    id: string;
    title: string;
    stato: number;
    amount: number;
    lastUpdate?: string | null;
}

export interface CreateDeviceDTO {
    id?: string | null;
    title: string;
    stato: number;
    amount: number;
}

export interface UpdateDeviceDTO {
    title: string;
    stato: number;
    amount: number;
}

export const DeviceService = {
    // 🔹 GET ALL DEVICES
    async getDevices(): Promise<Device[]> {
        try {
            const response = await apiClient.get('/api/Device');
            // L'API restituisce { success, message, data, errors, timestamp }
            return response.data?.data || [];
        } catch (error) {
            console.error('Error fetching devices:', error);
            throw error;
        }
    },

    // 🔹 GET DEVICE BY ID
    async getDeviceById(id: string): Promise<Device | null> {
        try {
            const response = await apiClient.get(`/api/Device/${id}`);
            // L'API restituisce { success, message, data, errors, timestamp }
            return response.data?.data || null;
        } catch (error) {
            console.error(`Error fetching device ${id}:`, error);
            throw error;
        }
    },

    // 🔹 CREATE DEVICE (Admin only)
    async createDevice(data: CreateDeviceDTO): Promise<Device> {
        try {
            const response = await apiClient.post('/api/Device', data);
            // L'API restituisce { success, message, data, errors, timestamp }
            return response.data?.data;
        } catch (error) {
            console.error('Error creating device:', error);
            throw error;
        }
    },

    // 🔹 UPDATE DEVICE (Admin only)
    async updateDevice(id: string, data: UpdateDeviceDTO): Promise<void> {
        try {
            await apiClient.put(`/api/Device/${id}`, data);
        } catch (error) {
            console.error(`Error updating device ${id}:`, error);
            throw error;
        }
    },

    // 🔹 DELETE DEVICE (Admin only)
    async deleteDevice(id: string): Promise<void> {
        try {
            await apiClient.delete(`/api/Device/${id}`);
        } catch (error) {
            console.error(`Error deleting device ${id}:`, error);
            throw error;
        }
    },
};

