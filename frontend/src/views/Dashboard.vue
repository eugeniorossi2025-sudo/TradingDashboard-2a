<script setup>
import { signalRService } from '@/api/signalRService';
import ProfitsChats from '@/components/dashboard/ProfitsChats.vue';
import Stats from '@/components/dashboard/Stats.vue';
import StatsWidget from '@/components/dashboard/StatsWidget.vue';
import TableBots from '@/components/dashboard/TableBots.vue';
import { useAuth } from '@/composables/useAuth';
import { ConfigurationService } from '@/service/ConfigurationService';
import { DashboardService } from '@/service/DashboardService';
import { FinancialReportService } from '@/service/FinancialReportService';
import Dialog from 'primevue/dialog';
import { useToast } from 'primevue/usetoast';
import { computed, onMounted, onUnmounted, ref } from 'vue';

const toast = useToast();

// Stati reattivi per i dati real-time
const loading = ref(true);
const dashboardData = ref(null);
const lastInfo = ref(null);
const resultValutation = ref(null);
const tableData = ref([]);
const chartData = ref([]);
const marginiChartData = ref([]);
const statisticsData = ref([]);
const isConnected = ref(false);
const decisionMethod = ref(null);
const missionState = ref(null);
const LIVE_DATA_REFRESH_INTERVAL_MS = 3000;
let liveDataRefreshInterval = null;

const { isAdmin } = useAuth();

const missionStatusLabel = computed(() => {
    if (!missionState.value?.hasOpenMission) return 'Nessuna missione aperta';
    return `Missione #${missionState.value.sessionId} aperta (${missionState.value.runtimeMode})`;
});

function parseServerUtcDate(value) {
    if (!value) return null;
    if (value instanceof Date) return value;
    if (typeof value !== 'string') return new Date(value);

    const normalized = /(?:Z|[+-]\d{2}:?\d{2})$/i.test(value) ? value : `${value}Z`;
    return new Date(normalized);
}

// 🔹 FETCH INITIAL DASHBOARD DATA
const fetchDashboardData = async () => {
    try {
        const data = await DashboardService.getDashboardData();
        chartData.value = await DashboardService.getChartData();
        await refreshTelemetry();
        await refreshMissionState();
        await refreshProfitChart();
        if (data) {
            dashboardData.value = data;
            const rows = Array.isArray(data) ? data : data.rows || data.tables || [];
            if (rows.length > 0) {
                const mostRecent = rows.reduce((a, b) => {
                    const aTime = parseServerUtcDate(a.lastUpdate || a.last_update)?.getTime() ?? 0;
                    const bTime = parseServerUtcDate(b.lastUpdate || b.last_update)?.getTime() ?? 0;
                    return bTime > aTime ? b : a;
                });
                if (mostRecent.lastInfo) {
                    try {
                        lastInfo.value = JSON.parse(mostRecent.lastInfo);
                    } catch {
                        lastInfo.value = null;
                    }
                } else {
                    lastInfo.value = null;
                }
                if (mostRecent.resultValutation) {
                    try {
                        resultValutation.value = JSON.parse(mostRecent.resultValutation);
                    } catch {
                        resultValutation.value = null;
                    }
                } else {
                    resultValutation.value = null;
                }
            } else {
                lastInfo.value = null;
                resultValutation.value = null;
            }
            tableData.value = rows.map((row) => {
                const lastAdviceStr = row.last_advice || row.lastAdvice;
                let lastAdvice = null;
                if (lastAdviceStr && typeof lastAdviceStr === 'string') {
                    try {
                        lastAdvice = parseJsonRecursive(lastAdviceStr);
                    } catch {
                        lastAdvice = null;
                    }
                }
                let parsedLastInfo = null;
                if (row.lastInfo && typeof row.lastInfo === 'string') {
                    try {
                        parsedLastInfo = parseJsonRecursive(row.lastInfo);
                    } catch {
                        parsedLastInfo = null;
                    }
                }
                // Se presente, mappa i valori estratti nei campi della tabella
                if (lastAdvice && typeof lastAdvice === 'object') {
                    return {
                        ...row,
                        valutazione: lastAdvice.Reason || row.valutazione,
                        reason: lastAdvice.Reason || row.reason,
                        prediction: lastAdvice.Prediction || row.prediction,
                        futureL5Pred: lastAdvice.FutureL5Pred || row.futureL5Pred,
                        stopAtL5: lastAdvice.StopAtL5 ?? row.stopAtL5,
                        authorizedHeavy: lastAdvice.AuthorizedHeavy ?? row.authorizedHeavy,
                        signalW10: lastAdvice.SignalW10 || row.signalW10,
                        tableScore: lastAdvice.TableScore || row.tableScore,
                        levelIndex: lastAdvice.LevelIndex ?? row.levelIndex,
                        lastAction: lastAdvice.ActionCode ?? row.lastAction,
                        _lastAdvice: lastAdvice,
                        _lastInfo: parsedLastInfo
                    };
                }
                return { ...row, _lastAdvice: lastAdvice, _lastInfo: parsedLastInfo };
                // Funzione ricorsiva per il parsing di JSON anche annidati
                function parseJsonRecursive(json) {
                    let obj = typeof json === 'string' ? JSON.parse(json) : json;
                    if (obj && typeof obj === 'object') {
                        for (const key in obj) {
                            if (typeof obj[key] === 'string') {
                                try {
                                    const parsed = JSON.parse(obj[key]);
                                    obj[key] = parseJsonRecursive(parsed);
                                } catch {
                                    // non è un json annidato, lascia la stringa
                                }
                            }
                        }
                    }
                    return obj;
                }
            });
        }
    } catch {
        toast.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to load dashboard data',
            life: 3000
        });
    }
};

const refreshMissionState = async () => {
    try {
        missionState.value = await FinancialReportService.getCurrentMission();
    } catch (error) {
        console.error('❌ Error loading mission state:', error);
        missionState.value = null;
    }
};

const refreshProfitChart = async () => {
    if (!missionState.value?.hasOpenMission) {
        marginiChartData.value = [];
        return;
    }

    try {
        marginiChartData.value = await DashboardService.getMarginiChart();
    } catch (error) {
        console.error('❌ Error loading profits chart:', error);
    }
};

const appendLiveProfitPoint = (payload) => {
    const rows = Array.isArray(payload) ? payload : payload?.tables || payload?.rows || [];
    if (!Array.isArray(rows) || !rows.length) return;

    const payloadMargin = payload?.statistics?.margineAttuale ?? payload?.statistics?.totalMargine;
    const liveMargin = Number(payloadMargin ?? rows.reduce((sum, row) => sum + Number(row.margine ?? row.Margine ?? 0), 0));
    if (!Number.isFinite(liveMargin)) return;

    const latestTimestamp = rows
        .map((row) => parseServerUtcDate(row.lastUpdate || row.last_update || row.dtUltimo || row.DtUltimo))
        .filter((date) => date && !Number.isNaN(date.getTime()))
        .sort((a, b) => b.getTime() - a.getTime())[0] ?? new Date();

    const dateTime = latestTimestamp.toISOString();
    const nextPoint = { dateTime, margine: Number(liveMargin.toFixed(2)) };
    const points = Array.isArray(marginiChartData.value) ? [...marginiChartData.value] : [];
    const lastPoint = points[points.length - 1];

    if (lastPoint && (lastPoint.dateTime ?? lastPoint.timestamp) === dateTime) {
        points[points.length - 1] = nextPoint;
        marginiChartData.value = points;
        return;
    }

    if (lastPoint && Number(lastPoint.margine) === nextPoint.margine) return;

    marginiChartData.value = [...points, nextPoint].slice(-200);
};

const refreshTelemetry = async () => {
    try {
        const telemetry = await DashboardService.getTelemetry();
        if (!telemetry) return;

        statisticsData.value = [{
            timestamp: telemetry.sessionStart ?? new Date().toISOString(),
            sessionEnd: telemetry.sessionEnd ?? null,
            margine: telemetry.margineTot ?? 0,
            margineMin: telemetry.margineMin ?? 0,
            margineMax: telemetry.margineMax ?? 0,
            elapsed: telemetry.elapsed ?? 0,
            telemetry: telemetry.rawTelemetry ?? null
        }];
    } catch (error) {
        console.error('❌ Error refreshing telemetry:', error);
    }
};

const startLiveDataPolling = () => {
    if (liveDataRefreshInterval) return;
    liveDataRefreshInterval = window.setInterval(async () => {
        await Promise.all([refreshTelemetry(), refreshMissionState()]);
        await refreshProfitChart();
    }, LIVE_DATA_REFRESH_INTERVAL_MS);
};

const stopLiveDataPolling = () => {
    if (!liveDataRefreshInterval) return;
    window.clearInterval(liveDataRefreshInterval);
    liveDataRefreshInterval = null;
};

const showResetDialog = ref(false);
const resetLoading = ref(false);

const confirmResetDashboard = async () => {
    resetLoading.value = true;
    try {
        const result = await DashboardService.resetDashboard();
        const mission = result?.mission;
        toast.add({
            severity: 'success',
            summary: 'Reset effettuato',
            detail: mission?.missionFinalized ? `Report missione #${mission.missionSessionId} finalizzato e reset inviato` : 'Reset inviato al Decisore',
            life: 2000
        });
        await fetchDashboardData();
        showResetDialog.value = false;
    } catch (error) {
        console.error('❌ Error during dashboard reset:', error);
        toast.add({
            severity: 'error',
            summary: 'Errore',
            detail: 'Reset dashboard fallito',
            life: 3000
        });
    } finally {
        resetLoading.value = false;
    }
};

const showStopEmergencyDialog = ref(false);

const confirmStopEmergency = async () => {
    resetLoading.value = true;
    try {
        await DashboardService.stopDashboard();
        toast.add({
            severity: 'success',
            summary: 'Stop avvenuto con successo',
            detail: 'Stop Pc di emergenza effettuato',
            life: 2000
        });
        await fetchDashboardData();
        showStopEmergencyDialog.value = false;
    } catch (error) {
        console.error('❌ Error during dashboard stop emergency:', error);
        toast.add({
            severity: 'error',
            summary: 'Errore',
            detail: 'Stop Pc di emergenza fallito',
            life: 3000
        });
    } finally {
        resetLoading.value = false;
    }
};

const onDashboardUpdate = (jsonPayload) => {
    try {
        const data = jsonPayload;

        // Se il payload è un array (vecchio formato)
        if (Array.isArray(data)) {
            // Estrai lastInfo dal primo elemento se presente
            if (data.length > 0 && data[0].lastInfo) {
                try {
                    lastInfo.value = JSON.parse(data[0].lastInfo);
                    resultValutation.value = JSON.parse(data[0].resultValutation);
                } catch {
                    lastInfo.value = null;
                    resultValutation.value = null;
                }
            } else {
                lastInfo.value = null;
                resultValutation.value = null;
            }
            // Applica mapping last_advice anche qui
            tableData.value = data.map((row) => {
                const lastAdviceStr = row.last_advice || row.lastAdvice;
                let lastAdvice = null;
                if (lastAdviceStr && typeof lastAdviceStr === 'string') {
                    try {
                        lastAdvice = JSON.parse(lastAdviceStr);
                    } catch {
                        lastAdvice = null;
                    }
                }
                if (lastAdvice && typeof lastAdvice === 'object') {
                    return {
                        ...row,
                        valutazione: lastAdvice.Reason || row.valutazione,
                        reason: lastAdvice.Reason || row.reason,
                        prediction: lastAdvice.Prediction || row.prediction,
                        futureL5Pred: lastAdvice.FutureL5Pred || row.futureL5Pred,
                        stopAtL5: lastAdvice.StopAtL5 ?? row.stopAtL5,
                        authorizedHeavy: lastAdvice.AuthorizedHeavy ?? row.authorizedHeavy,
                        signalW10: lastAdvice.SignalW10 || row.signalW10,
                        tableScore: lastAdvice.TableScore || row.tableScore,
                        levelIndex: lastAdvice.LevelIndex ?? row.levelIndex,
                        lastAction: lastAdvice.ActionCode ?? row.lastAction
                    };
                }
                return row;
            });
        } else if (data && typeof data === 'object') {
            // Se il payload è un oggetto DashboardResponse (nuovo formato)
            if (data.tables) {
                // Estrai lastInfo dal primo elemento se presente
                if (data.tables.length > 0 && data.tables[0].lastInfo) {
                    try {
                        lastInfo.value = JSON.parse(data.tables[0].lastInfo);
                        resultValutation.value = JSON.parse(data.tables[0].resultValutation);
                    } catch {
                        lastInfo.value = null;
                        resultValutation.value = null;
                    }
                } else {
                    lastInfo.value = null;
                    resultValutation.value = null;
                }
                tableData.value = data.tables.map((row) => {
                    const lastAdviceStr = row.last_advice || row.lastAdvice;
                    let lastAdvice = null;
                    if (lastAdviceStr && typeof lastAdviceStr === 'string') {
                        try {
                            lastAdvice = JSON.parse(lastAdviceStr);
                        } catch {
                            lastAdvice = null;
                        }
                    }
                    if (lastAdvice && typeof lastAdvice === 'object') {
                        return {
                            ...row,
                            valutazione: lastAdvice.Reason || row.valutazione,
                            reason: lastAdvice.Reason || row.reason,
                            prediction: lastAdvice.Prediction || row.prediction,
                            futureL5Pred: lastAdvice.FutureL5Pred || row.futureL5Pred,
                            stopAtL5: lastAdvice.StopAtL5 ?? row.stopAtL5,
                            authorizedHeavy: lastAdvice.AuthorizedHeavy ?? row.authorizedHeavy,
                            signalW10: lastAdvice.SignalW10 || row.signalW10,
                            tableScore: lastAdvice.TableScore || row.tableScore,
                            levelIndex: lastAdvice.LevelIndex ?? row.levelIndex,
                            lastAction: lastAdvice.ActionCode ?? row.lastAction
                        };
                    }
                    return row;
                });
            }
        }
    } catch (error) {
        console.error('❌ Error parsing SignalR payload:', error);
    }

    appendLiveProfitPoint(data);
    Promise.all([refreshTelemetry(), refreshMissionState()]).catch((error) => {
        console.error('❌ Error refreshing dashboard live data:', error);
    });
};

onMounted(async () => {
    loading.value = true;
    const decisionMethodResponse = await ConfigurationService.getConfigurationById('DECISION_METHOD');
    decisionMethod.value = decisionMethodResponse ? decisionMethodResponse.value : null;
    await fetchDashboardData();
    startLiveDataPolling();
    try {
        await signalRService.startConnection('/dashboardHub');
        isConnected.value = true;

        // Registra i listener per gli eventi (SignalR usa lowercase per i nomi dei metodi)
        signalRService.on('ReceiveDashboardUpdate', onDashboardUpdate);
        signalRService.on('ReceiveDashboardChartUpdate', (chartPayload) => {
            try {
                chartData.value = chartPayload.points || [];
                statisticsData.value = chartPayload.histories || [];
            } catch (error) {
                console.error('❌ Error parsing Chart SignalR payload:', error);
            }
        });

        toast.add({
            severity: 'success',
            summary: 'Connected',
            detail: 'Real-time updates active',
            life: 3000
        });
    } catch (error) {
        console.error('❌ Failed to connect to SignalR:', error);
        isConnected.value = false;

        toast.add({
            severity: 'warn',
            summary: 'Connection Warning',
            detail: 'Real-time updates unavailable',
            life: 3000
        });
    } finally {
        loading.value = false;
    }
});

const latestStatisticData = computed(() => {
    if (statisticsData.value && statisticsData.value.length > 0) {
        const openStat = statisticsData.value.find((stat) => stat.sessionEnd == null);
        if (openStat) return openStat;
        return statisticsData.value[statisticsData.value.length - 1];
    }
    return null;
});

onUnmounted(async () => {
    // Rimuovi i listener
    stopLiveDataPolling();
    signalRService.off('ReceiveDashboardUpdate', onDashboardUpdate);
    await signalRService.stopConnection();
});
</script>

<template>
    <div class="grid grid-cols-12 gap-8">
        <!-- Loading State -->
        <div v-if="loading" class="col-span-12">
            <div class="card">
                <div class="flex justify-content-center align-items-center" style="min-height: 200px">
                    <ProgressSpinner />
                </div>
            </div>
        </div>

        <!-- Dashboard Content -->
        <template v-else>
            <!-- Bottone Reset Dashboard sopra la tabella -->
            <Stats :tableData="tableData || []" :chartData="statisticsData || []" v-if="statisticsData" />

            <div class="col-span-12 flex justify-end mb-2 gap-2" v-if="isAdmin">
                <div class="mr-auto flex items-center text-sm text-muted-color">
                    {{ missionStatusLabel }}
                </div>
                <Button severity="danger" class="p-button p-component" @click="showStopEmergencyDialog = true">
                    <span class="pi pi-power-off mr-2"></span>
                    Arresto di emergenza
                </Button>
                <Button severity="primary" class="p-button p-component" @click="showResetDialog = true">
                    <span class="pi pi-refresh mr-2"></span>
                    Reset Dashboard
                </Button>
            </div>
            <Dialog v-model:visible="showStopEmergencyDialog" :closable="!resetLoading" :modal="true" :dismissableMask="!resetLoading" :style="{ width: '350px' }">
                <template #header>
                    <span>Arresto di emergenza</span>
                </template>
                <div class="mb-4">Sei sicuro di voler stoppare tutti i PC?</div>
                <div class="flex justify-end gap-2">
                    <Button class="p-button p-component" @click="showStopEmergencyDialog = false" :disabled="resetLoading">Annulla</Button>
                    <Button severity="danger" class="p-button p-component" @click="confirmStopEmergency" :disabled="resetLoading">
                        <span v-if="resetLoading" class="pi pi-spin pi-spinner mr-2"></span>
                        <span v-else class="pi pi-check mr-2"></span>
                        Conferma
                    </Button>
                </div>
            </Dialog>

            <Dialog v-model:visible="showResetDialog" :closable="!resetLoading" :modal="true" :dismissableMask="!resetLoading" :style="{ width: '350px' }">
                <template #header>
                    <span>Conferma Reset</span>
                </template>
                <div class="mb-4">
                    Sei sicuro di voler resettare la dashboard?
                    <div v-if="missionState?.hasOpenMission" class="mt-2 text-sm text-muted-color">
                        Prima del reset verra finalizzata la missione #{{ missionState.sessionId }} e generato il report contabile.
                    </div>
                </div>
                <div class="flex justify-end gap-2">
                    <Button class="p-button p-component" @click="showResetDialog = false" :disabled="resetLoading">Annulla</Button>
                    <Button severity="danger" class="p-button p-component p-button-danger" @click="confirmResetDashboard" :disabled="resetLoading">
                        <span v-if="resetLoading" class="pi pi-spin pi-spinner mr-2"></span>
                        <span v-else class="pi pi-check mr-2"></span>
                        Conferma
                    </Button>
                </div>
            </Dialog>
            <div class="col-span-12">
                <TableBots :tableData="tableData || []" :decisionMethod="decisionMethod || ''" />
            </div>
            <StatsWidget :telemetry="latestStatisticData?.telemetry" />
            <div class="col-span-12">
                <ProfitsChats :title="'Profits Chart'" :chartData="marginiChartData.length ? marginiChartData : (chartData || [])" />
            </div>
        </template>
    </div>
</template>
