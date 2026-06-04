<script setup>
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
    <div class="root-owner-page min-h-screen p-4 pb-10">
        <header class="mb-4">
            <p class="text-xs uppercase tracking-widest text-orange-400 m-0">Console nascosta</p>
            <h1 class="text-2xl font-bold m-0 mt-1">Root Owner</h1>
            <p class="text-sm text-muted-color m-0 mt-2">Comandi essenziali — non compare nel menu.</p>
        </header>

        <div class="card mb-4">
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

        <div v-if="loading" class="text-center py-8 text-muted-color">Caricamento…</div>

        <template v-else-if="status">
            <div class="card mb-3">
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

            <div class="card">
                <h3 class="mt-0 text-lg">Ultimi audit critici</h3>
                <ul v-if="status.recentAudits?.length" class="list-none p-0 m-0 flex flex-col gap-3">
                    <li v-for="row in status.recentAudits" :key="row.id" class="text-sm border-b border-surface pb-2">
                        <div class="font-semibold">{{ row.action }} · {{ row.outcome }}</div>
                        <div class="text-muted-color">{{ row.actorUsername || '—' }} · {{ formatRomeDateTime(row.occurredAtUtc) }}</div>
                        <div v-if="row.reason" class="text-xs mt-1">{{ row.reason }}</div>
                    </li>
                </ul>
                <p v-else class="text-muted-color m-0">Nessun evento registrato.</p>
            </div>
        </template>
    </div>
</template>

<style scoped>
.root-owner-page {
    background: linear-gradient(165deg, #0f172a 0%, #1e293b 45%, #0f172a 100%);
    color: #f8fafc;
}
.root-owner-page :deep(.card) {
    background: rgba(15, 23, 42, 0.85);
    border: 1px solid rgba(251, 146, 60, 0.25);
    border-radius: 12px;
    padding: 1rem;
}
</style>
