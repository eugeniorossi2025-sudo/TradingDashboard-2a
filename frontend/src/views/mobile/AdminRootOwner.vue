<script setup>
import MobileAdminQuickNav from '@/components/mobile/MobileAdminQuickNav.vue';
import { RootOwnerService } from '@/service/RootOwnerService';
import { formatRomeDateTime } from '@/utils/romeTime';
import { useToast } from 'primevue/usetoast';
import { onMounted, ref } from 'vue';

const toast = useToast();
const loading = ref(true);
const busy = ref(false);
const status = ref(null);
const commandReason = ref('');

function readApiError(error, fallback = 'Operazione non riuscita') {
    return error?.response?.data?.code || error?.response?.data?.message || error?.code || error?.message || fallback;
}

async function refresh() {
    loading.value = true;
    try {
        status.value = await RootOwnerService.getStatus();
    } catch (error) {
        const code = error?.response?.data?.code;
        if (code === 'ROOT_OWNER_ONLY' || error?.response?.status === 403) {
            toast.add({ severity: 'error', summary: '403 ROOT_OWNER_ONLY', detail: 'Accesso riservato al proprietario.', life: 8000 });
        } else {
            toast.add({ severity: 'error', summary: 'Errore', detail: readApiError(error), life: 6000 });
        }
    } finally {
        loading.value = false;
    }
}

async function run(action, fn) {
    if (busy.value) return;
    const confirmed = window.confirm(`Confermi: ${action}?`);
    if (!confirmed) return;

    busy.value = true;
    try {
        await fn(commandReason.value || action);
        toast.add({ severity: 'success', summary: action, detail: 'Comando eseguito', life: 3500 });
        await refresh();
    } catch (error) {
        toast.add({ severity: 'error', summary: 'Errore', detail: readApiError(error), life: 7000 });
    } finally {
        busy.value = false;
    }
}

onMounted(refresh);
</script>

<template>
    <main class="mobile-page">
        <section class="shell">
            <section class="intro">
                <div class="intro-head">
                    <div class="intro-copywrap">
                        <div class="intro-kicker">Root owner</div>
                        <div class="intro-title">Console Owner</div>
                        <div class="intro-copy">Comandi essenziali di sistema — solo per il proprietario.</div>
                    </div>
                    <div class="status-pill">{{ loading ? 'Sync…' : 'Owner live' }}</div>
                </div>
                <div class="brand-signature">
                    <div class="brand-signature-mark">D2A</div>
                    <div class="brand-signature-copy">EuGenio Lab<br />Trading Dashboard 2A</div>
                </div>
                <MobileAdminQuickNav :on-sync="refresh" />
            </section>

            <div class="owner-panel card-block mb-3">
                <label class="block text-sm mb-2">Motivo (audit)</label>
                <InputText v-model="commandReason" class="w-full" placeholder="Opzionale" />
            </div>

            <div class="grid grid-cols-1 gap-2 mb-4">
                <Button label="Pausa Sistema" severity="warn" :loading="busy" class="w-full" @click="run('Pausa Sistema', RootOwnerService.pauseSystem)" />
                <Button label="Blackout Sistema" severity="danger" :loading="busy" class="w-full" @click="run('Blackout Sistema', RootOwnerService.blackoutSystem)" />
                <Button label="Riattiva Sistema" severity="success" :loading="busy" class="w-full" @click="run('Riattiva Sistema', RootOwnerService.reactivateSystem)" />
                <Button label="Stop tutti i bot" severity="danger" outlined :loading="busy" class="w-full" @click="run('Stop tutti i bot', RootOwnerService.stopAllBots)" />
                <Button label="Stop missione attiva" severity="secondary" :loading="busy" class="w-full" @click="run('Stop missione attiva', RootOwnerService.stopActiveMission)" />
                <Button label="Aggiorna stato" icon="pi pi-refresh" severity="secondary" outlined :loading="loading" class="w-full" @click="refresh" />
            </div>

            <div v-if="loading" class="text-center py-8 owner-muted">Caricamento…</div>

            <template v-else-if="status">
                <div class="owner-panel card-block mb-3">
                    <h3 class="mt-0 text-lg">Stato sistema</h3>
                    <p class="m-0"><strong>Modalità:</strong> {{ status.systemState }}</p>
                    <p class="m-0 mt-2">
                        <strong>API Decisore:</strong>
                        {{ status.api.reachable ? 'OK' : 'KO' }}
                        <span v-if="status.api.statusCode"> ({{ status.api.statusCode }})</span>
                    </p>
                    <p class="m-0 mt-2"><strong>Database:</strong> {{ status.database.ok ? 'OK' : 'KO' }}</p>
                    <p class="m-0 mt-2"><strong>Bot attivi:</strong> {{ status.activeBots }} / {{ status.totalBotRows }}</p>
                    <p class="m-0 mt-2">
                        <strong>Missione attiva:</strong>
                        <template v-if="status.activeMission">#{{ status.activeMission.sessionId }} · {{ status.activeMission.runtimeMode }} · margine {{ status.activeMission.totalMargin }}</template>
                        <template v-else>nessuna</template>
                    </p>
                </div>

                <div class="owner-panel card-block">
                    <h3 class="mt-0 text-lg">Ultimi audit critici</h3>
                    <ul v-if="status.recentAudits?.length" class="list-none p-0 m-0 flex flex-col gap-3">
                        <li v-for="row in status.recentAudits" :key="row.id" class="text-sm border-b border-surface pb-2">
                            <div class="font-semibold">{{ row.action }} · {{ row.outcome }}</div>
                            <div class="owner-muted">{{ row.actorUsername || '—' }} · {{ formatRomeDateTime(row.occurredAtUtc) }}</div>
                            <div v-if="row.reason" class="text-xs mt-1">{{ row.reason }}</div>
                        </li>
                    </ul>
                    <p v-else class="owner-muted m-0">Nessun evento registrato.</p>
                </div>
            </template>
        </section>
    </main>
</template>

<style scoped>
.mobile-page {
    min-height: 100vh;
    padding: 14px 14px 28px;
    background:
        radial-gradient(circle at top right, color-mix(in srgb, var(--primary-color) 22%, transparent), transparent 34%),
        linear-gradient(180deg, color-mix(in srgb, var(--surface-ground) 92%, #000 8%), var(--surface-ground));
    color: var(--text-color);
}
.shell {
    display: flex;
    flex-direction: column;
    gap: 14px;
}
.intro {
    display: flex;
    flex-direction: column;
    gap: 12px;
}
.intro-head {
    display: flex;
    justify-content: space-between;
    gap: 12px;
    align-items: flex-start;
}
.intro-kicker {
    font-size: 11px;
    letter-spacing: 0.18em;
    text-transform: uppercase;
    color: var(--primary-color);
}
.intro-title {
    margin-top: 6px;
    font-size: 24px;
    font-weight: 800;
    line-height: 1.05;
}
.intro-copy {
    margin-top: 8px;
    font-size: 13px;
    line-height: 1.45;
    color: var(--text-color-secondary);
}
.brand-signature {
    width: fit-content;
    padding: 10px 14px 12px;
    border-radius: 18px;
    border: 1px solid var(--surface-border);
    background: rgba(255, 255, 255, 0.05);
    box-shadow: 0 14px 34px rgba(0, 0, 0, 0.18);
}
.brand-signature-mark {
    font-size: 21px;
    color: var(--primary-color);
    font-weight: 900;
}
.brand-signature-copy {
    margin-top: 2px;
    font-size: 10px;
    line-height: 1.2;
}
.status-pill {
    display: inline-flex;
    align-items: center;
    gap: 8px;
    padding: 9px 12px;
    border-radius: 999px;
    background: rgba(255, 255, 255, 0.05);
    border: 1px solid var(--surface-border);
    font-size: 11px;
    color: var(--text-color);
    white-space: nowrap;
}
.status-pill::before {
    content: '';
    width: 8px;
    height: 8px;
    border-radius: 50%;
    background: #fb923c;
    box-shadow: 0 0 0 6px rgba(251, 146, 60, 0.12);
}
.owner-panel.card-block {
    border-radius: 30px;
    background: linear-gradient(180deg, rgba(255, 255, 255, 0.05), rgba(255, 255, 255, 0.015)), var(--surface-card);
    border: 1px solid var(--surface-border);
    box-shadow: 0 30px 80px rgba(0, 0, 0, 0.22);
    padding: 18px;
}
.owner-muted {
    color: var(--text-color-secondary);
}
</style>
