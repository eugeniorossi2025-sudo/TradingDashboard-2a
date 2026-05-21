import { getApiConfiguration } from '@/api/apiClient';
import { UserApi } from '@/api/client';
import { AuthApi } from '@/api/client/api/auth-api';
import type { LoginRequest, LoginResponse } from '@/api/client/models';
import { AuthConstants } from '@/constants/AuthConstants';
import { computed, ref } from 'vue';
import { TokenService } from './TokenService';

export interface CurrentUser {
    id: string;
    username: string;
    description: string;
    isAdmin: boolean;
    roles: string[];
    permissions: string[];
    token: string;
}

function decodeJWT(token: string): any {
    try {
        const base64Url = token.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(
            atob(base64)
                .split('')
                .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
                .join('')
        );
        return JSON.parse(jsonPayload);
    } catch (error) {
        console.error('Error decoding JWT:', error);
        return null;
    }
}

const currentUser = ref<CurrentUser | null>(null);

const loadUserFromStorage = () => {
    const user = TokenService.getUser();
    const token = TokenService.getToken();
    
    if (user && token) {
        currentUser.value = { 
            ...user, 
            token,
            roles: user.roles || [],
            permissions: user.permissions || []
        };
    }
};

loadUserFromStorage();

export const AuthService = {
    isAuthenticated: computed(() => !!currentUser.value),
    isAdmin: computed(() => currentUser.value?.isAdmin || false),
    currentUser: computed(() => currentUser.value),

    /**
     * Login utente
     */
    async login(username: string, password: string, rememberMe = false): Promise<LoginResponse> {
        const authApi = new AuthApi(getApiConfiguration());
        const loginRequest: LoginRequest = { username, password };

        const response = await authApi.apiAuthLoginPost(loginRequest);
        const loginData = response.data;

        if (loginData.token) {
            TokenService.setToken(loginData.token, rememberMe);
            
            const decodedToken = decodeJWT(loginData.token);
            
            let roles: string[] = [];
            if (decodedToken) {
                const roleClaim = decodedToken[AuthConstants.Claims.Role] || 
                                 decodedToken['role'] || 
                                 decodedToken['roles'] ||
                                 decodedToken['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
                
                if (typeof roleClaim === 'string') {
                    roles = [roleClaim];
                } else if (Array.isArray(roleClaim)) {
                    roles = roleClaim;
                }
            }
            
            // Determina se l'utente è admin controllando sia il claim che il ruolo
            let isAdmin = false;
            if (decodedToken) {
                const isAdminClaim = decodedToken[AuthConstants.Claims.IsAdmin] || 
                                    decodedToken['isAdmin'] ||
                                    decodedToken['IsAdmin'];
                
                isAdmin = isAdminClaim === 'true' || isAdminClaim === true;
            }
            // Se non c'è il claim isAdmin, controlla se ha il ruolo Admin
            if (!isAdmin && roles.some(r => r.toLowerCase() === 'admin')) {
                isAdmin = true;
            }
            
            // Extract permissions from token
            let permissions: string[] = [];
            if (decodedToken) {
                const permClaim = decodedToken[AuthConstants.Claims.Permissions] || 
                                 decodedToken['permissions'] ||
                                 decodedToken['permission'];
                
                if (typeof permClaim === 'string') {
                    // Permissions might be comma-separated or JSON array string
                    try {
                        permissions = JSON.parse(permClaim);
                    } catch {
                        permissions = permClaim.split(',').map(p => p.trim()).filter(p => p);
                    }
                } else if (Array.isArray(permClaim)) {
                    permissions = permClaim;
                }
            }
            
            // Crea utente con dati estratti dal token
            const user: CurrentUser = {
                id: decodedToken?.sub || decodedToken?.nameid || username,
                username: decodedToken?.unique_name || username,
                description: '',
                isAdmin: isAdmin,
                roles: roles,
                permissions: permissions,
                token: loginData.token,
            };

            TokenService.setUser(user, rememberMe);
            currentUser.value = user;

            return loginData;
        }

        throw new Error('Invalid login response - no token received');
    },

    /**
     * Logout utente
     */
    async logout(): Promise<void> {
        try {
            const authApi = new AuthApi(getApiConfiguration());
            await authApi.apiAuthLogoutPost();
        } catch (error) {
            console.error('Logout API error:', error);
        } finally {
            // Pulisci sempre lo stato locale usando TokenService
            TokenService.clearAll();
            currentUser.value = null;
        }
    },

    /**
     * Richiesta reset password
     */
    async resetPasswordRequest(email: string): Promise<void> {
        const authApi = new AuthApi(getApiConfiguration());
        await authApi.apiAuthResetPasswordRequestPost({ email });
    },

    /**
     * Conferma reset password
     */
    async resetPasswordConfirm(email: string, token: string, newPassword: string): Promise<void> {
        const authApi = new AuthApi(getApiConfiguration());
        await authApi.apiAuthResetPasswordConfirmPost({
            email,
            token,
            newPassword,
        });
    },

    /**
     * Verifica se l'utente ha un ruolo specifico
     */
    hasRole(roleName: string): boolean {
        if (!currentUser.value || !currentUser.value.roles) return false;
        return currentUser.value.roles.some(r => r.toLowerCase() === roleName.toLowerCase());
    },

    /**
     * Verifica se l'utente ha un permesso specifico
     * Gli Admin hanno accesso a tutti i permessi
     */
    hasPermission(permission: string): boolean {
        if (!currentUser.value) return false;
        // Gli Admin hanno accesso a tutto (controlla sia il flag isAdmin che il ruolo ADMIN)
        if (currentUser.value.isAdmin || this.hasRole('Admin')) return true;
        if (!currentUser.value.permissions) return false;
        return currentUser.value.permissions.includes(permission);
    },

    /**
     * Verifica se l'utente soddisfa una policy
     */
    checkPolicy(policy: string): boolean {
        if (!currentUser.value) return false;

        switch (policy) {
            case AuthConstants.Policies.RequireAdmin:
                return currentUser.value.isAdmin;

            case AuthConstants.Policies.RequireUser:
                return true; // Already authenticated

            case AuthConstants.Policies.RequireBotOperator:
                return this.hasPermission(AuthConstants.Permissions.BotManage);

            case AuthConstants.Policies.RequireAdminOrBotOperator:
                return currentUser.value.isAdmin || this.hasPermission(AuthConstants.Permissions.BotManage);

            default:
                console.warn(`Unknown policy: ${policy}`);
                return false;
        }
    },

    /**
     * Verifica se l'utente ha uno o più permessi
     */
    hasAnyPermission(...permissions: string[]): boolean {
        if (!currentUser.value) return false;
        return permissions.some(p => this.hasPermission(p));
    },

    /**
     * Verifica se l'utente ha tutti i permessi specificati
     */
    hasAllPermissions(...permissions: string[]): boolean {
        if (!currentUser.value) return false;
        return permissions.every(p => this.hasPermission(p));
    },

    /**
     * Ottieni tutti i ruoli dell'utente corrente
     */
    getUserRoles(): string[] {
        return currentUser.value?.roles || [];
    },

    /**
     * Ottieni tutti i permessi dell'utente corrente
     */
    getUserPermissions(): string[] {
        return currentUser.value?.permissions || [];
    },

        /**
     * Ottieni tutti i permessi disponibili nel sistema
     */
    async getAllPermissions(): Promise<string[]> {
        const userApi = new UserApi(getApiConfiguration());
        const response = await userApi.apiUserAvailablePermissionsGet();
        // L'API restituisce un oggetto con definedPermissions e rolePermissions
        return response.data.data?.definedPermissions || [];
    },

    /**
     * Ottieni l'utente corrente
     */
    getCurrentUser(): CurrentUser | null {
        return currentUser.value;
    },

    /**
     * Ricarica l'utente da sessionStorage
     */
    reloadUser(): void {
        loadUserFromStorage();
    },
};
