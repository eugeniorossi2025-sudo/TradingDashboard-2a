/**
 * Authentication and Authorization Constants
 * Mirrors backend AuthConstants configuration
 */
export const AuthConstants = {
    /**
     * Authorization Policies
     */
    Policies: {
        RequireAdmin: 'RequireAdmin',
        RequireUser: 'RequireUser',
        RequireBotOperator: 'RequireBotOperator',
        RequireAdminOrBotOperator: 'RequireAdminOrBotOperator'
    },

    /**
     * JWT Claims
     */
    Claims: {
        IsAdmin: 'isAdmin',
        Permissions: 'permissions',
        Role: 'role',
        UserId: 'sub',
        Username: 'unique_name',
        NameId: 'nameid'
    },

    /**
     * Permission Names
     */
    Permissions: {
        // Dashboard
        DashboardView: 'Dashboard.View',
        DashboardManage: 'Dashboard.Manage',

        // Users
        UsersView: 'Users.View',
        UsersCreate: 'Users.Create',
        UsersEdit: 'Users.Edit',
        UsersDelete: 'Users.Delete',

        // Roles
        RolesView: 'Roles.View',
        RolesAssign: 'Roles.Assign',

        // Permissions
        PermissionsView: 'Permissions.View',
        PermissionsAssign: 'Permissions.Assign',

        // Configurations
        ConfigurationsView: 'Configurations.View',
        ConfigurationsEdit: 'Configurations.Edit',

        // Logs
        LogsView: 'Logs.View',

        // Devices
        DevicesView: 'Devices.View',
        DevicesCreate: 'Devices.Create',
        DevicesEdit: 'Devices.Edit',
        DevicesDelete: 'Devices.Delete',

        // Commands
        CommandsExecute: 'Commands.Execute',

        // Bot Management
        BotManage: 'bot.manage'
    },

    /**
     * Role Names
     */
    Roles: {
        Admin: 'Admin',
        User: 'User',
        BotOperator: 'BotOperator',
        Viewer: 'Viewer'
    }
};
