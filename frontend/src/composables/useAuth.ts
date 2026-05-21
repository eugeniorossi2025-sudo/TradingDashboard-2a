import { AuthConstants } from '@/constants/AuthConstants';
import { AuthService } from '@/service/AuthService';
import { computed } from 'vue';

/**
 * Composable per gestire autenticazione e autorizzazione nei componenti Vue
 */
export function useAuth() {
    // Computed properties
    const isAuthenticated = computed(() => AuthService.isAuthenticated.value);
    const isAdmin = computed(() => AuthService.isAdmin.value);
    const currentUser = computed(() => AuthService.currentUser.value);
    const userRoles = computed(() => AuthService.getUserRoles());
    const userPermissions = computed(() => AuthService.getUserPermissions());

    // Role checks
    const hasRole = (roleName: string) => AuthService.hasRole(roleName);
    const isUserRole = computed(() => hasRole(AuthConstants.Roles.User));
    const isBotOperator = computed(() => hasRole(AuthConstants.Roles.BotOperator));
    const isViewer = computed(() => hasRole(AuthConstants.Roles.Viewer));

    // Permission checks
    const hasPermission = (permission: string) => AuthService.hasPermission(permission);
    const hasAnyPermission = (...permissions: string[]) => AuthService.hasAnyPermission(...permissions);
    const hasAllPermissions = (...permissions: string[]) => AuthService.hasAllPermissions(...permissions);

    // Policy checks
    const checkPolicy = (policy: string) => AuthService.checkPolicy(policy);
    const canManageBots = computed(() => checkPolicy(AuthConstants.Policies.RequireAdminOrBotOperator));
    const canManageUsers = computed(() => hasPermission(AuthConstants.Permissions.UsersCreate));
    const canManageRoles = computed(() => hasPermission(AuthConstants.Permissions.RolesAssign));
    const canManagePermissions = computed(() => hasPermission(AuthConstants.Permissions.PermissionsAssign));
    const canViewLogs = computed(() => hasPermission(AuthConstants.Permissions.LogsView));
    const canManageDevices = computed(() => hasPermission(AuthConstants.Permissions.DevicesCreate));
    const canExecuteCommands = computed(() => hasPermission(AuthConstants.Permissions.CommandsExecute));

    // Specific permission checks
    const canViewDashboard = computed(() => hasPermission(AuthConstants.Permissions.DashboardView));
    const canManageDashboard = computed(() => hasPermission(AuthConstants.Permissions.DashboardManage));
    const canViewUsers = computed(() => hasPermission(AuthConstants.Permissions.UsersView));
    const canCreateUsers = computed(() => hasPermission(AuthConstants.Permissions.UsersCreate));
    const canEditUsers = computed(() => hasPermission(AuthConstants.Permissions.UsersEdit));
    const canDeleteUsers = computed(() => hasPermission(AuthConstants.Permissions.UsersDelete));
    const canViewConfigurations = computed(() => hasPermission(AuthConstants.Permissions.ConfigurationsView));
    const canEditConfigurations = computed(() => hasPermission(AuthConstants.Permissions.ConfigurationsEdit));

    return {
        // State
        isAuthenticated,
        isAdmin,
        currentUser,
        userRoles,
        userPermissions,

        // Role checks
        hasRole,
        isUserRole,
        isBotOperator,
        isViewer,

        // Permission checks
        hasPermission,
        hasAnyPermission,
        hasAllPermissions,

        // Policy checks
        checkPolicy,
        canManageBots,
        canManageUsers,
        canManageRoles,
        canManagePermissions,
        canViewLogs,
        canManageDevices,
        canExecuteCommands,

        // Specific permissions
        canViewDashboard,
        canManageDashboard,
        canViewUsers,
        canCreateUsers,
        canEditUsers,
        canDeleteUsers,
        canViewConfigurations,
        canEditConfigurations,

        // Constants
        AuthConstants
    };
}
