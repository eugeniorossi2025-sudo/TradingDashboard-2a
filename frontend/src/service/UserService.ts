import { getApiConfiguration } from '@/api/apiClient';
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
        const userApi = new UserApi(getApiConfiguration());
        
        const createRequest: CreateUserRequest = {
            username: userData.username,
            email: userData.email,
            description: userData.description,
            password: userData.password,
            isAdmin: userData.isAdmin,
        };

        const response = await userApi.apiUserPost(roleName, createRequest);
        
        if (response.data.data) {
            return mapApiUser(response.data.data);
        }
        
        throw new Error('Failed to create user');
    },

    // 🔹 DELETE USER (Admin only)
    async deleteUser(id: string): Promise<boolean> {
        const userApi = new UserApi(getApiConfiguration());
        const response = await userApi.apiUserIdDelete(id);
        return response.data.success || false;
    },
};
