<script setup>
import { AuthService } from '@/service/AuthService';
import { RoleService } from '@/service/RoleService';
import { UserService } from '@/service/UserService';
import { FilterMatchMode } from '@primevue/core/api';
import { useToast } from 'primevue/usetoast';
import { computed, onMounted, ref } from 'vue';

const toast = useToast();
const users = ref([]);
const loading = ref(false);
const filters = ref({
    global: { value: null, matchMode: FilterMatchMode.CONTAINS }
});

// Available roles and permissions
const availableRoles = ref([]);
const availablePermissions = ref([]);

// Statistics
const statistics = computed(() => ({
    totalUsers: users.value.length,
    usersWithRoles: users.value.filter(u => u.roles && u.roles.length > 0).length,
    usersWithPermissions: users.value.filter(u => u.permissions && u.permissions.length > 0).length,
    adminUsers: users.value.filter(u => u.isAdmin).length
}));

// Role statistics
const roleStats = computed(() => {
    const stats = {};
    users.value.forEach(user => {
        if (user.roles) {
            user.roles.forEach(role => {
                if (!stats[role]) {
                    stats[role] = { role, count: 0, users: [] };
                }
                stats[role].count++;
                stats[role].users.push(user.username);
            });
        }
    });
    return Object.values(stats).sort((a, b) => b.count - a.count);
});

// Permission statistics
const permissionStats = computed(() => {
    const stats = {};
    users.value.forEach(user => {
        if (user.permissions) {
            user.permissions.forEach(perm => {
                if (!stats[perm]) {
                    stats[perm] = { permission: perm, count: 0, users: [] };
                }
                stats[perm].count++;
                stats[perm].users.push(user.username);
            });
        }
    });
    return Object.values(stats).sort((a, b) => b.count - a.count);
});

onMounted(async () => {
    await loadData();
});

const loadData = async () => {
    try {
        loading.value = true;
        const allUsers = await UserService.getUsers();

        // Load roles and permissions for all users in parallel
        const usersWithRolesAndPerms = await Promise.all(
            allUsers.map(async (user) => {
                try {
                    const rolesAndPerms = await RoleService.getUserRolesAndPermissions(user.id);
                    return {
                        ...user,
                        roles: rolesAndPerms?.roles || [],
                        permissions: rolesAndPerms?.permissions || []
                    };
                } catch (error) {
                    console.warn(`Failed to load roles/perms for user ${user.id}:`, error);
                    return user;
                }
            })
        );

        users.value = usersWithRolesAndPerms;
        availableRoles.value = await RoleService.getAvailableRoles();
        availablePermissions.value = await AuthService.getAllPermissions();
    } catch (error) {
        console.error('Error loading data:', error);
        toast.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to load users data',
            life: 3000
        });
    } finally {
        loading.value = false;
    }
};

// Bulk role assignment
const bulkRoleDialog = ref(false);
const selectedUsersForBulk = ref([]);
const selectedRole = ref(null);

const openBulkRoleDialog = async () => {
    selectedUsersForBulk.value = [];
    selectedRole.value = null;
    bulkRoleDialog.value = true;

    // 🔹 Ricarica i ruoli disponibili quando si apre il dialog
    try {
        availableRoles.value = await RoleService.getAvailableRoles();
    } catch (error) {
        console.error('Error refreshing available roles:', error);
    }
};

const assignBulkRole = async () => {
    if (!selectedRole.value || selectedUsersForBulk.value.length === 0) {
        toast.add({
            severity: 'warn',
            summary: 'Warning',
            detail: 'Please select a role and at least one user',
            life: 3000
        });
        return;
    }

    try {
        loading.value = true;
        const promises = selectedUsersForBulk.value.map(userId =>
            RoleService.assignRole(userId, selectedRole.value)
        );
        await Promise.all(promises);

        toast.add({
            severity: 'success',
            summary: 'Success',
            detail: `Role "${selectedRole.value}" assigned to ${selectedUsersForBulk.value.length} users`,
            life: 3000
        });

        bulkRoleDialog.value = false;
        await loadData();
    } catch (error) {
        console.error('Error assigning bulk role:', error);
        toast.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to assign role to some users',
            life: 3000
        });
    } finally {
        loading.value = false;
    }
};

// Bulk permission assignment
const bulkPermissionDialog = ref(false);
const selectedUsersForPermission = ref([]);
const selectedPermission = ref(null);

const openBulkPermissionDialog = async () => {
    selectedUsersForPermission.value = [];
    selectedPermission.value = null;
    bulkPermissionDialog.value = true;

    // 🔹 Ricarica i permessi disponibili quando si apre il dialog
    try {
        availablePermissions.value = await AuthService.getAllPermissions();
    } catch (error) {
        console.error('Error refreshing available permissions:', error);
    }
};

const assignBulkPermission = async () => {
    if (!selectedPermission.value || selectedUsersForPermission.value.length === 0) {
        toast.add({
            severity: 'warn',
            summary: 'Warning',
            detail: 'Please select a permission and at least one user',
            life: 3000
        });
        return;
    }

    try {
        loading.value = true;
        const promises = selectedUsersForPermission.value.map(userId =>
            RoleService.assignPermission(userId, selectedPermission.value)
        );
        await Promise.all(promises);

        toast.add({
            severity: 'success',
            summary: 'Success',
            detail: `Permission "${selectedPermission.value}" assigned to ${selectedUsersForPermission.value.length} users`,
            life: 3000
        });

        bulkPermissionDialog.value = false;
        await loadData();
    } catch (error) {
        console.error('Error assigning bulk permission:', error);
        toast.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to assign permission to some users',
            life: 3000
        });
    } finally {
        loading.value = false;
    }
};
</script>

<template>
    <div class="grid grid-cols-12 gap-8">
        <!-- STATISTICS CARDS -->
        <div class="col-span-12">
            <div class="grid grid-cols-12 gap-6">
                <div class="col-span-12 md:col-span-6 lg:col-span-3">
                    <div class="card mb-0">
                        <div class="flex justify-between mb-3">
                            <div>
                                <span class="block text-muted-color font-medium mb-3">Total Users</span>
                                <div class="text-primary-500 font-medium text-xl">{{ statistics.totalUsers }}</div>
                            </div>
                            <div class="flex items-center justify-center bg-blue-100 dark:bg-blue-400/10 rounded-full"
                                style="width: 2.5rem; height: 2.5rem">
                                <i class="pi pi-users text-blue-500 !text-xl"></i>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="col-span-12 md:col-span-6 lg:col-span-3">
                    <div class="card mb-0">
                        <div class="flex justify-between mb-3">
                            <div>
                                <span class="block text-muted-color font-medium mb-3">Users with Roles</span>
                                <div class="text-blue-500 font-medium text-xl">{{ statistics.usersWithRoles }}</div>
                            </div>
                            <div class="flex items-center justify-center bg-blue-100 dark:bg-blue-400/10 rounded-full"
                                style="width: 2.5rem; height: 2.5rem">
                                <i class="pi pi-shield text-blue-500 !text-xl"></i>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="col-span-12 md:col-span-6 lg:col-span-3">
                    <div class="card mb-0">
                        <div class="flex justify-between mb-3">
                            <div>
                                <span class="block text-muted-color font-medium mb-3">Users with Permissions</span>
                                <div class="text-green-500 font-medium text-xl">{{ statistics.usersWithPermissions }}
                                </div>
                            </div>
                            <div class="flex items-center justify-center bg-green-100 dark:bg-green-400/10 rounded-full"
                                style="width: 2.5rem; height: 2.5rem">
                                <i class="pi pi-lock text-green-500 !text-xl"></i>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="col-span-12 md:col-span-6 lg:col-span-3">
                    <div class="card mb-0">
                        <div class="flex justify-between mb-3">
                            <div>
                                <span class="block text-muted-color font-medium mb-3">Admin Users</span>
                                <div class="text-orange-500 font-medium text-xl">{{ statistics.adminUsers }}</div>
                            </div>
                            <div class="flex items-center justify-center bg-orange-100 dark:bg-orange-400/10 rounded-full"
                                style="width: 2.5rem; height: 2.5rem">
                                <i class="pi pi-star text-orange-500 !text-xl"></i>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- ROLES & PERMISSIONS OVERVIEW -->
        <div class="col-span-12 lg:col-span-6">
            <div class="card">
                <div class="flex justify-between items-center mb-4">
                    <h5 class="m-0">Roles Distribution</h5>
                    <Button label="Bulk Assign Role" icon="pi pi-users" @click="openBulkRoleDialog" severity="info"
                        size="small" />
                </div>

                <DataTable :value="roleStats" :loading="loading" responsiveLayout="scroll" breakpoint="960px">
                    <Column field="role" header="Role" sortable :style="{ minWidth: '150px' }">
                        <template #body="slotProps">
                            <Tag :value="slotProps.data.role" severity="info" />
                        </template>
                    </Column>
                    <Column field="count" header="Users Count" sortable :style="{ width: '120px' }">
                        <template #body="slotProps">
                            <Badge :value="slotProps.data.count" severity="info" />
                        </template>
                    </Column>
                    <Column field="users" header="Users" :style="{ minWidth: '200px' }" :class="'hidden md:table-cell'">
                        <template #body="slotProps">
                            <ScrollPanel style="width: 100%; height: 60px">
                                <div class="flex gap-1 flex-wrap">
                                    <Chip v-for="username in slotProps.data.users" :key="username" :label="username"
                                        class="text-xs" />
                                </div>
                            </ScrollPanel>
                        </template>
                    </Column>
                    <template #empty>
                        <div class="text-center py-4">
                            <p class="text-gray-500">No role data available.</p>
                        </div>
                    </template>
                </DataTable>
            </div>
        </div>

        <div class="col-span-12 lg:col-span-6">
            <div class="card">
                <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-3 mb-4">
                    <h5 class="m-0">Permissions Distribution</h5>
                    <Button label="Bulk Assign Permission" icon="pi pi-lock" @click="openBulkPermissionDialog"
                        severity="success" size="small" class="w-full sm:w-auto" />
                </div>

                <DataTable :value="permissionStats" :loading="loading" responsiveLayout="scroll" :paginator="true"
                    :rows="5" breakpoint="960px">
                    <Column field="permission" header="Permission" sortable :style="{ minWidth: '180px' }">
                        <template #body="slotProps">
                            <Tag :value="slotProps.data.permission" severity="success" />
                        </template>
                    </Column>
                    <Column field="count" header="Users Count" sortable :style="{ width: '120px' }">
                        <template #body="slotProps">
                            <Badge :value="slotProps.data.count" severity="success" />
                        </template>
                    </Column>
                    <Column field="users" header="Users" :style="{ minWidth: '200px' }" :class="'hidden lg:table-cell'">
                        <template #body="slotProps">
                            <ScrollPanel style="width: 100%; height: 60px">
                                <div class="flex gap-1 flex-wrap">
                                    <Chip v-for="username in slotProps.data.users" :key="username" :label="username"
                                        class="text-xs" />
                                </div>
                            </ScrollPanel>
                        </template>
                    </Column>
                    <template #empty>
                        <div class="text-center py-4">
                            <p class="text-gray-500">No permission data available.</p>
                        </div>
                    </template>
                </DataTable>
            </div>
        </div>

        <!-- USER TABLE WITH ROLES & PERMISSIONS -->
        <div class="col-span-12">
            <div class="card">
                <h5>Users - Roles & Permissions Details</h5>

                <DataTable :value="users" dataKey="id" :paginator="true" :rows="10" :filters="filters"
                    :loading="loading" responsiveLayout="scroll" breakpoint="960px"
                    paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
                    :rowsPerPageOptions="[5, 10, 25]"
                    currentPageReportTemplate="Showing {first} to {last} of {totalRecords} users">

                    <template #header>
                        <div class="flex flex-col sm:flex-row gap-3 items-start sm:items-center justify-between">
                            <h4 class="m-0 text-lg">All Users</h4>
                            <IconField class="w-full sm:w-auto">
                                <InputIcon>
                                    <i class="pi pi-search" />
                                </InputIcon>
                                <InputText v-model="filters['global'].value" placeholder="Search..."
                                    class="w-full sm:w-auto" />
                            </IconField>
                        </div>
                    </template>

                    <Column field="username" header="Username" sortable :style="{ minWidth: '150px' }"></Column>

                    <Column field="isAdmin" header="Admin" sortable :style="{ minWidth: '100px' }"
                        :class="'hidden md:table-cell'">
                        <template #body="slotProps">
                            <Tag :value="slotProps.data.isAdmin ? 'Yes' : 'No'"
                                :severity="slotProps.data.isAdmin ? 'success' : 'secondary'" />
                        </template>
                    </Column>

                    <Column field="roles" header="Roles" :style="{ minWidth: '180px' }">
                        <template #body="slotProps">
                            <div class="flex gap-1 flex-wrap">
                                <Tag v-for="role in slotProps.data.roles" :key="role" :value="role" severity="info" />
                                <span v-if="!slotProps.data.roles || slotProps.data.roles.length === 0"
                                    class="text-gray-400">No roles</span>
                            </div>
                        </template>
                    </Column>

                    <Column field="permissions" header="Permissions" :style="{ minWidth: '200px' }"
                        :class="'hidden lg:table-cell'">
                        <template #body="slotProps">
                            <ScrollPanel style="width: 100%; height: 60px">
                                <div class="flex gap-1 flex-wrap">
                                    <Tag v-for="perm in slotProps.data.permissions" :key="perm" :value="perm"
                                        severity="success" rounded />
                                    <span v-if="!slotProps.data.permissions || slotProps.data.permissions.length === 0"
                                        class="text-gray-400">No permissions</span>
                                </div>
                            </ScrollPanel>
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
        </div>

        <!-- BULK ROLE ASSIGNMENT DIALOG -->
        <Dialog v-model:visible="bulkRoleDialog" :style="{ width: '500px' }" header="Bulk Assign Role" :modal="true">
            <div class="flex flex-col gap-4">
                <div>
                    <label class="block font-bold mb-2">Select Role:</label>
                    <Select v-model="selectedRole" :options="availableRoles" placeholder="Choose a role" fluid
                        showClear />
                </div>

                <div>
                    <label class="block font-bold mb-2">Select Users:</label>
                    <MultiSelect v-model="selectedUsersForBulk"
                        :options="users.map(u => ({ label: u.username, value: u.id }))" optionLabel="label"
                        optionValue="value" placeholder="Select users" display="chip" fluid />
                </div>
            </div>

            <template #footer>
                <Button label="Cancel" icon="pi pi-times" text @click="bulkRoleDialog = false" />
                <Button label="Assign" icon="pi pi-check" @click="assignBulkRole" />
            </template>
        </Dialog>

        <!-- BULK PERMISSION ASSIGNMENT DIALOG -->
        <Dialog v-model:visible="bulkPermissionDialog" :style="{ width: '500px' }" header="Bulk Assign Permission"
            :modal="true">
            <div class="flex flex-col gap-4">
                <div>
                    <label class="block font-bold mb-2">Select Permission:</label>
                    <Select v-model="selectedPermission" :options="availablePermissions"
                        placeholder="Choose a permission" fluid showClear />
                </div>

                <div>
                    <label class="block font-bold mb-2">Select Users:</label>
                    <MultiSelect v-model="selectedUsersForPermission"
                        :options="users.map(u => ({ label: u.username, value: u.id }))" optionLabel="label"
                        optionValue="value" placeholder="Select users" display="chip" fluid />
                </div>
            </div>

            <template #footer>
                <Button label="Cancel" icon="pi pi-times" text @click="bulkPermissionDialog = false" />
                <Button label="Assign" icon="pi pi-check" @click="assignBulkPermission" />
            </template>
        </Dialog>
    </div>
</template>

<style scoped>
@media (max-width: 640px) {
    :deep(.p-datatable-table) {
        font-size: 0.875rem;
    }

    :deep(.p-datatable-header-cell),
    :deep(.p-datatable-tbody > tr > td) {
        padding: 0.5rem;
    }
}

@media (min-width: 641px) and (max-width: 1024px) {
    :deep(.p-datatable-table) {
        font-size: 0.9375rem;
    }
}

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
</style>
