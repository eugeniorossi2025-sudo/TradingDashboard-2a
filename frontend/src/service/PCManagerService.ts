import { DeviceService, type CreateDeviceDTO, type Device, type UpdateDeviceDTO } from './DeviceService';

export const PCManagerService = {
    async getPCs(): Promise<Device[]> {
        return await DeviceService.getDevices();
    },
    
    async getPCById(id: string): Promise<Device | null> {
        return await DeviceService.getDeviceById(id);
    },
    
    async createPC(data: CreateDeviceDTO): Promise<Device> {
        return await DeviceService.createDevice(data);
    },
    
    async updatePC(id: string, data: UpdateDeviceDTO): Promise<void> {
        return await DeviceService.updateDevice(id, data);
    },
    
    async deletePC(id: string): Promise<void> {
        return await DeviceService.deleteDevice(id);
    }
};
