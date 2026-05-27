<script setup>
import { useAuth } from '@/composables/useAuth';
import { computed } from 'vue';
import AppMenuItem from './AppMenuItem.vue';

const { isAdmin, canViewUsers, canManageRoles, canViewLogs, canManageDevices, canExecuteCommands, canViewConfigurations } = useAuth();

// 🔹 BASE MENU ITEMS (VISIBLE TO ALL AUTHENTICATED USERS)
const baseMenuItems = [
    {
        label: 'Dashboard',
        items: [
            { label: 'Dashboard Live', icon: 'pi pi-fw pi-home', to: '/' }
        ]
    }
];

// 🔹 ADMIN & MANAGEMENT MENU ITEMS (BASED ON PERMISSIONS)
const managementMenuItems = computed(() => {
    const items = [];

    // Admin section - visible if user has any admin permission
    if (isAdmin.value || canViewUsers.value || canManageRoles.value || canViewConfigurations.value || canManageDevices.value) {
        const adminItems = [];

        if (isAdmin.value || canViewUsers.value) {
            adminItems.push({ label: 'Utenti / Email', icon: 'pi pi-fw pi-envelope', to: '/pages/user' });
        }

        if (isAdmin.value || canManageRoles.value) {
            adminItems.push({ label: 'Roles & Permissions', icon: 'pi pi-fw pi-shield', to: '/pages/roles-permissions' });
        }

        if (isAdmin.value || canViewConfigurations.value) {
            adminItems.push({ label: 'Configurazioni DASH', icon: 'pi pi-fw pi-cog', to: '/pages/configurations' });
        }

        if (isAdmin.value || canManageDevices.value) {
            adminItems.push({ label: 'PC Managment', icon: 'pi pi-fw pi-mobile', to: '/pages/pc-configuration', class: 'rotated-icon' });
        }

        if (adminItems.length > 0) {
            items.push({
                label: 'Admin',
                icon: 'pi pi-fw pi-briefcase',
                items: adminItems
            });
        }
    }

    // Other Data section - visible if user has logs or console permissions
    if (canViewLogs.value || canExecuteCommands.value) {
        const dataItems = [];

        if (canViewLogs.value) {
            dataItems.push({ label: 'Report e Log', icon: 'pi pi-fw pi-file', to: '/pages/log' });
        }

        if (dataItems.length > 0) {
            items.push({
                label: 'Operatività',
                icon: 'pi pi-fw pi-briefcase',
                items: dataItems
            });
        }
    }

    return items;
});

// 🔹 DYNAMIC MENU BASED ON USER PERMISSIONS
const model = computed(() => {
    return [...baseMenuItems, ...managementMenuItems.value];
});
</script>

<template>
    <ul class="layout-menu">
        <template v-for="(item, i) in model" :key="item">
            <app-menu-item v-if="!item.separator" :item="item" :index="i"></app-menu-item>
            <li v-if="item.separator" class="menu-separator"></li>
        </template>
    </ul>
</template>

<style lang="scss" scoped></style>
