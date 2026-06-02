<script setup>
import { PushNotificationService } from '@/service/PushNotificationService';
import { computed, onMounted, ref } from 'vue';

const props = defineProps({
    title: { type: String, default: 'Notifiche push' },
    description: {
        type: String,
        default: 'Avvisi missione e test immediato, se browser e backend li supportano.'
    },
    returnPath: { type: String, required: true }
});

const pushLoading = ref(false);
const testLoading = ref(false);
const testMessage = ref('');
const pushStatus = ref({
    supported: false,
    permission: 'unsupported',
    configured: false,
    subscribed: false,
    uiState: 'permission-default',
    edgeAndroid: false,
    invalidEndpoint: false,
    message: 'Verifica notifiche in corso.',
    stateLabel: 'Verifica in corso',
    guidance: ''
});

const canRequestPermission = computed(
    () =>
        pushStatus.value.supported &&
        pushStatus.value.uiState !== 'unsupported' &&
        pushStatus.value.uiState !== 'permission-denied' &&
        pushStatus.value.uiState !== 'edge-android' &&
        !pushStatus.value.subscribed
);

const canSendTest = computed(() => pushStatus.value.subscribed && pushStatus.value.configured);

const stateClass = computed(() => {
    if (pushStatus.value.subscribed) return 'active';
    if (pushStatus.value.uiState === 'permission-denied' || pushStatus.value.uiState === 'invalid-subscription') return 'blocked';
    if (pushStatus.value.uiState === 'edge-android') return 'warn';
    if (pushStatus.value.uiState === 'permission-default' || pushStatus.value.uiState === 'ready-to-subscribe') return 'pending';
    return 'warn';
});

async function refreshStatus() {
    pushLoading.value = true;
    testMessage.value = '';
    try {
        pushStatus.value = await PushNotificationService.getStatus();
    } finally {
        pushLoading.value = false;
    }
}

async function enablePush() {
    pushLoading.value = true;
    testMessage.value = '';
    try {
        pushStatus.value = await PushNotificationService.subscribe();
    } catch (err) {
        console.warn('Push subscription failed:', err);
        const permission = typeof Notification !== 'undefined' ? Notification.permission : 'unsupported';
        pushStatus.value = {
            ...(await PushNotificationService.getStatus()),
            message: 'Notifiche push non attivabili: endpoint backend non disponibile.',
            permission
        };
    } finally {
        pushLoading.value = false;
    }
}

async function sendTest() {
    testLoading.value = true;
    testMessage.value = '';
    try {
        const result = await PushNotificationService.sendTest(props.returnPath);
        testMessage.value = result.message;
    } catch (err) {
        console.warn('Push test failed:', err);
        testMessage.value = 'Invio prova fallito. Verifica login e subscription attiva.';
    } finally {
        testLoading.value = false;
    }
}

onMounted(refreshStatus);
</script>

<template>
    <section class="panel section push-panel">
        <div class="section-head">
            <div>
                <div class="section-title">{{ title }}</div>
                <div class="section-copy">{{ description }}</div>
            </div>
            <div class="mini-label">{{ pushStatus.subscribed ? 'ON' : 'OFF' }}</div>
        </div>
        <div class="push-box">
            <div class="push-state" :class="stateClass">{{ pushStatus.stateLabel }}</div>
            <div class="push-message">{{ pushStatus.message }}</div>
            <div v-if="pushStatus.guidance" class="push-guidance">{{ pushStatus.guidance }}</div>
            <div v-if="pushStatus.edgeAndroid" class="push-alert edge">
                Edge Android non è consigliato per le notifiche push. Utilizzare Chrome Android.
            </div>
            <div v-if="testMessage" class="push-test-result">{{ testMessage }}</div>
            <div class="push-actions">
                <button type="button" class="push-button" :disabled="pushLoading || !canRequestPermission" @click="enablePush">
                    {{ pushLoading ? 'Verifica...' : 'Consenti notifiche' }}
                </button>
                <button type="button" class="push-button secondary" :disabled="testLoading || !canSendTest" @click="sendTest">
                    {{ testLoading ? 'Invio...' : 'Invia notifica di prova' }}
                </button>
                <button type="button" class="push-button ghost" :disabled="pushLoading" @click="refreshStatus">Aggiorna stato</button>
            </div>
        </div>
    </section>
</template>

<style scoped>
.section {
    padding: 18px;
}
.section-head {
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    gap: 14px;
}
.section-title {
    font-size: 16px;
    font-weight: 800;
    letter-spacing: -0.03em;
}
.section-copy,
.push-message,
.push-guidance,
.push-test-result {
    font-size: 12px;
    color: var(--text-color-secondary);
    line-height: 1.45;
}
.mini-label {
    font-size: 11px;
    letter-spacing: 0.16em;
    text-transform: uppercase;
    color: var(--text-color-secondary);
}
.push-box {
    margin-top: 12px;
    padding: 13px;
    border-radius: 18px;
    background: rgba(255, 255, 255, 0.04);
    border: 1px solid rgba(255, 255, 255, 0.06);
}
.push-state {
    display: inline-flex;
    padding: 7px 10px;
    border-radius: 999px;
    font-size: 11px;
    font-weight: 800;
    letter-spacing: 0.08em;
    text-transform: uppercase;
}
.push-state.active {
    background: rgba(34, 197, 94, 0.12);
    color: #22c55e;
}
.push-state.pending {
    background: rgba(245, 158, 11, 0.12);
    color: #f59e0b;
}
.push-state.blocked {
    background: rgba(239, 68, 68, 0.12);
    color: #ef4444;
}
.push-state.warn {
    background: rgba(245, 158, 11, 0.12);
    color: #f59e0b;
}
.push-message {
    margin-top: 10px;
}
.push-guidance {
    margin-top: 8px;
    padding: 10px 12px;
    border-radius: 14px;
    background: rgba(255, 255, 255, 0.03);
    border: 1px solid var(--surface-border);
}
.push-alert.edge {
    margin-top: 10px;
    padding: 10px 12px;
    border-radius: 14px;
    border: 1px solid rgba(239, 68, 68, 0.35);
    background: rgba(239, 68, 68, 0.08);
    color: #fecaca;
    font-size: 12px;
    line-height: 1.45;
}
.push-test-result {
    margin-top: 10px;
    color: var(--primary-color);
    font-weight: 700;
}
.push-actions {
    display: flex;
    flex-direction: column;
    gap: 8px;
    margin-top: 12px;
}
.push-button {
    min-height: 38px;
    padding: 8px 12px;
    border-radius: 999px;
    border: 1px solid color-mix(in srgb, var(--primary-color) 34%, transparent);
    background: color-mix(in srgb, var(--primary-color) 14%, transparent);
    color: var(--primary-color);
    font: inherit;
    font-size: 11px;
    font-weight: 800;
    letter-spacing: 0.06em;
    text-transform: uppercase;
    cursor: pointer;
}
.push-button.secondary {
    border-color: color-mix(in srgb, #22c55e 40%, transparent);
    background: color-mix(in srgb, #22c55e 12%, transparent);
    color: #22c55e;
}
.push-button.ghost {
    border-color: var(--surface-border);
    background: transparent;
    color: var(--text-color-secondary);
    text-transform: none;
    letter-spacing: 0;
    font-weight: 600;
}
.push-button:disabled {
    opacity: 0.55;
    cursor: not-allowed;
}
.panel {
    border-radius: 30px;
    background: linear-gradient(180deg, rgba(255, 255, 255, 0.05), rgba(255, 255, 255, 0.015)), var(--surface-card);
    border: 1px solid var(--surface-border);
    box-shadow: 0 30px 80px rgba(0, 0, 0, 0.22);
}
</style>
