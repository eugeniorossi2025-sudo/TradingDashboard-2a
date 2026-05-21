# Authorization System Documentation

This document explains how to use the authorization system in the frontend, which mirrors the backend policy configuration.

## Overview

The authorization system provides:
- **Role-based access control (RBAC)**
- **Permission-based access control**
- **Policy-based authorization** (matching backend policies)
- **Vue composables** for easy component integration
- **Vue directives** for declarative UI control
- **Router guards** for route protection

## Backend Policies (Mirrored)

The frontend matches these backend policies:

```csharp
// Backend policies
RequireAdmin          → Only users with isAdmin claim
RequireUser           → Any authenticated user
RequireBotOperator    → Users with "bot.manage" permission
RequireAdminOrBotOperator → Admin OR BotOperator
```

## Core Files

### 1. AuthConstants (`/src/constants/AuthConstants.ts`)

Defines all policies, permissions, roles, and claims.

```typescript
import { AuthConstants } from '@/constants/AuthConstants';

// Policies
AuthConstants.Policies.RequireAdmin
AuthConstants.Policies.RequireUser
AuthConstants.Policies.RequireBotOperator
AuthConstants.Policies.RequireAdminOrBotOperator

// Permissions
AuthConstants.Permissions.DashboardView
AuthConstants.Permissions.UsersCreate
AuthConstants.Permissions.BotManage
// ... etc

// Roles
AuthConstants.Roles.Admin
AuthConstants.Roles.User
AuthConstants.Roles.BotOperator
AuthConstants.Roles.Viewer
```

### 2. AuthService (`/src/service/AuthService.ts`)

Enhanced with JWT parsing to extract roles and permissions from token claims.

```typescript
import { AuthService } from '@/service/AuthService';

// Check authentication
AuthService.isAuthenticated.value
AuthService.isAdmin.value
AuthService.currentUser.value

// Check roles
AuthService.hasRole('Admin')
AuthService.getUserRoles() // ['Admin', 'User']

// Check permissions
AuthService.hasPermission('Users.Create')
AuthService.hasAnyPermission('Users.Create', 'Users.Edit')
AuthService.hasAllPermissions('Users.Create', 'Users.Edit')
AuthService.getUserPermissions() // ['Users.Create', 'Dashboard.View', ...]

// Check policies (matches backend)
AuthService.checkPolicy(AuthConstants.Policies.RequireAdmin)
AuthService.checkPolicy(AuthConstants.Policies.RequireAdminOrBotOperator)
```

## Usage in Vue Components

### Using the `useAuth()` Composable

```vue
<script setup>
import { useAuth } from '@/composables/useAuth';

const { 
    isAuthenticated,
    isAdmin,
    currentUser,
    hasPermission,
    hasRole,
    checkPolicy,
    canManageUsers,
    canManageBots,
    canExecuteCommands
} = useAuth();

function createUser() {
    if (!canManageUsers.value) {
        alert('You do not have permission to create users');
        return;
    }
    // ... create user logic
}
</script>

<template>
    <div>
        <h1>Welcome {{ currentUser?.username }}</h1>
        
        <!-- Show only to admins -->
        <Button v-if="isAdmin" @click="openAdminPanel">Admin Panel</Button>
        
        <!-- Show based on permission -->
        <Button v-if="canManageUsers" @click="createUser">Create User</Button>
        
        <!-- Show based on policy -->
        <Button v-if="checkPolicy(AuthConstants.Policies.RequireAdminOrBotOperator)">
            Manage Bots
        </Button>
    </div>
</template>
```

### Using Vue Directives

```vue
<template>
    <!-- Hide if user doesn't have permission -->
    <Button v-permission="'Users.Create'" @click="createUser">
        Create User
    </Button>
    
    <!-- Show if user has ANY of these permissions (OR) -->
    <Button v-permission="['Users.Create', 'Users.Edit']">
        Manage Users
    </Button>
    
    <!-- Show if user has ALL permissions (AND) -->
    <Button v-permission.all="['Users.Create', 'Users.Delete']">
        Full User Management
    </Button>
    
    <!-- Hide if user doesn't have role -->
    <div v-role="'Admin'">
        <h2>Admin Section</h2>
    </div>
    
    <!-- Show if user has any of these roles -->
    <div v-role="['Admin', 'BotOperator']">
        <h2>Management Section</h2>
    </div>
    
    <!-- Hide if user doesn't satisfy policy -->
    <Button v-policy="'RequireAdminOrBotOperator'" @click="manageBots">
        Manage Bots
    </Button>
</template>
```

## Router Guards

### Basic Guards

```javascript
import { authGuard, adminGuard, guestGuard } from '@/router/middleware';

{
    path: '/dashboard',
    beforeEnter: authGuard, // Requires authentication
    component: Dashboard
}

{
    path: '/admin',
    beforeEnter: adminGuard, // Requires admin role
    component: AdminPanel
}

{
    path: '/login',
    beforeEnter: guestGuard, // Only for non-authenticated users
    component: Login
}
```

### Advanced Guards

```javascript
import { 
    policyGuard, 
    permissionGuard, 
    roleGuard, 
    botOperatorGuard 
} from '@/router/middleware';
import { AuthConstants } from '@/constants/AuthConstants';

// Policy-based guard
{
    path: '/bots',
    beforeEnter: policyGuard(AuthConstants.Policies.RequireAdminOrBotOperator),
    component: BotManagement
}

// Permission-based guard (OR logic)
{
    path: '/users',
    beforeEnter: permissionGuard('Users.View', 'Users.Create'),
    component: UserManagement
}

// Role-based guard
{
    path: '/operators',
    beforeEnter: roleGuard('Admin', 'BotOperator'),
    component: OperatorPanel
}

// Specific bot operator guard
{
    path: '/bot-control',
    beforeEnter: botOperatorGuard, // Admin OR BotOperator
    component: BotControl
}
```

## JWT Token Structure

The backend sends a JWT token with these claims:

```json
{
    "sub": "user-id",
    "unique_name": "username",
    "isAdmin": "true",
    "role": ["Admin", "User"],
    "permissions": ["Users.Create", "Dashboard.View", "bot.manage"],
    "exp": 1234567890,
    "iss": "your-issuer",
    "aud": "your-audience"
}
```

The frontend automatically extracts:
- `isAdmin` → `currentUser.isAdmin`
- `role` / `roles` → `currentUser.roles[]`
- `permissions` → `currentUser.permissions[]`

## Common Permission Checks

```typescript
import { useAuth } from '@/composables/useAuth';

const {
    // Dashboard
    canViewDashboard,
    canManageDashboard,
    
    // Users
    canViewUsers,
    canCreateUsers,
    canEditUsers,
    canDeleteUsers,
    
    // Roles & Permissions
    canManageRoles,
    canManagePermissions,
    
    // Configurations
    canViewConfigurations,
    canEditConfigurations,
    
    // Logs
    canViewLogs,
    
    // Devices
    canManageDevices,
    
    // Commands
    canExecuteCommands,
    
    // Bots
    canManageBots
} = useAuth();
```

## Example: User Management Page

```vue
<script setup>
import { useAuth } from '@/composables/useAuth';
import { ref, onMounted } from 'vue';
import UserService from '@/service/UserService';

const { canCreateUsers, canEditUsers, canDeleteUsers } = useAuth();
const users = ref([]);

onMounted(async () => {
    users.value = await UserService.getUsers();
});

async function createUser() {
    if (!canCreateUsers.value) {
        toast.add({ severity: 'error', summary: 'Access Denied', detail: 'No permission to create users' });
        return;
    }
    // ... create logic
}

async function deleteUser(userId) {
    if (!canDeleteUsers.value) {
        toast.add({ severity: 'error', summary: 'Access Denied', detail: 'No permission to delete users' });
        return;
    }
    // ... delete logic
}
</script>

<template>
    <div>
        <Button 
            v-permission="'Users.Create'" 
            @click="createUser" 
            label="Create User" 
        />
        
        <DataTable :value="users">
            <Column field="username" header="Username" />
            <Column field="email" header="Email" />
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
                        @click="deleteUser(slotProps.data.id)" 
                    />
                </template>
            </Column>
        </DataTable>
    </div>
</template>
```

## Example: Dynamic Menu (Already Implemented)

See `/src/layout/AppMenu.vue` for the dynamic menu implementation using permission checks.

## Testing Authorization

### In Browser Console

```javascript
// Import AuthService in browser console (if available via dev tools)
// Or check in Vue DevTools

// Check current user
console.log(AuthService.currentUser.value);

// Check roles
console.log(AuthService.getUserRoles());

// Check permissions
console.log(AuthService.getUserPermissions());

// Test specific permission
console.log(AuthService.hasPermission('Users.Create'));

// Test policy
console.log(AuthService.checkPolicy('RequireAdmin'));
```

## Security Notes

1. **Frontend validation is for UX only** - Never rely solely on frontend checks for security
2. **Backend must enforce all policies** - Always verify permissions on the API side
3. **JWT tokens contain sensitive data** - Use HTTPS and secure token storage
4. **Token expiration** - Backend handles `ClockSkew = TimeSpan.Zero` for strict expiration
5. **SignalR authentication** - Token passed via query string for WebSocket connection

## Migration from Old System

If you're migrating from the old simple `isAdmin` check:

**Before:**
```vue
<Button v-if="AuthService.isAdmin.value">Admin Only</Button>
```

**After (more granular):**
```vue
<Button v-permission="'Users.Create'">Create User</Button>
<Button v-policy="'RequireAdminOrBotOperator'">Manage Bots</Button>
```

## Best Practices

1. **Use composables in script** - `useAuth()` for logic
2. **Use directives in template** - `v-permission`, `v-role`, `v-policy` for UI
3. **Use router guards** - Protect entire routes
4. **Check permissions before actions** - Always validate before API calls
5. **Provide user feedback** - Show toast messages for denied actions
6. **Keep permissions granular** - Use specific permissions instead of broad roles
7. **Match backend policies** - Ensure frontend checks mirror backend authorization

## Troubleshooting

### Permissions not working after login

```typescript
// Force reload user from storage
AuthService.reloadUser();
```

### JWT not being parsed correctly

Check browser console for JWT decode logs:
```typescript
// In AuthService.login(), logs will show:
// "Decoded JWT: { ... }"
// "Extracted isAdmin: true"
// "Extracted roles: ['Admin']"
// "Extracted permissions: ['Users.Create', ...]"
```

### Directive not hiding element

Ensure element is not overridden by CSS:
```css
/* Avoid using !important on display properties */
.my-button {
    display: block !important; /* ❌ Will override directive */
}
```
