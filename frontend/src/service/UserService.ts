import { apiClient, getApiConfiguration } from '@/api/apiClient';
import { UserApi } from '@/api/client/api/user-api';
import type { User as ApiUser, CreateUserRequest } from '@/api/client/models';
import { RoleService } from './RoleService';

export interface User {
    id: string;
    description: string;
    username: string;
    email: string;
    isAdmin: boolean;
    lastLoginDate: string | null;
    roles?: string[];
    permissions?: string[];
}

export interface CreateUserDTO {
    description: string;
    username: string;
    email: string;
    password: string;
    isAdmin: boolean;
}

export interface UpdateUserDTO {
    description?: string;
    username?: string;
    email?: string;
    password?: string;
    isAdmin?: boolean;
}

export interface AdminUserOverviewRow {
    userId: number;
    username: string;
    email?: string | null;
    role: string;
    roles: string[];
    accountType: string;
    status: string;
    lastLoginUtc?: string | null;
    lastIp?: string | null;
    lastPage?: string | null;
    lastEvent?: string | null;
    enabled: boolean;
}

export interface AdminUsersOverview {
    operative: AdminUserOverviewRow[];
    bots: AdminUserOverviewRow[];
    admins: AdminUserOverviewRow[];
}

export interface UserNotificationSetting {
    userId: number;
    username: string;
    loginEmail?: string | null;
    notificationEmail?: string | null;
    enabled: boolean;
    mission: boolean;
    system: boolean;
    errors: boolean;
}

export interface UserAccessEvent {
    id: number;
    userId?: number | null;
    username?: string | null;
    eventType: string;
    ipAddress?: string | null;
    page?: string | null;
    userAgent?: string | null;
    occurredAtUtc: string;
}

function unwrap<T>(response: { data: T | { data?: T } }): T {
    const body = response.data as T | { data?: T };
    if (body && typeof body === 'object' && 'data' in body) {
        return (body as { data?: T }).data as T;
    }
    return body as T;
}

// Mapping da API User a User locale (senza caricare ruoli/permessi)
const mapApiUser = (apiUser: ApiUser): User => ({
    id: String(apiUser.id || ''),
    description: apiUser.description || '',
    username: apiUser.userName || '',
    email: apiUser.email || '',
    isAdmin: apiUser.admin || false,
    lastLoginDate: apiUser.lastLogin || null,
    roles: [],
    permissions: []
});

export const UserService = {
    // 🔹 GET ALL USERS (Admin only)
    async getUsers(): Promise<User[]> {
        const userApi = new UserApi(getApiConfiguration());
    
        try {
            const response = await userApi.apiUserGet();
            
            if (response.data.data) {
                return response.data.data.map(mapApiUser);
            }
            return [];
        } catch (error: any) {
            console.error('Error fetching users:', error);
            throw error;
        }
    },

    // 🔹 GET USER BY ID (with roles and permissions)
    async getUserById(id: string): Promise<User | null> {
        const userApi = new UserApi(getApiConfiguration());
        const response = await userApi.apiUserIdGet(id);
        
        if (response.data.data) {
            const user = mapApiUser(response.data.data);
            
            // Load roles and permissions
            try {
                const rolesAndPerms = await RoleService.getUserRolesAndPermissions(user.id);
                if (rolesAndPerms) {
                    user.roles = rolesAndPerms.roles;
                    user.permissions = rolesAndPerms.permissions;
                }
            } catch (error) {
                console.warn(`Failed to fetch roles/permissions for user ${user.id}:`, error);
            }
            
            return user;
        }
        return null;
    },

    // 🔹 GET USER WITH ROLES AND PERMISSIONS
    async getUserWithRolesAndPermissions(id: string): Promise<User | null> {
        return this.getUserById(id);
    },

    // 🔹 CREATE USER (Admin only)
    async createUser(userData: CreateUserDTO, roleName?: string): Promise<User> {
        const createRequest: CreateUserRequest = {
            username: userData.username,
            email: userData.email,
            description: userData.description,
            password: userData.password,
            isAdmin: userData.isAdmin,
        };

        const response = await apiClient.post('/api/User', createRequest, {
            params: roleName ? { roleName } : undefined,
        });

        const payload = response.data as { data?: ApiUser };
        if (payload?.data) {
            return mapApiUser(payload.data);
        }

        throw new Error('Failed to create user');
    },

    // 🔹 DELETE USER (Admin only)
    async deleteUser(id: string): Promise<boolean> {
        const userApi = new UserApi(getApiConfiguration());
        const response = await userApi.apiUserIdDelete(id);
        return response.data.success || false;
    },

    async getAdminOverview(): Promise<AdminUsersOverview> {
        const response = await apiClient.get('/api/admin/users/overview');
        return unwrap<AdminUsersOverview>(response);
    },

    async getNotificationSettings(): Promise<UserNotificationSetting[]> {
        const response = await apiClient.get('/api/admin/user-notification-settings');
        return unwrap<UserNotificationSetting[]>(response);
    },

    async saveNotificationSetting(setting: UserNotificationSetting): Promise<void> {
        await apiClient.put(`/api/admin/user-notification-settings/${encodeURIComponent(String(setting.userId))}`, {
            notificationEmail: setting.notificationEmail,
            enabled: setting.enabled,
            mission: setting.mission,
            system: setting.system,
            errors: setting.errors
        });
    },

    async sendNotificationTest(userId: number): Promise<void> {
        await apiClient.post(`/api/admin/user-notification-settings/${encodeURIComponent(String(userId))}/test`);
    },

    async getAccessReport(userId: number): Promise<UserAccessEvent[]> {
        const response = await apiClient.get(`/api/admin/users/${encodeURIComponent(String(userId))}/access-report`, {
            params: { limit: 250 }
        });
        return unwrap<UserAccessEvent[]>(response);
    },

    async disableUser(userId: number): Promise<void> {
        await apiClient.post(`/api/admin/users/${encodeURIComponent(String(userId))}/disable`);
    },

    async enableUser(userId: number): Promise<void> {
        await apiClient.post(`/api/admin/users/${encodeURIComponent(String(userId))}/enable`);
    },

    async deleteAdminUser(userId: number): Promise<void> {
        await apiClient.delete(`/api/admin/users/${encodeURIComponent(String(userId))}`);
    },

    async trackAccessEvent(eventType: string, page?: string): Promise<void> {
        await apiClient.post('/api/admin/access-events', {
            eventType,
            page: page || window.location.pathname
        });
    },
};
