<script setup>
import { useAuth } from '@/composables/useAuth';
import { AuthService } from '@/service/AuthService';
import { RoleService } from '@/service/RoleService';
import { UserService } from '@/service/UserService';
import { FilterMatchMode } from '@primevue/core/api';
import { useToast } from 'primevue/usetoast';
import { onMounted, ref } from 'vue';

const { canCreateUsers, canEditUsers, canDeleteUsers, canManageRoles, canManagePermissions } = useAuth();
const toast = useToast();
const dt = ref();
const users = ref([]);
const userDialog = ref(false);
const deleteuserDialog = ref(false);
const deleteUsersDialog = ref(false);
const rolesPermissionsDialog = ref(false);
const user = ref({});
const selectedUsers = ref();
const filters = ref({
    global: { value: null, matchMode: FilterMatchMode.CONTAINS }
});
const submitted = ref(false);
const loading = ref(false);

// Roles and Permissions
const currentUserRolesPerms = ref({ roles: [], permissions: [] });
const availableRoles = ref([]);
const availablePermissions = ref([]);
const loadingRolesPerms = ref(false);

// 🔹 LOAD USERS ON MOUNT
onMounted(async () => {
    await loadUsers();
    await loadAvailableRolesAndPermissions();
});

const loadAvailableRolesAndPermissions = async () => {
    try {
        availableRoles.value = await RoleService.getAvailableRoles();
        availablePermissions.value = await AuthService.getAllPermissions();
    } catch (error) {
        console.error('Error loading available roles/permissions:', error);
        toast.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to load available roles and permissions',
            life: 3000
        });
    }
};

const loadUsers = async () => {
    try {
        loading.value = true;
        users.value = await UserService.getUsers();
    } catch (error) {
        console.error('Error loading users:', error);
        toast.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to load users',
            life: 3000
        });
    } finally {
        loading.value = false;
    }
};

const openNew = () => {
    if (!canCreateUsers.value) {
        toast.add({
            severity: 'error',
            summary: 'Access Denied',
            detail: 'You do not have permission to create users',
            life: 3000
        });
        return;
    }

    user.value = {
        username: '',
        description: '',
        email: '',
        password: '',
        administrator: false
    };
    submitted.value = false;
    userDialog.value = true;
};

const hideDialog = () => {
    userDialog.value = false;
    submitted.value = false;
};

const saveUser = async () => {
    submitted.value = true;

    if (!user.value.username?.trim()) {
        return;
    }

    try {
        if (user.value.id) {
            if (!canEditUsers.value) {
                toast.add({
                    severity: 'error',
                    summary: 'Access Denied',
                    detail: 'You do not have permission to edit users',
                    life: 3000
                });
                return;
            }

            toast.add({
                severity: 'warn',
                summary: 'Not Supported',
                detail: 'User update is not available. Please delete and recreate the user.',
                life: 5000
            });
            return;
        } else {
            // CREATE USER
            if (!canCreateUsers.value) {
                toast.add({
                    severity: 'error',
                    summary: 'Access Denied',
                    detail: 'You do not have permission to create users',
                    life: 3000
                });
                return;
            }

            if (!user.value.password?.trim()) {
                toast.add({
                    severity: 'warn',
                    summary: 'Validation',
                    detail: 'Password is required for new users',
                    life: 3000
                });
                return;
            }

            if (!user.value.email?.trim()) {
                toast.add({
                    severity: 'warn',
                    summary: 'Validation',
                    detail: 'Email is required',
                    life: 3000
                });
                return;
            }

            await UserService.createUser({
                username: user.value.username,
                description: user.value.description,
                email: user.value.email,
                password: user.value.password,
                isAdmin: user.value.administrator
            });

            toast.add({
                severity: 'success',
                summary: 'Success',
                detail: 'User created successfully',
                life: 3000
            });
        }

        userDialog.value = false;
        user.value = {};
        await loadUsers();
    } catch (error) {
        console.error('Error saving user:', error);
        toast.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to save user',
            life: 3000
        });
    }
};

const editUser = (userData) => {
    user.value = {
        id: userData.id,
        username: userData.username,
        description: userData.description,
        email: userData.email,
        administrator: userData.isAdmin,
        lastLoginDate: userData.lastLoginDate,
        password: '' // Empty password for edit
    };
    userDialog.value = true;
};

const confirmDeleteUser = (userData) => {
    if (!canDeleteUsers.value) {
        toast.add({
            severity: 'error',
            summary: 'Access Denied',
            detail: 'You do not have permission to delete users',
            life: 3000
        });
        return;
    }

    user.value = userData;
    deleteuserDialog.value = true;
};

const deleteUser = async () => {
    if (!canDeleteUsers.value) {
        toast.add({
            severity: 'error',
            summary: 'Access Denied',
            detail: 'You do not have permission to delete users',
            life: 3000
        });
        return;
    }

    try {
        await UserService.deleteUser(user.value.id);

        toast.add({
            severity: 'success',
            summary: 'Success',
            detail: 'User deleted successfully',
            life: 3000
        });

        deleteuserDialog.value = false;
        user.value = {};
        await loadUsers();
    } catch (error) {
        console.error('Error deleting user:', error);
        toast.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to delete user',
            life: 3000
        });
    }
};

const exportCSV = () => {
    dt.value.exportCSV();
};

const confirmDeleteSelected = () => {
    if (!canDeleteUsers.value) {
        toast.add({
            severity: 'error',
            summary: 'Access Denied',
            detail: 'You do not have permission to delete users',
            life: 3000
        });
        return;
    }

    deleteUsersDialog.value = true;
};

const deleteSelectedUsers = async () => {
    if (!canDeleteUsers.value) {
        toast.add({
            severity: 'error',
            summary: 'Access Denied',
            detail: 'You do not have permission to delete users',
            life: 3000
        });
        return;
    }

    try {
        const deletePromises = selectedUsers.value.map(u => UserService.deleteUser(u.id));
        await Promise.all(deletePromises);

        toast.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Users deleted successfully',
            life: 3000
        });

        deleteUsersDialog.value = false;
        selectedUsers.value = null;
        await loadUsers();
    } catch (error) {
        console.error('Error deleting users:', error);
        toast.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to delete users',
            life: 3000
        });
    }
};

// 🔹 MANAGE ROLES AND PERMISSIONS
const openRolesPermissionsDialog = async (userData) => {
    if (!canManageRoles.value && !canManagePermissions.value) {
        toast.add({
            severity: 'error',
            summary: 'Access Denied',
            detail: 'You do not have permission to manage roles or permissions',
            life: 3000
        });
        return;
    }

    user.value = userData;
    loadingRolesPerms.value = true;

    try {
        // 🔹 Carica in parallelo: ruoli/permessi utente + liste disponibili
        const [rolesPermsData, roles, permissions] = await Promise.all([
            RoleService.getUserRolesAndPermissions(userData.id),
            RoleService.getAvailableRoles(),
            AuthService.getAllPermissions()
        ]);

        // Aggiorna le liste disponibili
        availableRoles.value = roles || [];
        availablePermissions.value = permissions || [];
        // Aggiorna ruoli/permessi dell'utente
        if (rolesPermsData) {
            currentUserRolesPerms.value = {
                roles: rolesPermsData.roles || [],
                permissions: rolesPermsData.permissions || []
            };
        }

        // Apri il dialog solo dopo aver caricato tutti i dati
        rolesPermissionsDialog.value = true;
    } catch (error) {
        console.error('Error loading roles/permissions:', error);
        toast.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to load roles and permissions',
            life: 3000
        });
    } finally {
        loadingRolesPerms.value = false;
    }
};

const addRole = async (roleName) => {
    if (!canManageRoles.value) {
        toast.add({
            severity: 'error',
            summary: 'Access Denied',
            detail: 'You do not have permission to assign roles',
            life: 3000
        });
        return;
    }

    try {
        await RoleService.assignRole(user.value.id, roleName);
        currentUserRolesPerms.value.roles.push(roleName);

        toast.add({
            severity: 'success',
            summary: 'Success',
            detail: `Role "${roleName}" assigned successfully`,
            life: 3000
        });

        await loadUsers(); // Refresh table
    } catch (error) {
        console.error('Error assigning role:', error);
        toast.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to assign role',
            life: 3000
        });
    }
};

const removeRole = async (roleName) => {
    if (!canManageRoles.value) {
        toast.add({
            severity: 'error',
            summary: 'Access Denied',
            detail: 'You do not have permission to remove roles',
            life: 3000
        });
        return;
    }

    try {
        await RoleService.removeRole(user.value.id, roleName);
        currentUserRolesPerms.value.roles = currentUserRolesPerms.value.roles.filter(r => r !== roleName);

        toast.add({
            severity: 'success',
            summary: 'Success',
            detail: `Role "${roleName}" removed successfully`,
            life: 3000
        });

        await loadUsers(); // Refresh table
    } catch (error) {
        console.error('Error removing role:', error);
        toast.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to remove role',
            life: 3000
        });
    }
};

const addPermission = async (permission) => {
    try {
        await RoleService.assignPermission(user.value.id, permission);
        currentUserRolesPerms.value.permissions.push(permission);

        toast.add({
            severity: 'success',
            summary: 'Success',
            detail: `Permission "${permission}" assigned successfully`,
            life: 3000
        });

        await loadUsers(); // Refresh table
    } catch (error) {
        console.error('Error assigning permission:', error);
        toast.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to assign permission',
            life: 3000
        });
    }
};

const removePermission = async (permission) => {
    if (!canManagePermissions.value) {
        toast.add({
            severity: 'error',
            summary: 'Access Denied',
            detail: 'You do not have permission to remove permissions',
            life: 3000
        });
        return;
    }

    try {
        await RoleService.removePermission(user.value.id, permission);
        currentUserRolesPerms.value.permissions = currentUserRolesPerms.value.permissions.filter(p => p !== permission);

        toast.add({
            severity: 'success',
            summary: 'Success',
            detail: `Permission "${permission}" removed successfully`,
            life: 3000
        });

        await loadUsers(); // Refresh table
    } catch (error) {
        console.error('Error removing permission:', error);
        toast.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to remove permission',
            life: 3000
        });
    }
};

const closeRolesPermissionsDialog = () => {
    rolesPermissionsDialog.value = false;
    user.value = {};
    currentUserRolesPerms.value = { roles: [], permissions: [] };
};
</script>

<template>
    <div>
        <div class="card">
            <Toolbar class="mb-6">
                <template #start>
                    <div class="flex flex-col sm:flex-row gap-2">
                        <Button v-permission="'Users.Create'" label="New" icon="pi pi-plus" severity="secondary"
                            class="w-full sm:w-auto" @click="openNew" />
                    </div>
                </template>

                <template #end>
                    <Button label="Export" icon="pi pi-upload" severity="secondary" @click="exportCSV($event)"
                        class="w-full sm:w-auto" />
                </template>
            </Toolbar>

            <DataTable ref="dt" v-model:selection="selectedUsers" :value="users" dataKey="id" :paginator="true"
                :rows="10" :filters="filters" :loading="loading" responsiveLayout="scroll" breakpoint="960px"
                paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
                :rowsPerPageOptions="[5, 10, 25]"
                currentPageReportTemplate="Showing {first} to {last} of {totalRecords} users">
                <!-- HEADER -->
                <template #header>
                    <div class="flex flex-col sm:flex-row gap-3 items-start sm:items-center justify-between">
                        <h4 class="m-0 text-lg">Utenti</h4>
                        <IconField class="w-full sm:w-auto">
                            <InputIcon>
                                <i class="pi pi-search" />
                            </InputIcon>
                            <InputText v-model="filters['global'].value" placeholder="Search..."
                                class="w-full sm:w-auto" />
                        </IconField>
                    </div>
                </template>

                <!-- MULTISELECT -->
                <Column selectionMode="multiple" headerStyle="width: 3rem" :exportable="false"
                    :class="'hidden md:table-cell'">
                </Column>

                <!-- ID -->
                <Column field="id" header="ID" sortable :style="{ minWidth: '80px' }" :class="'hidden lg:table-cell'">
                </Column>

                <!-- USERNAME -->
                <Column field="username" header="Username" sortable :style="{ minWidth: '150px' }"></Column>

                <!-- DESCRIPTION -->
                <Column field="description" header="Descrizione" sortable :style="{ minWidth: '200px' }"
                    :class="'hidden md:table-cell'">
                    <template #body="slotProps">
                        <span class="line-clamp-2">{{ slotProps.data.description }}</span>
                    </template>
                </Column>

                <!-- ADMINISTRATOR -->
                <Column field="isAdmin" header="Admin" sortable :style="{ minWidth: '100px' }">
                    <template #body="slotProps">
                        <Tag :value="slotProps.data.isAdmin ? 'Yes' : 'No'"
                            :severity="slotProps.data.isAdmin ? 'success' : 'secondary'" />
                    </template>
                </Column>

                <!-- ROLES -->
                <Column field="roles" header="Roles" :style="{ minWidth: '150px' }" :class="'hidden lg:table-cell'">
                    <template #body="slotProps">
                        <div class="flex gap-1 flex-wrap">
                            <Tag v-for="role in slotProps.data.roles" :key="role" :value="role" severity="info" />
                            <span v-if="!slotProps.data.roles || slotProps.data.roles.length === 0"
                                class="text-gray-400">No roles</span>
                        </div>
                    </template>
                </Column>

                <!-- LAST LOGIN -->
                <Column field="lastLoginDate" header="Ultimo Login" sortable :style="{ minWidth: '150px' }"
                    :class="'hidden md:table-cell'">
                    <template #body="slotProps">
                        {{ slotProps.data.lastLoginDate || 'Mai' }}
                    </template>
                </Column>

                <!-- ACTIONS -->
                <Column :exportable="false" :style="{ minWidth: '140px', width: '140px' }">
                    <template #body="slotProps">
                        <div class="flex gap-2">
                            <Button icon="pi pi-eye" outlined rounded size="small" @click="editUser(slotProps.data)"
                                v-tooltip.top="'View Details'" />

                            <Button v-permission="['Roles.Assign', 'Permissions.Assign']" icon="pi pi-shield" outlined
                                rounded severity="info" size="small" @click="openRolesPermissionsDialog(slotProps.data)"
                                v-tooltip.top="'Manage Roles & Permissions'" />

                            <Button v-permission="'Users.Delete'" icon="pi pi-trash" outlined rounded severity="danger"
                                size="small" @click="confirmDeleteUser(slotProps.data)" v-tooltip.top="'Delete'" />
                        </div>
                    </template>
                </Column>

                <template #empty>
                    <div class="text-center py-6">
                        <i class="pi pi-inbox text-4xl text-gray-400 mb-3"></i>
                        <p class="text-gray-500">No users found.</p>
                    </div>
                </template>
            </DataTable>
        </div>

        <Dialog v-model:visible="userDialog" :style="{ width: '450px' }"
            :header="user.id ? 'User Details (Read Only)' : 'Create New User'" :modal="true">
            <div class="flex flex-col gap-6">
                <!-- USERNAME -->
                <div>
                    <label for="username" class="block font-bold mb-3">Username</label>
                    <InputText id="username" v-model.trim="user.username" required autofocus
                        :invalid="submitted && !user.username" :disabled="!!user.id" fluid />
                    <small v-if="submitted && !user.username" class="text-red-500"> Username is required. </small>
                </div>

                <!-- DESCRIPTION -->
                <div>
                    <label for="description" class="block font-bold mb-3">Description</label>
                    <Textarea id="description" v-model="user.description" rows="3" cols="20" :disabled="!!user.id"
                        fluid />
                </div>

                <!-- EMAIL -->
                <div>
                    <label for="email" class="block font-bold mb-3">Email</label>
                    <InputText id="email" v-model.trim="user.email" type="email" :disabled="!!user.id" fluid />
                    <small v-if="submitted && !user.email && !user.id" class="text-red-500">Email is required.</small>
                </div>

                <!-- PASSWORD -->
                <div v-if="!user.id">
                    <label for="password" class="block font-bold mb-3">Password</label>
                    <Password id="password" v-model="user.password" toggleMask :feedback="false" fluid />
                    <small v-if="submitted && !user.id && !user.password" class="text-red-500">Password is required for
                        new
                        users.</small>
                </div>

                <!-- ADMINISTRATOR -->
                <div class="flex items-center gap-2">
                    <Checkbox inputId="admin" v-model="user.administrator" :binary="true" :disabled="!!user.id" />
                    <label for="admin" class="font-bold">Administrator</label>
                </div>

                <!-- LAST LOGIN (readonly) -->
                <div v-if="user.id">
                    <label for="lastLogin" class="block font-bold mb-3">Last Login</label>
                    <InputText id="lastLogin" :value="user.lastLoginDate || 'Never logged in'" readonly disabled
                        fluid />
                </div>
            </div>

            <template #footer>
                <Button label="Close" icon="pi pi-times" text @click="hideDialog" v-if="user.id" />
                <template v-else>
                    <Button label="Cancel" icon="pi pi-times" text @click="hideDialog" />
                    <Button label="Save" icon="pi pi-check" @click="saveUser" />
                </template>
            </template>
        </Dialog>

        <Dialog v-model:visible="deleteuserDialog" :style="{ width: '450px' }" header="Confirm" :modal="true">
            <div class="flex items-center gap-4">
                <i class="pi pi-exclamation-triangle !text-3xl" />
                <span v-if="user">Are you sure you want to delete user <b>{{ user.username }}</b>?</span>
            </div>
            <template #footer>
                <Button label="No" icon="pi pi-times" text @click="deleteuserDialog = false" />
                <Button label="Yes" icon="pi pi-check" @click="deleteUser" />
            </template>
        </Dialog>

        <Dialog v-model:visible="deleteUsersDialog" :style="{ width: '450px' }" header="Confirm" :modal="true">
            <div class="flex items-center gap-4">
                <i class="pi pi-exclamation-triangle !text-3xl" />
                <span v-if="user">Are you sure you want to delete the selected users?</span>
            </div>
            <template #footer>
                <Button label="No" icon="pi pi-times" text @click="deleteUsersDialog = false" />
                <Button label="Yes" icon="pi pi-check" text @click="deleteSelectedUsers" />
            </template>
        </Dialog>

        <!-- ROLES & PERMISSIONS DIALOG -->
        <Dialog v-model:visible="rolesPermissionsDialog" :style="{ width: '700px' }"
            :header="`Manage Roles & Permissions - ${user.username}`" :modal="true">

            <div v-if="loadingRolesPerms" class="flex justify-center items-center py-8">
                <ProgressSpinner style="width: 50px; height: 50px" strokeWidth="4" />
            </div>

            <div v-else class="flex flex-col gap-6">
                <!-- ROLES SECTION -->
                <div>
                    <h3 class="text-xl font-bold mb-3 flex items-center gap-2">
                        <i class="pi pi-users"></i>
                        Roles
                    </h3>

                    <!-- Current Roles -->
                    <div class="mb-3">
                        <label class="block font-semibold mb-2">Assigned Roles:</label>
                        <div
                            class="flex gap-2 flex-wrap p-3 border rounded-md bg-gray-50 dark:bg-gray-800 min-h-[50px]">
                            <Chip v-for="role in currentUserRolesPerms.roles" :key="role" :label="role" removable
                                @remove="removeRole(role)"
                                class="bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200" />
                            <span v-if="currentUserRolesPerms.roles.length === 0" class="text-gray-400">No roles
                                assigned</span>
                        </div>
                    </div>

                    <!-- Add Role -->
                    <div>
                        <label class="block font-semibold mb-2">Add Role:</label>
                        <Select :options="availableRoles.filter(r => !currentUserRolesPerms.roles.includes(r))"
                            placeholder="Select a role to add" @change="(e) => addRole(e.value)"
                            :disabled="availableRoles.filter(r => !currentUserRolesPerms.roles.includes(r)).length === 0"
                            fluid />
                    </div>
                </div>

                <Divider />

                <!-- PERMISSIONS SECTION -->
                <div>
                    <h3 class="text-xl font-bold mb-3 flex items-center gap-2">
                        <i class="pi pi-lock"></i>
                        Permissions
                    </h3>

                    <!-- Current Permissions -->
                    <div class="mb-3">
                        <label class="block font-semibold mb-2">Assigned Permissions:</label>
                        <div
                            class="flex gap-2 flex-wrap p-3 border rounded-md bg-gray-50 dark:bg-gray-800 min-h-[100px] max-h-[300px] overflow-y-auto">
                            <Chip v-for="perm in currentUserRolesPerms.permissions" :key="perm" :label="perm" removable
                                @remove="removePermission(perm)"
                                class="bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200" />
                            <span v-if="currentUserRolesPerms.permissions.length === 0" class="text-gray-400">No
                                permissions
                                assigned</span>
                        </div>
                    </div>

                    <!-- Add Permission -->
                    <div>
                        <label class="block font-semibold mb-2">Add Permission:</label>
                        <Select
                            :options="availablePermissions.filter(p => !currentUserRolesPerms.permissions.includes(p))"
                            placeholder="Select a permission to add" @change="(e) => addPermission(e.value)"
                            :disabled="availablePermissions.filter(p => !currentUserRolesPerms.permissions.includes(p)).length === 0"
                            fluid />
                    </div>
                </div>
            </div>

            <template #footer>
                <Button label="Close" icon="pi pi-times" @click="closeRolesPermissionsDialog" />
            </template>
        </Dialog>
    </div>
</template>

<style scoped>
/* Responsive utilities */
.line-clamp-2 {
    display: -webkit-box;
    -webkit-line-clamp: 2;
    line-clamp: 2;
    -webkit-box-orient: vertical;
    overflow: hidden;
    text-overflow: ellipsis;
}

/* Mobile optimization */
@media (max-width: 640px) {
    :deep(.p-datatable-table) {
        font-size: 0.875rem;
    }

    :deep(.p-datatable-header-cell),
    :deep(.p-datatable-tbody > tr > td) {
        padding: 0.5rem;
    }

    :deep(.p-toolbar) {
        flex-direction: column;
        gap: 1rem;
    }

    :deep(.p-paginator) {
        flex-wrap: wrap;
        gap: 0.5rem;
    }
}

/* Tablet optimization */
@media (min-width: 641px) and (max-width: 1024px) {
    :deep(.p-datatable-table) {
        font-size: 0.9375rem;
    }
}

/* Column visibility breakpoints */
@media (max-width: 768px) {
    :deep(.hidden.md\\:table-cell) {
        display: none !important;
    }
}

@media (max-width: 1024px) {
    :deep(.hidden.lg\\:table-cell) {
        display: none !important;
    }
}

@media (max-width: 1280px) {
    :deep(.hidden.xl\\:table-cell) {
        display: none !important;
    }
}

@media (min-width: 769px) {
    :deep(.hidden.md\\:table-cell) {
        display: table-cell !important;
    }
}

@media (min-width: 1025px) {
    :deep(.hidden.lg\\:table-cell) {
        display: table-cell !important;
    }
}

@media (min-width: 1281px) {
    :deep(.hidden.xl\\:table-cell) {
        display: table-cell !important;
    }
}
</style>
