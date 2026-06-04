<script setup>
import { AuthService } from '@/service/AuthService';
import { computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';

const props = defineProps({
    /** When set, shows Sync and calls this handler (live / owner refresh). */
    onSync: { type: Function, default: null }
});

const router = useRouter();
const route = useRoute();
const showOwner = computed(() => AuthService.isRootOwner.value);
const onOwnerPage = computed(() => route.name === 'admin-root-owner');

async function logout() {
    await AuthService.logout();
    router.push('/auth/login');
}

function goLive() {
    router.push('/admin/mobile-live');
}

function goReports() {
    router.push('/admin/mobile-reports');
}

function goOwner() {
    router.push('/admin/root-owner');
}
</script>

<template>
    <div class="actions">
        <button v-if="onSync" type="button" class="link-btn" @click="onSync">Sync</button>
        <button type="button" class="link-btn" @click="goReports">Report finanziari</button>
        <button
            v-if="showOwner && !onOwnerPage"
            type="button"
            class="link-btn owner-link"
            @click="goOwner"
        >
            Owner
        </button>
        <button v-if="showOwner && onOwnerPage" type="button" class="link-btn" @click="goLive">Live</button>
        <button type="button" class="logout-btn" @click="logout">Logout</button>
    </div>
</template>

<style scoped>
.actions {
    display: flex;
    gap: 8px;
    flex-wrap: wrap;
}
.link-btn,
.logout-btn {
    min-height: 34px;
    padding: 8px 11px;
    border-radius: 999px;
    border: 1px solid var(--surface-border);
    background: color-mix(in srgb, var(--surface-card) 80%, transparent);
    color: var(--text-color);
    font: inherit;
    font-size: 12px;
    cursor: pointer;
}
.owner-link {
    border-color: color-mix(in srgb, var(--primary-color) 55%, var(--surface-border));
    background: color-mix(in srgb, var(--primary-color) 18%, var(--surface-card));
    color: var(--primary-color);
    font-weight: 700;
    letter-spacing: 0.04em;
}
.logout-btn {
    color: #fda4af;
    border-color: color-mix(in srgb, #fb7185 35%, var(--surface-border));
}
</style>
