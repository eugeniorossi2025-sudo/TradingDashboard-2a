import { getApiConfiguration } from '@/api/apiClient';
import { UserApi } from '@/api/client/api/user-api';
import type {
    AssignPermissionRequest,
    AssignRoleRequest
} from '@/api/client/models';

export interface UserRolesAndPermissions {
    userId: string;
    userName: string;
    roles: string[];
    permissions: string[];
    isAdmin: boolean;
}

export const RoleService = {
    // 🔹 GET USER ROLES AND PERMISSIONS
    async getUserRolesAndPermissions(userId: string): Promise<UserRolesAndPermissions | null> {
        const userApi = new UserApi(getApiConfiguration());
        const response = await userApi.apiUserIdRolesAndPermissionsGet(userId);
        
        if (response.data.data) {
            const data = response.data.data;
            return {
                userId: data.userId || '',
                userName: data.userName || '',
                roles: data.roles || [],
                permissions: data.permissions || [],
                isAdmin: data.isAdmin || false
            };
        }
        return null;
    },

    // 🔹 ASSIGN ROLE TO USER
    async assignRole(userId: string, roleName: string): Promise<boolean> {
        const userApi = new UserApi(getApiConfiguration());
        const request: AssignRoleRequest = { roleName };
        const response = await userApi.apiUserIdRolesPost(userId, request);
        return response.data.success || false;
    },

    // 🔹 REMOVE ROLE FROM USER
    async removeRole(userId: string, roleName: string): Promise<boolean> {
        const userApi = new UserApi(getApiConfiguration());
        const response = await userApi.apiUserIdRolesRoleNameDelete(userId, roleName);
        return response.data.success || false;
    },

    // 🔹 ASSIGN PERMISSION TO USER
    async assignPermission(userId: string, permission: string): Promise<boolean> {
        const userApi = new UserApi(getApiConfiguration());
        const request: AssignPermissionRequest = { permission };
        const response = await userApi.apiUserIdPermissionsPost(userId, request);
        return response.data.success || false;
    },

    // 🔹 REMOVE PERMISSION FROM USER
    async removePermission(userId: string, permission: string): Promise<boolean> {
        const userApi = new UserApi(getApiConfiguration());
        const response = await userApi.apiUserIdPermissionsPermissionDelete(userId, permission);
        return response.data.success || false;
    },

    // 🔹 GET AVAILABLE ROLES (from API)
    async getAvailableRoles(): Promise<string[]> {
        try {
            const userApi = new UserApi(getApiConfiguration());
            const response = await userApi.apiUserAvailableRolesGet();
            return response.data.data || [];
        } catch (error) {
            console.error('Error fetching available roles:', error);
            return [];
        }
    },

    // 🔹 GET AVAILABLE PERMISSIONS (from API)
    async getAvailablePermissions(): Promise<string[]> {
        try {
            const userApi = new UserApi(getApiConfiguration());
            const response = await userApi.apiUserAvailablePermissionsGet();
            // L'API restituisce un oggetto con definedPermissions e rolePermissions
            return response.data.data?.definedPermissions || [];
        } catch (error) {
            console.error('Error fetching available permissions:', error);
            return [];
        }
    }
};
