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

function formatSeconds(value) {
    if (value == null || Number(value) <= 0) return '-';
    return `${Number(value).toFixed(1)}s`;
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

    <div class="col-span-12" v-if="securityFilterRows.length">
        <div class="card">
            <div class="flex justify-between items-center mb-4">
                <span class="block text-muted-color font-medium">Security Filter per bot</span>
                <span class="text-sm text-muted-color">{{ securityFilterRows.length }} bot</span>
            </div>
            <div class="overflow-x-auto">
                <table class="w-full text-sm">
                    <thead>
                        <tr class="text-left text-muted-color">
                            <th class="py-2 pr-3">Bot</th>
                            <th class="py-2 pr-3">Avg mano</th>
                            <th class="py-2 pr-3">Ultimo delta</th>
                            <th class="py-2 pr-3">Min delta missione</th>
                            <th class="py-2 pr-3">Max delta missione</th>
                            <th class="py-2 pr-3">Streak</th>
                            <th class="py-2 pr-3">Score</th>
                            <th class="py-2 pr-3">Filtro</th>
                            <th class="py-2 pr-3">Scope pausa</th>
                            <th class="py-2 pr-3">L6 prevenuti</th>
                            <th class="py-2 pr-3">Mano shoe</th>
                            <th class="py-2 pr-3">L</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="row in securityFilterRows" :key="row.computer" class="border-t border-surface-200 dark:border-surface-700 transition-colors" :class="getSecurityFilterRowClass(row)">
                            <td class="py-2 pr-3">
                                <span class="inline-flex items-center gap-2 rounded-full bg-primary-100 px-3 py-1 font-semibold text-primary-700 dark:bg-primary-900/30 dark:text-primary-300">
                                    <span class="h-2 w-2 rounded-full" :class="row.SecurityFilterActive ? 'bg-red-500' : 'bg-emerald-500'"></span>
                                    {{ row.Computer || row.computer }}
                                </span>
                            </td>
                            <td class="py-2 pr-3">{{ formatSeconds(row.AvgHandSeconds) }}</td>
                            <td class="py-2 pr-3">{{ formatSeconds(row.LastHandDeltaSeconds) }}</td>
                            <td class="py-2 pr-3">{{ formatSeconds(row.MinHandDeltaSeconds) }}</td>
                            <td class="py-2 pr-3">{{ formatSeconds(row.MaxHandDeltaSeconds) }}</td>
                            <td class="py-2 pr-3">{{ row.CurrentStreak ?? 0 }}</td>
                            <td class="py-2 pr-3">
                                <span class="inline-flex rounded-full px-2.5 py-1 text-xs font-semibold" :class="getScoreClass(row)">
                                    {{ row.SecurityRiskScore ?? 0 }}/4
                                </span>
                            </td>
                            <td class="py-2 pr-3">
                                <span class="inline-flex rounded-full px-2.5 py-1 text-xs font-semibold" :class="getSecurityFilterStatusClass(row)">
                                    {{ getSecurityFilterStatus(row) }}
                                </span>
                            </td>
                            <td class="py-2 pr-3">
                                <span :class="row.PauseBot ? 'font-semibold text-red-500' : 'text-muted-color'">
                                    {{ row.PauseBot ? `Solo ${row.PauseComputer || row.Computer || row.computer}` : 'Nessuna' }}
                                </span>
                            </td>
                            <td class="py-2 pr-3">{{ row.PreventedL6 ?? 0 }}</td>
                            <td class="py-2 pr-3">{{ row.LastShoeHand ?? '-' }}</td>
                            <td class="py-2 pr-3">{{ row.Martingala ?? '-' }}</td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    </div>
</template>
