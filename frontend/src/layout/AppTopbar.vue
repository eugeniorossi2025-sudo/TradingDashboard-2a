<script setup>
import { useLayout } from '@/layout/composables/layout';
import { AuthService } from '@/service/AuthService';
import { useToast } from 'primevue/usetoast';
import { useRouter } from 'vue-router';
import AppConfigurator from './AppConfigurator.vue';

const { toggleMenu, toggleDarkMode, isDarkTheme } = useLayout();
const router = useRouter();
const toast = useToast();

// 🔹 LOGOUT HANDLER
const handleLogout = async () => {
    try {
        await AuthService.logout();

        toast.add({
            severity: 'success',
            summary: 'Logout',
            detail: 'Logged out successfully',
            life: 2000
        });

        // Redirect to login after short delay
        setTimeout(() => {
            router.push('/auth/login');
        }, 500);
    } catch (error) {
        console.error('Logout error:', error);
        toast.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Logout failed',
            life: 3000
        });
    }
};
</script>

<template>
    <div class="layout-topbar">
        <div class="layout-topbar-logo-container">
            <button class="layout-menu-button layout-topbar-action" @click="toggleMenu">
                <i class="pi pi-bars"></i>
            </button>
            <router-link to="/" class="layout-topbar-logo">
                <img src="/demo/images/logo.png" alt="Logo Eugenio" />
            </router-link>
        </div>

        <div class="layout-topbar-actions">
            <div class="layout-config-menu">
                <button type="button" class="layout-topbar-action" @click="toggleDarkMode">
                    <i :class="['pi', { 'pi-moon': isDarkTheme, 'pi-sun': !isDarkTheme }]"></i>
                </button>
                <div class="relative">
                    <button
                        v-styleclass="{ selector: '@next', enterFromClass: 'hidden', enterActiveClass: 'animate-scalein', leaveToClass: 'hidden', leaveActiveClass: 'animate-fadeout', hideOnOutsideClick: true }"
                        type="button" class="layout-topbar-action layout-topbar-action-highlight">
                        <i class="pi pi-palette"></i>
                    </button>
                    <AppConfigurator />
                </div>
            </div>

            <button class="layout-topbar-menu-button layout-topbar-action"
                v-styleclass="{ selector: '@next', enterFromClass: 'hidden', enterActiveClass: 'animate-scalein', leaveToClass: 'hidden', leaveActiveClass: 'animate-fadeout', hideOnOutsideClick: true }">
                <i class="pi pi-ellipsis-v"></i>
            </button>

            <div class="layout-topbar-menu hidden lg:block">
                <div class="layout-topbar-menu-content">
                    <button type="button" class="layout-topbar-action" @click="handleLogout">
                        <i class="pi pi-sign-out"></i>
                        <span>LogOut</span>
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>
