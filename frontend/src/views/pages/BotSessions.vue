<script setup>
import BotSessionService from '@/service/BotSessionService';
import { useToast } from 'primevue/usetoast';
import { onMounted, onUnmounted, ref } from 'vue';

const toast = useToast();
const sessions = ref([]);
const loading = ref(false);
const selectedSessions = ref([]);
const refreshInterval = ref(null);

// Fetch active sessions
const fetchActiveSessions = async () => {
    loading.value = true;
    try {
        sessions.value = await BotSessionService.getActiveSessions();
    } catch (error) {
        console.error('Error fetching active sessions:', error);
        toast.add({
            severity: 'error',
            summary: 'Errore',
            detail: 'Impossibile caricare le sessioni attive',
            life: 3000
        });
    } finally {
        loading.value = false;
    }
};

// Stop a bot session
const stopSession = async (session) => {
    try {
        const response = await BotSessionService.stopSession(session.pcName, session.botVersion);

        if (response.success) {
            toast.add({
                severity: 'success',
                summary: 'Successo',
                detail: `Sessione ${session.pcName} fermata con successo`,
                life: 3000
            });
            await fetchActiveSessions();
        } else {
            toast.add({
                severity: 'error',
                summary: 'Errore',
                detail: response.message || 'Errore durante lo stop della sessione',
                life: 3000
            });
        }
    } catch (error) {
        console.error('Error stopping session:', error);
        toast.add({
            severity: 'error',
            summary: 'Errore',
            detail: 'Impossibile fermare la sessione',
            life: 3000
        });
    }
};

// Cleanup inactive sessions
const cleanupInactive = async () => {
    try {
        const response = await BotSessionService.cleanupInactiveSessions();

        if (response.success) {
            toast.add({
                severity: 'success',
                summary: 'Successo',
                detail: 'Sessioni inattive pulite con successo',
                life: 3000
            });
            await fetchActiveSessions();
        }
    } catch (error) {
        console.error('Error cleaning up inactive sessions:', error);
        toast.add({
            severity: 'error',
            summary: 'Errore',
            detail: 'Impossibile pulire le sessioni inattive',
            life: 3000
        });
    }
};

// Format currency
const formatCurrency = (value) => {
    if (value == null) return '€0.00';
    return new Intl.NumberFormat('it-IT', {
        style: 'currency',
        currency: 'EUR',
        minimumFractionDigits: 2
    }).format(value);
};

// Get profit class for color coding
const getSessionProfitClass = (profit) => {
    if (profit > 0) return 'text-green-500 font-bold';
    if (profit < 0) return 'text-red-500 font-bold';
    return 'text-gray-500';
};

// Calculate session duration
const calculateDuration = (startDateTime) => {
    if (!startDateTime) return 'N/A';

    const start = new Date(startDateTime);
    const now = new Date();
    const diffMs = now - start;

    const hours = Math.floor(diffMs / (1000 * 60 * 60));
    const minutes = Math.floor((diffMs % (1000 * 60 * 60)) / (1000 * 60));

    return `${hours}h ${minutes}m`;
};

// Format date time
const formatDateTime = (dateTime) => {
    if (!dateTime) return 'N/A';

    const date = new Date(dateTime);
    return new Intl.DateTimeFormat('it-IT', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    }).format(date);
};

// Setup auto-refresh
onMounted(async () => {
    await fetchActiveSessions();

    // Auto-refresh every 30 seconds
    refreshInterval.value = setInterval(async () => {
        await fetchActiveSessions();
    }, 30000);
});

// Cleanup interval on unmount
onUnmounted(() => {
    if (refreshInterval.value) {
        clearInterval(refreshInterval.value);
    }
});
</script>

<template>
    <div class="card">
        <div class="flex justify-between items-center mb-4">
            <h2 class="text-2xl font-bold">Sessioni Bot Attive</h2>
            <div class="flex gap-2">
                <Button icon="pi pi-refresh" label="Aggiorna" @click="fetchActiveSessions" :loading="loading" outlined />
                <Button icon="pi pi-trash" label="Pulisci Inattive" @click="cleanupInactive" severity="warning" outlined />
            </div>
        </div>

        <DataTable
            v-model:selection="selectedSessions"
            :value="sessions"
            :loading="loading"
            dataKey="id"
            paginator
            :rows="10"
            :rowsPerPageOptions="[5, 10, 20, 50]"
            paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
            currentPageReportTemplate="Mostrando {first} a {last} di {totalRecords} sessioni"
            stripedRows
            responsiveLayout="scroll"
            class="p-datatable-sm"
        >
            <template #empty>
                <div class="text-center p-4">
                    <i class="pi pi-inbox text-4xl text-gray-400 mb-3"></i>
                    <p class="text-gray-500">Nessuna sessione attiva trovata</p>
                </div>
            </template>

            <Column field="pcName" header="PC" sortable style="min-width: 150px">
                <template #body="{ data }">
                    <div class="flex items-center gap-2">
                        <i class="pi pi-desktop text-blue-500"></i>
                        <span class="font-semibold">{{ data.pcName }}</span>
                    </div>
                </template>
            </Column>

            <Column field="botVersion" header="Versione Bot" sortable style="min-width: 120px">
                <template #body="{ data }">
                    <Tag :value="data.botVersion" severity="info" />
                </template>
            </Column>

            <Column field="startDateTime" header="Data Inizio" sortable style="min-width: 180px">
                <template #body="{ data }">
                    {{ formatDateTime(data.startDateTime) }}
                </template>
            </Column>

            <Column field="lastHeartbeat" header="Ultimo Heartbeat" sortable style="min-width: 180px">
                <template #body="{ data }">
                    {{ formatDateTime(data.lastHeartbeat) }}
                </template>
            </Column>

            <Column field="duration" header="Durata" style="min-width: 100px">
                <template #body="{ data }">
                    <Badge :value="calculateDuration(data.startDateTime)" severity="info" />
                </template>
            </Column>

            <Column field="initialBalance" header="Saldo Iniziale" sortable style="min-width: 140px">
                <template #body="{ data }">
                    <span class="font-semibold">{{ formatCurrency(data.initialBalance) }}</span>
                </template>
            </Column>

            <Column field="currentBalance" header="Saldo Corrente" sortable style="min-width: 140px">
                <template #body="{ data }">
                    <span class="font-semibold">{{ formatCurrency(data.currentBalance) }}</span>
                </template>
            </Column>

            <Column field="sessionProfit" header="Profitto" sortable style="min-width: 120px">
                <template #body="{ data }">
                    <span :class="getSessionProfitClass(data.sessionProfit)">
                        {{ formatCurrency(data.sessionProfit) }}
                    </span>
                </template>
            </Column>

            <Column field="isActive" header="Stato" sortable style="min-width: 100px">
                <template #body="{ data }">
                    <Tag :value="data.isActive ? 'Attivo' : 'Inattivo'" :severity="data.isActive ? 'success' : 'danger'" />
                </template>
            </Column>

            <Column header="Azioni" style="min-width: 100px">
                <template #body="{ data }">
                    <Button icon="pi pi-stop-circle" severity="danger" size="small" @click="stopSession(data)" :disabled="!data.isActive" v-tooltip.top="'Ferma sessione'" text rounded />
                </template>
            </Column>
        </DataTable>
    </div>
</template>

<style scoped>
.card {
    background: var(--surface-card);
    padding: 1.5rem;
    border-radius: 12px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}
</style>
