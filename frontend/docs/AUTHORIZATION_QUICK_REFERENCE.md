# Authorization Quick Reference

## Import and Setup

```typescript
import { useAuth } from '@/composables/useAuth';
import { AuthConstants } from '@/constants/AuthConstants';

const { 
    isAdmin, 
    hasPermission, 
    canCreateUsers, 
    canManageBots 
} = useAuth();
```

## Check Permissions in Script

```typescript
// Single permission
if (canCreateUsers.value) {
    // User can create
}

// Check specific permission
if (hasPermission('Users.Delete')) {
    // User can delete
}
```

## Hide UI Elements (Template)

```vue
<!-- Single permission -->
<Button v-permission="'Users.Create'">Create</Button>

<!-- Multiple permissions (OR) -->
<Button v-permission="['Users.Create', 'Users.Edit']">Manage</Button>

<!-- All permissions required (AND) -->
<Button v-permission.all="['Users.Create', 'Users.Delete']">Full Access</Button>

<!-- Role-based -->
<div v-role="'Admin'">Admin Only</div>

<!-- Policy-based -->
<Button v-policy="'RequireAdminOrBotOperator'">Manage Bots</Button>
```

## Protect Routes

```javascript
import { adminGuard, permissionGuard, policyGuard } from '@/router/middleware';
import { AuthConstants } from '@/constants/AuthConstants';

// Admin only
{
    path: '/admin',
    beforeEnter: adminGuard,
    component: AdminPanel
}

// Permission-based
{
    path: '/users',
    beforeEnter: permissionGuard('Users.View', 'Users.Create'),
    component: UserManagement
}

// Policy-based
{
    path: '/bots',
    beforeEnter: policyGuard(AuthConstants.Policies.RequireAdminOrBotOperator),
    component: BotManagement
}
```

## Available Permissions

```typescript
// Dashboard
AuthConstants.Permissions.DashboardView
AuthConstants.Permissions.DashboardManage

// Users
AuthConstants.Permissions.UsersView
AuthConstants.Permissions.UsersCreate
AuthConstants.Permissions.UsersEdit
AuthConstants.Permissions.UsersDelete

// Roles & Permissions
AuthConstants.Permissions.RolesView
AuthConstants.Permissions.RolesAssign
AuthConstants.Permissions.PermissionsView
AuthConstants.Permissions.PermissionsAssign

// Configurations
AuthConstants.Permissions.ConfigurationsView
AuthConstants.Permissions.ConfigurationsEdit

// Logs
AuthConstants.Permissions.LogsView

// Devices
AuthConstants.Permissions.DevicesView
AuthConstants.Permissions.DevicesCreate
AuthConstants.Permissions.DevicesEdit
AuthConstants.Permissions.DevicesDelete

// Commands
AuthConstants.Permissions.CommandsExecute

// Bots
AuthConstants.Permissions.BotManage
```

## Available Policies

```typescript
// Admin only
AuthConstants.Policies.RequireAdmin

// Any authenticated user
AuthConstants.Policies.RequireUser

// Bot operator only
AuthConstants.Policies.RequireBotOperator

// Admin OR Bot operator
AuthConstants.Policies.RequireAdminOrBotOperator
```

## Common Patterns

### Button with Permission Check

```vue
<script setup>
import { useAuth } from '@/composables/useAuth';

const { canCreateUsers } = useAuth();

function createUser() {
    if (!canCreateUsers.value) {
        toast.add({ 
            severity: 'error', 
            summary: 'Access Denied', 
            detail: 'No permission to create users' 
        });
        return;
    }
    // ... create logic
}
</script>

<template>
    <Button 
        v-permission="'Users.Create'" 
        @click="createUser"
        label="Create User" 
    />
</template>
```

### Conditional Rendering

```vue
<template>
    <!-- Show to admins only -->
    <div v-if="isAdmin">
        <h2>Admin Panel</h2>
    </div>
    
    <!-- Show based on permission -->
    <Button v-if="canManageUsers">Manage Users</Button>
    
    <!-- Show based on policy -->
    <section v-if="checkPolicy(AuthConstants.Policies.RequireAdminOrBotOperator)">
        Bot Management
    </section>
</template>
```

### DataTable Action Buttons

```vue
<Column>
    <template #body="slotProps">
        <Button 
            v-permission="'Users.Edit'"
            icon="pi pi-pencil"
            @click="editUser(slotProps.data)" 
        />
        <Button 
            v-permission="'Users.Delete'"
            icon="pi pi-trash"
            severity="danger"
            @click="deleteUser(slotProps.data)" 
        />
    </template>
</Column>
```

## Pre-built Composable Checks

```typescript
const {
    // User state
    isAuthenticated,      // Boolean: logged in
    isAdmin,             // Boolean: has admin role
    currentUser,         // Object: current user data
    userRoles,           // Array: user's roles
    userPermissions,     // Array: user's permissions
    
    // Common checks
    canManageBots,       // Admin OR BotOperator
    canManageUsers,      // Has Users.Create
    canManageRoles,      // Has Roles.Assign
    canViewLogs,         // Has Logs.View
    canManageDevices,    // Has Devices.Create
    canExecuteCommands,  // Has Commands.Execute
    
    // Specific permissions
    canViewDashboard,
    canManageDashboard,
    canViewUsers,
    canCreateUsers,
    canEditUsers,
    canDeleteUsers,
    canViewConfigurations,
    canEditConfigurations
} = useAuth();
```

## Backend Policy Mapping

| Frontend | Backend C# |
|----------|-----------|
| `AuthConstants.Policies.RequireAdmin` | `[Authorize(Policy = "RequireAdmin")]` |
| `AuthConstants.Policies.RequireUser` | `[Authorize(Policy = "RequireUser")]` |
| `AuthConstants.Policies.RequireBotOperator` | `[Authorize(Policy = "RequireBotOperator")]` |
| `AuthConstants.Policies.RequireAdminOrBotOperator` | `[Authorize(Policy = "RequireAdminOrBotOperator")]` |

## Security Reminder

⚠️ **Frontend checks are for UX only!**

All authorization must be enforced on the backend:
- API endpoints validate JWT claims
- Backend checks `isAdmin` claim
- Backend verifies `permissions` claim
- SignalR hub methods validate authorization

Frontend directives and guards improve user experience by hiding unavailable features, but backend is the source of truth.
