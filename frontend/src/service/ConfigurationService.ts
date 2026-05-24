import { apiClient, getApiConfiguration } from '@/api/apiClient';
import { ConfigurationApi } from '@/api/client/api/configuration-api';
import type {
    Configuration as ApiConfiguration,
    CreateConfigurationRequest,
    UpdateConfigurationRequest
} from '@/api/client/models';

export interface Configuration {
    id: number;
    k: string;
    description: string;
    pos: number;
    value: string;
}

export interface CreateConfigurationDTO {
    k: string;
    description: string;
    pos: number;
    value: string;
}

export interface UpdateConfigurationDTO {
    description?: string;
    pos?: number;
    value?: string;
}

// Mapping da API Configuration a Configuration locale
const mapApiConfiguration = (apiConfig: ApiConfiguration): Configuration => ({
    id: Number(apiConfig.id || 0),
    k: apiConfig.k || apiConfig.key || '',
    description: apiConfig.description || '',
    pos: apiConfig.pos || 0,
    value: apiConfig.value || '',
});

export const ConfigurationService = {
    // 🔹 GET ALL CONFIGURATIONS
    async getConfigurations(): Promise<Configuration[]> {
        const configApi = new ConfigurationApi(getApiConfiguration());
        const response = await configApi.apiConfigurationGet();
        
        if (response.data && Array.isArray(response.data)) {
            return (response.data as unknown as ApiConfiguration[]).map(mapApiConfiguration);
        }
        return [];
    },

    // 🔹 GET CONFIGURATION BY ID
    async getConfigurationById(id: string): Promise<Configuration | null> {
        try {
            const response = await apiClient.get(`/api/Configuration/key/${encodeURIComponent(id)}`);
            return response.data ? mapApiConfiguration(response.data as unknown as ApiConfiguration) : null;
        } catch (error) {
            if (error?.response?.status === 404) return null;
            throw error;
        }
    },

    // 🔹 CREATE CONFIGURATION (Admin only)
    async createConfiguration(data: CreateConfigurationDTO): Promise<Configuration> {
        const configApi = new ConfigurationApi(getApiConfiguration());
        
        const createRequest: CreateConfigurationRequest = {
            k: data.k,
            description: data.description,
            pos: data.pos,
            value: data.value,
        };

        const response = await configApi.apiConfigurationPost(createRequest);
        
        if (response.data) {
            return mapApiConfiguration(response.data as unknown as ApiConfiguration);
        }
        
        throw new Error('Failed to create configuration');
    },

    // 🔹 UPDATE CONFIGURATION (Admin only)
    async updateConfiguration(id: string, data: UpdateConfigurationDTO): Promise<boolean> {
        const configApi = new ConfigurationApi(getApiConfiguration());
        
        const updateRequest: UpdateConfigurationRequest = {
            description: data.description,
            pos: data.pos,
            value: data.value,
        };

        await configApi.apiConfigurationIdPut(id, updateRequest);
        return true;
    },

    // 🔹 DELETE CONFIGURATION (Admin only)
    async deleteConfiguration(id: string): Promise<boolean> {
        const configApi = new ConfigurationApi(getApiConfiguration());
        await configApi.apiConfigurationIdDelete(id);
        return true;
    },
};


