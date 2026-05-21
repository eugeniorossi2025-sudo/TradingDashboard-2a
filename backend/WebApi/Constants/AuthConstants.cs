namespace WebApi.Constants;

/// <summary>
/// Constants for authentication and authorization.
/// </summary>
public static class AuthConstants
{
    /// <summary>
    /// Role names.
    /// </summary>
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string User = "User";
        public const string BotOperator = "BotOperator";
    }

    /// <summary>
    /// Policy names.
    /// </summary>
    public static class Policies
    {
        public const string RequireAdmin = "RequireAdmin";
        public const string RequireUser = "RequireUser";
        public const string RequireBotOperator = "RequireBotOperator";
        public const string RequireAdminOrBotOperator = "RequireAdminOrBotOperator";
    }

    /// <summary>
    /// Claim types.
    /// </summary>
    public static class Claims
    {
        public const string UserId = "userId";
        public const string IsAdmin = "isAdmin";
        public const string Permissions = "permissions";
    }

    /// <summary>
    /// Available permissions in the system.
    /// </summary>
    public static class AvailablePermissions
    {
        // User permissions
        public const string UserRead = "user.read";
        public const string UserWrite = "user.write";
        public const string UserDelete = "user.delete";
        
        // Bot permissions
        public const string BotManage = "bot.manage";
        public const string BotView = "bot.view";
        
        // Configuration permissions
        public const string ConfigManage = "config.manage";
        
        // Dashboard permissions
        public const string DashboardView = "Dashboard.View";
        public const string DashboardManage = "Dashboard.Manage";
        
        // Users management permissions
        public const string UsersView = "Users.View";
        public const string UsersCreate = "Users.Create";
        public const string UsersEdit = "Users.Edit";
        public const string UsersDelete = "Users.Delete";
        
        // Roles permissions
        public const string RolesView = "Roles.View";
        public const string RolesAssign = "Roles.Assign";
        
        // Permissions management
        public const string PermissionsView = "Permissions.View";
        public const string PermissionsAssign = "Permissions.Assign";
        
        // Configurations permissions
        public const string ConfigurationsView = "Configurations.View";
        public const string ConfigurationsEdit = "Configurations.Edit";
        
        // Logs permissions
        public const string LogsView = "Logs.View";
        
        // Devices permissions
        public const string DevicesView = "Devices.View";
        public const string DevicesCreate = "Devices.Create";
        public const string DevicesEdit = "Devices.Edit";
        public const string DevicesDelete = "Devices.Delete";
        
        // Commands permissions
        public const string CommandsExecute = "Commands.Execute";

        /// <summary>
        /// Gets all available permissions.
        /// </summary>
        public static IEnumerable<string> GetAll()
        {
            return new[]
            {
                // User permissions
                UserRead,
                UserWrite,
                UserDelete,
                
                // Bot permissions
                BotManage,
                BotView,
                
                // Configuration permissions
                ConfigManage,
                
                // Dashboard permissions
                DashboardView,
                DashboardManage,
                
                // Users management
                UsersView,
                UsersCreate,
                UsersEdit,
                UsersDelete,
                
                // Roles
                RolesView,
                RolesAssign,
                
                // Permissions
                PermissionsView,
                PermissionsAssign,
                
                // Configurations
                ConfigurationsView,
                ConfigurationsEdit,
                
                // Logs
                LogsView,
                
                // Devices
                DevicesView,
                DevicesCreate,
                DevicesEdit,
                DevicesDelete,
                
                // Commands
                CommandsExecute
            };
        }
    }
}