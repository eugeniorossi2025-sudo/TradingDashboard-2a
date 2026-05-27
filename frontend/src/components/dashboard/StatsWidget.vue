<script setup>
import { computed } from 'vue';

const props = defineProps({
    telemetry: {
        type: String,
        default: null
    }
});

const telemetryData = computed(() => {
    if (!props.telemetry) return {};
    try {
        return JSON.parse(props.telemetry);
    } catch {
        return {};
    }
});

const securityFilterRows = computed(() => {
    const byBot = telemetryData.value?.SecurityFilterByBot;
    if (!byBot || typeof byBot !== 'object') return [];

    return Object.entries(byBot)
        .map(([computer, row]) => ({
            computer,
            ...(row || {})
        }))
        .sort((a, b) => String(a.computer).localeCompare(String(b.computer)));
});

const securityFilterSetup = computed(() => ({
    minScore: telemetryData.value?.SecurityFilterMinScore ?? 3,
    minStreak: telemetryData.value?.SecurityFilterMinStreak ?? 5,
    maxShoeHand: telemetryData.value?.SecurityFilterMaxShoeHand ?? 20,
    maxAvgSeconds: telemetryData.value?.SecurityFilterMaxAvgSeconds ?? 23.5,
    veryFastSeconds: telemetryData.value?.SecurityFilterVeryFastSeconds ?? 21.0,
    deltaWindow: telemetryData.value?.SecurityFilterDeltaWindow ?? 8
}));

function formatSeconds(value) {
    if (value == null || Number(value) <= 0) return '-';
    return `${Number(value).toFixed(1)}s`;
}

function formatDuration(value) {
    const seconds = Number(value);
    if (!Number.isFinite(seconds) || seconds <= 0) return '-';
    if (seconds < 60) return `${seconds.toFixed(1)}s`;

    const minutes = Math.floor(seconds / 60);
    const remainingSeconds = Math.round(seconds % 60);
    if (minutes < 60) return remainingSeconds > 0 ? `${minutes}m ${remainingSeconds}s` : `${minutes}m`;

    const hours = Math.floor(minutes / 60);
    const remainingMinutes = minutes % 60;
    return remainingMinutes > 0 ? `${hours}h ${remainingMinutes}m` : `${hours}h`;
}

function formatDurationRange(minValue, maxValue) {
    const min = formatDuration(minValue);
    const max = formatDuration(maxValue);
    if (min === '-' && max === '-') return '-';
    return `${min} - ${max}`;
}

function getNumber(value, fallback = 0) {
    const numberValue = Number(value);
    return Number.isFinite(numberValue) ? numberValue : fallback;
}

function getRiskPillClass(active) {
    return active ? 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-300' : 'bg-surface-100 text-muted-color dark:bg-surface-800';
}

function getScorePoint(active) {
    return active ? '+1' : '+0';
}

function isAvgFast(row) {
    const avg = getNumber(row?.AvgHandSeconds);
    return avg > 0 && avg < getNumber(securityFilterSetup.value.maxAvgSeconds);
}

function isVeryFast(row) {
    const avg = getNumber(row?.AvgHandSeconds);
    return avg > 0 && avg < getNumber(securityFilterSetup.value.veryFastSeconds);
}

function isStreakRisk(row) {
    return getNumber(row?.CurrentStreak) >= getNumber(securityFilterSetup.value.minStreak);
}

function isShoeRisk(row) {
    return getNumber(row?.LastShoeHand, Number.MAX_SAFE_INTEGER) <= getNumber(securityFilterSetup.value.maxShoeHand);
}

function isSecurityFilterEnabled() {
    return telemetryData.value?.SecurityFilterEnabled !== false;
}

function getSecurityFilterStatus(row) {
    if (!isSecurityFilterEnabled()) return 'Disattivato';
    return row?.SecurityFilterActive ? 'Pausa bot' : 'In valutazione';
}

function getSecurityFilterStatusClass(row) {
    if (!isSecurityFilterEnabled()) return 'bg-surface-100 text-muted-color dark:bg-surface-800';
    if (row?.SecurityFilterActive) return 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-300';
    return 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-300';
}

function getSecurityFilterRowClass(row) {
    if (!isSecurityFilterEnabled()) return 'bg-surface-50/50 dark:bg-surface-900/30';
    if (row?.SecurityFilterActive) return 'bg-red-50 dark:bg-red-950/30';
    return 'bg-emerald-50/60 dark:bg-emerald-950/20';
}

function getScoreClass(row) {
    const score = Number(row?.SecurityRiskScore ?? 0);
    if (score >= 3) return 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-300';
    if (score >= 2) return 'bg-orange-100 text-orange-700 dark:bg-orange-900/30 dark:text-orange-300';
    if (score >= 1) return 'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-300';
    return 'bg-surface-100 text-muted-color dark:bg-surface-800';
}
</script>

<template>
    <!-- ================= GLOBAL STATISTICS ================= -->
    <div class="col-span-12">
        <h3 class="text-xl font-semibold mb-4">Global Statistics</h3>
    </div>

    <div class="col-span-12 lg:col-span-6 xl:col-span-3">
        <div class="card">
            <span class="block text-muted-color font-medium mb-4">Total L6 Authorized</span>
            <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">
                {{ telemetryData?.TotalAuthL6Authorized }}
            </div>
        </div>
    </div>

    <div class="col-span-12 lg:col-span-6 xl:col-span-3">
        <div class="card">
            <span class="block text-muted-color font-medium mb-4">Total L5 Played</span>
            <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">
                {{ telemetryData?.TotalL5Played }}
            </div>
        </div>
    </div>

    <div class="col-span-12 lg:col-span-6 xl:col-span-3">
        <div class="card">
            <span class="block text-muted-color font-medium mb-4">Total L5 Won</span>
            <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">
                {{ telemetryData?.TotalL5Won }}
            </div>
        </div>
    </div>

    <div class="col-span-12 lg:col-span-6 xl:col-span-3">
        <div class="card">
            <span class="block text-muted-color font-medium mb-4">Total L5 Lost</span>
            <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">
                {{ telemetryData?.TotalL5Lost }}
            </div>
        </div>
    </div>

    <div class="col-span-12 lg:col-span-6 xl:col-span-3">
        <div class="card">
            <span class="block text-muted-color font-medium mb-4">Total PB Hands Played</span>
            <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">
                {{ telemetryData?.TotalPBHandsPlayed }}
            </div>
        </div>
    </div>

    <div class="col-span-12 lg:col-span-6 xl:col-span-3">
        <div class="card">
            <span class="block text-muted-color font-medium mb-4">Total L8 Played</span>
            <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">
                {{ telemetryData?.TotalL8Played }}
            </div>
        </div>
    </div>

    <div class="col-span-12 lg:col-span-6 xl:col-span-3">
        <div class="card">
            <span class="block text-muted-color font-medium mb-4">Total L8 Won</span>
            <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">
                {{ telemetryData?.TotalL8Won }}
            </div>
        </div>
    </div>

    <div class="col-span-12 lg:col-span-6 xl:col-span-3">
        <div class="card">
            <span class="block text-muted-color font-medium mb-4">Total L8 Lost</span>
            <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">
                {{ telemetryData?.TotalL8Lost }}
            </div>
        </div>
    </div>

    <!-- ================= SPOT METRICS ================= -->
    <div class="col-span-12 mt-6">
        <h3 class="text-xl font-semibold mb-4">Spot Metrics</h3>
    </div>

    <div class="col-span-12 lg:col-span-6 xl:col-span-3">
        <div class="card">
            <span class="block text-muted-color font-medium mb-4">Spot ID</span>
            <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">
                {{ telemetryData?.SpotID }}
            </div>
        </div>
    </div>

    <div class="col-span-12 lg:col-span-6 xl:col-span-3">
        <div class="card">
            <span class="block text-muted-color font-medium mb-4">Spot PB Hands Played</span>
            <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">
                {{ telemetryData?.SpotPBHandsPlayed }}
            </div>
        </div>
    </div>

    <div class="col-span-12 lg:col-span-6 xl:col-span-3">
        <div class="card">
            <span class="block text-muted-color font-medium mb-4">Spot L6 Authorized</span>
            <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">
                {{ telemetryData?.SpotAuthL6Counter }}
            </div>
        </div>
    </div>

    <div class="col-span-12 lg:col-span-6 xl:col-span-3">
        <div class="card">
            <span class="block text-muted-color font-medium mb-4">Spot L5 Loss</span>
            <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">
                {{ telemetryData?.SpotL5Loss }}
            </div>
        </div>
    </div>

    <!-- ================= INDICATORS & PAUSE ================= -->
    <div class="col-span-12 mt-6">
        <h3 class="text-xl font-semibold mb-4">Indicators & Pause Logic</h3>
    </div>

    <div class="col-span-12 lg:col-span-6 xl:col-span-3">
        <div class="card">
            <span class="block text-muted-color font-medium mb-2">Global Pause Scalping</span>
            <div class="text-xl font-medium">
                {{ telemetryData?.GlobalPauseScalping ? 'Attiva' : 'Non attiva' }}
            </div>
            <div class="text-sm text-muted-color mt-2">
                {{ telemetryData?.GlobalPauseScalpingDetails }}
            </div>
            <div class="text-sm text-muted-color">Durata: {{ telemetryData?.GlobalPauseScalpingDuration }}</div>
            <div class="text-sm text-muted-color">Pause Soglie: {{ telemetryData?.TotalPauseScalpingSoglieActivated }}</div>
            <div class="text-sm text-muted-color">Pause EWMA: {{ telemetryData?.TotalPauseScalpingEWMAActivated }}</div>
        </div>
    </div>

    <div class="col-span-12 lg:col-span-6 xl:col-span-3">
        <div class="card">
            <span class="block text-muted-color font-medium mb-4">INC</span>
            <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">
                {{ telemetryData?.INC }}
            </div>
        </div>
    </div>

    <div class="col-span-12 lg:col-span-6 xl:col-span-3">
        <div class="card">
            <span class="block text-muted-color font-medium mb-4">EWMA</span>
            <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">
                {{ telemetryData?.EWMA }}
            </div>
        </div>
    </div>

    <!-- ================= SECURITY FILTER ================= -->
    <div class="col-span-12 mt-6">
        <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-2 mb-4">
            <h3 class="text-xl font-semibold m-0">Security Filter (sperimentale)</h3>
            <span class="text-sm" :class="telemetryData?.SecurityFilterEnabled === false ? 'text-red-500' : 'text-green-500'">
                {{ telemetryData?.SecurityFilterEnabled === false ? 'Disattivato da Config' : 'Attivo da Config' }}
            </span>
        </div>
    </div>

    <div class="col-span-12 lg:col-span-6 xl:col-span-3">
        <div class="card">
            <span class="block text-muted-color font-medium mb-4">Security Filter Attivazioni</span>
            <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">
                {{ telemetryData?.TotalSecurityFilterActivated ?? 0 }}
            </div>
            <div class="text-sm text-muted-color mt-2">Avg hand: {{ telemetryData?.LastAvgHandSeconds != null ? Number(telemetryData.LastAvgHandSeconds).toFixed(1) + 's' : '-' }}</div>
            <div class="text-sm text-muted-color">Bot attivi: {{ telemetryData?.ActiveSecurityFilterBots ?? 0 }}</div>
        </div>
    </div>

    <div class="col-span-12 lg:col-span-6 xl:col-span-3">
        <div class="card">
            <span class="block text-muted-color font-medium mb-4">Security Filter L6 Prevenuti</span>
            <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">
                {{ telemetryData?.TotalSecurityFilterPreventedL6 ?? 0 }}
            </div>
            <div class="text-sm text-muted-color mt-2">Filtro a L5 con credito disponibile</div>
        </div>
    </div>

    <div class="col-span-12">
        <div class="card">
            <div class="flex flex-col gap-1 mb-4">
                <span class="block text-muted-color font-medium">Setup Security Filter</span>
                <span class="text-sm text-muted-color">Condizioni correnti usate per comporre lo score per singolo bot.</span>
            </div>
            <div class="grid grid-cols-12 gap-3 text-sm">
                <div class="col-span-12 md:col-span-6 xl:col-span-4 rounded-xl bg-surface-50 p-3 dark:bg-surface-800">
                    <div class="text-muted-color mb-1">Soglia attivazione</div>
                    <div class="font-semibold">Score minimo {{ securityFilterSetup.minScore }}/4</div>
                </div>
                <div class="col-span-12 md:col-span-6 xl:col-span-4 rounded-xl bg-surface-50 p-3 dark:bg-surface-800">
                    <div class="text-muted-color mb-1">Finestra avg mano</div>
                    <div class="font-semibold">Ultimi {{ securityFilterSetup.deltaWindow }} delta mano</div>
                </div>
                <div class="col-span-12 md:col-span-6 xl:col-span-4 rounded-xl bg-surface-50 p-3 dark:bg-surface-800">
                    <div class="text-muted-color mb-1">Media</div>
                    <div class="font-semibold">Trimmata quando ci sono almeno 3 campioni</div>
                </div>
                <div class="col-span-12 md:col-span-6 xl:col-span-3 rounded-xl border border-surface-200 p-3 dark:border-surface-700">
                    <div class="font-semibold">+1 streak</div>
                    <div class="text-muted-color">se streak &gt;= {{ securityFilterSetup.minStreak }}</div>
                </div>
                <div class="col-span-12 md:col-span-6 xl:col-span-3 rounded-xl border border-surface-200 p-3 dark:border-surface-700">
                    <div class="font-semibold">+1 avg veloce</div>
                    <div class="text-muted-color">se avg mano &lt; {{ Number(securityFilterSetup.maxAvgSeconds).toFixed(1) }}s</div>
                </div>
                <div class="col-span-12 md:col-span-6 xl:col-span-3 rounded-xl border border-surface-200 p-3 dark:border-surface-700">
                    <div class="font-semibold">+1 inizio shoe</div>
                    <div class="text-muted-color">se mano shoe &lt;= {{ securityFilterSetup.maxShoeHand }}</div>
                </div>
                <div class="col-span-12 md:col-span-6 xl:col-span-3 rounded-xl border border-surface-200 p-3 dark:border-surface-700">
                    <div class="font-semibold">+1 very fast</div>
                    <div class="text-muted-color">se avg mano &lt; {{ Number(securityFilterSetup.veryFastSeconds).toFixed(1) }}s</div>
                </div>
            </div>
        </div>
    </div>

    <div class="col-span-12" v-if="securityFilterRows.length">
        <div class="card">
            <div class="flex justify-between items-center mb-4">
                <span class="block text-muted-color font-medium">Security Filter per bot</span>
                <span class="text-sm text-muted-color">{{ securityFilterRows.length }} bot</span>
            </div>
            <div class="grid grid-cols-1 gap-4">
                <div v-for="row in securityFilterRows" :key="row.computer" class="rounded-2xl border border-surface-200 p-4 transition-colors dark:border-surface-700" :class="getSecurityFilterRowClass(row)">
                    <div class="mb-4 flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
                        <div class="flex flex-wrap items-center gap-2">
                            <span class="inline-flex items-center gap-2 rounded-full bg-primary-100 px-3 py-1 font-semibold text-primary-700 dark:bg-primary-900/30 dark:text-primary-300">
                                <span class="h-2 w-2 rounded-full" :class="row.SecurityFilterActive ? 'bg-red-500' : 'bg-emerald-500'"></span>
                                {{ row.Computer || row.computer }}
                            </span>
                            <span class="rounded-full bg-surface-100 px-2.5 py-1 text-xs font-semibold text-muted-color dark:bg-surface-800">L{{ row.Martingala ?? '-' }}</span>
                        </div>
                        <div class="flex flex-wrap items-center gap-2">
                            <span class="rounded-full px-2.5 py-1 text-xs font-semibold" :class="getScoreClass(row)">{{ row.SecurityRiskScore ?? 0 }}/4</span>
                            <span class="rounded-full px-2.5 py-1 text-xs font-semibold" :class="getSecurityFilterStatusClass(row)">{{ getSecurityFilterStatus(row) }}</span>
                            <span class="text-xs text-muted-color">pausa da {{ securityFilterSetup.minScore }}/4</span>
                        </div>
                    </div>

                    <div class="grid grid-cols-1 gap-3 md:grid-cols-2 xl:grid-cols-3">
                        <div class="rounded-xl bg-surface-0 p-3 text-sm dark:bg-surface-900">
                            <div class="mb-2 font-semibold">Ritmo vs soglie tempo</div>
                            <div class="flex flex-col gap-1 leading-tight">
                                <div>
                                    <span class="font-semibold">Avg</span>
                                    {{ formatSeconds(row.AvgHandSeconds) }} / {{ Number(securityFilterSetup.maxAvgSeconds).toFixed(1) }}s
                                    <span class="ml-1 rounded-full px-2 py-0.5 text-xs font-semibold" :class="getRiskPillClass(isAvgFast(row))">{{ getScorePoint(isAvgFast(row)) }}</span>
                                </div>
                                <div>
                                    <span class="font-semibold">Very fast</span>
                                    {{ formatSeconds(row.AvgHandSeconds) }} / {{ Number(securityFilterSetup.veryFastSeconds).toFixed(1) }}s
                                    <span class="ml-1 rounded-full px-2 py-0.5 text-xs font-semibold" :class="getRiskPillClass(isVeryFast(row))">{{ getScorePoint(isVeryFast(row)) }}</span>
                                </div>
                                <div class="text-xs text-muted-color">Ult {{ formatSeconds(row.LastHandDeltaSeconds) }} / {{ Number(securityFilterSetup.maxAvgSeconds).toFixed(1) }}s</div>
                                <div class="text-xs text-muted-color">Range missione {{ formatSeconds(row.MinHandDeltaSeconds) }} - {{ formatSeconds(row.MaxHandDeltaSeconds) }}</div>
                            </div>
                        </div>

                        <div class="rounded-xl bg-surface-0 p-3 text-sm dark:bg-surface-900">
                            <div class="mb-2 font-semibold">Streak e shoe vs soglia</div>
                            <div class="grid grid-cols-2 gap-3">
                                <div>
                                    <div class="text-xs text-muted-color">Streak</div>
                                    <div class="font-semibold">{{ row.CurrentStreak ?? 0 }} / {{ securityFilterSetup.minStreak }}</div>
                                    <span class="mt-1 inline-flex rounded-full px-2 py-0.5 text-xs font-semibold" :class="getRiskPillClass(isStreakRisk(row))">{{ getScorePoint(isStreakRisk(row)) }}</span>
                                </div>
                                <div>
                                    <div class="text-xs text-muted-color">Mano shoe</div>
                                    <div class="font-semibold">{{ row.LastShoeHand ?? '-' }} / {{ securityFilterSetup.maxShoeHand }}</div>
                                    <span class="mt-1 inline-flex rounded-full px-2 py-0.5 text-xs font-semibold" :class="getRiskPillClass(isShoeRisk(row))">{{ getScorePoint(isShoeRisk(row)) }}</span>
                                </div>
                            </div>
                        </div>

                        <div class="rounded-xl bg-surface-0 p-3 text-sm dark:bg-surface-900">
                            <div class="mb-2 font-semibold">Pausa</div>
                            <span :class="row.PauseBot ? 'font-semibold text-red-500' : 'text-muted-color'">
                                {{ row.PauseBot ? `Solo ${row.PauseComputer || row.Computer || row.computer}` : 'Nessuna' }}
                            </span>
                            <div class="text-xs text-muted-color mt-1">L6 prevenuti: {{ row.PreventedL6 ?? 0 }}</div>
                        </div>

                        <div class="rounded-xl bg-surface-0 p-3 text-sm dark:bg-surface-900">
                            <div class="mb-2 font-semibold">Frequenza L6</div>
                            <div class="flex flex-col gap-1 leading-tight">
                                <span><strong>{{ row.L6PlayedCount ?? 0 }}</strong> giocati</span>
                                <span class="text-xs text-muted-color">Ult {{ formatDuration(row.LastL6DeltaSeconds) }}</span>
                                <span class="text-xs text-muted-color">Avg {{ formatDuration(row.AvgL6DeltaSeconds) }}</span>
                                <span class="text-xs text-muted-color">Range {{ formatDurationRange(row.MinL6DeltaSeconds, row.MaxL6DeltaSeconds) }}</span>
                            </div>
                        </div>

                        <div class="rounded-xl bg-surface-0 p-3 text-sm dark:bg-surface-900 md:col-span-2 xl:col-span-2">
                            <div class="mb-2 font-semibold">Frequenza L8 auth</div>
                            <div class="grid grid-cols-1 gap-2 md:grid-cols-2">
                                <div class="flex flex-col gap-1 leading-tight">
                                    <span><strong>{{ row.AuthorizedL8LostCount ?? 0 }}</strong> L8 persi auth</span>
                                    <span class="text-xs text-muted-color">Delta persi: ult {{ formatDuration(row.LastAuthorizedL8LostDeltaSeconds) }}, avg {{ formatDuration(row.AvgAuthorizedL8LostDeltaSeconds) }}</span>
                                </div>
                                <div class="flex flex-col gap-1 leading-tight">
                                    <span class="text-xs text-muted-color">Auth -> L8: {{ formatDuration(row.LastAuthorizedL8LossFromAuthorizationSeconds) }}</span>
                                    <span class="text-xs text-muted-color">Range auth -> L8: {{ formatDurationRange(row.MinAuthorizedL8LossFromAuthorizationSeconds, row.MaxAuthorizedL8LossFromAuthorizationSeconds) }}</span>
                                    <span class="text-xs text-muted-color">Score auth: {{ Number(row.AuthorizedL8LostFromAuthorizationCount ?? 0) > 0 ? `${row.LastAuthorizedL8LossAuthorizationScore ?? 0}/4` : '-' }}</span>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
