<script setup>
import { computed, ref } from 'vue';

const props = defineProps({
    tableData: {
        type: Array,
        default: () => []
    },
    decisionMethod: {
        type: String,
        default: null
    }
});

const expandedRows = ref({});

function onRowExpand(event) {
    expandedRows.value = { [event.data.id]: true };
}

function onRowCollapse() {
    expandedRows.value = {};
}

// 🔹 Data source
const displayData = computed(() => {
    return (props.tableData ?? []).map((row) => ({
        ...row,
        id: `${row.account ?? ''}-${row.computer ?? ''}`
    }));
});

// =====================
// FORMATTERS
// =====================
const formatCurrency = (value) => {
    if (value === null || value === undefined) return '€0,00';

    let num = value;
    if (typeof num === 'string') {
        num = num.replace(',', '.');
    }

    num = Number(num);
    if (isNaN(num)) return '€0,00';

    return new Intl.NumberFormat('it-IT', {
        style: 'currency',
        currency: 'EUR'
    }).format(num);
};

const formatTime = (ore) => {
    if (!ore) return '-';

    if (typeof ore === 'number') {
        const totalMinutes = Math.round(ore * 60);
        const h = Math.floor(totalMinutes / 60);
        const m = totalMinutes % 60;
        return `${h.toString().padStart(2, '0')}:${m.toString().padStart(2, '0')}`;
    }

    if (typeof ore === 'string') {
        const num = Number(ore);
        if (!isNaN(num)) return formatTime(num);

        const parts = ore.split(':');
        if (parts.length >= 2) return `${parts[0]}:${parts[1]}`;
    }

    return ore;
};

const calculateMinutesPassed = (ore) => {
    if (!ore) return 0;

    if (typeof ore === 'number') return Math.round(ore * 60);

    if (typeof ore === 'string') {
        const num = Number(ore);
        if (!isNaN(num)) return Math.round(num * 60);

        const parts = ore.split(':');
        if (parts.length >= 2) {
            return (parseInt(parts[0]) || 0) * 60 + (parseInt(parts[1]) || 0);
        }
    }

    return 0;
};

// =====================
// CLASSI / SEVERITY
// =====================
const getMarginClass = (margin) => {
    if (margin > 5) return 'text-green-600 font-bold';
    if (margin > 0) return 'text-green-500';
    if (margin < -5) return 'text-red-600 font-bold';
    if (margin < 0) return 'text-red-500';
    return 'text-gray-500';
};

const getStatusSeverity = (status) => {
    const map = {
        RUNNING: 'success',
        ACTIVE: 'success',
        WARNING: 'warn',
        CRITICAL: 'danger',
        INACTIVE: 'secondary',
        STOPPED: 'secondary'
    };
    return map[status?.toUpperCase()] ?? 'info';
};

function parseServerUtcDate(value) {
    if (!value) return null;
    if (value instanceof Date) return value;
    if (typeof value !== 'string') return new Date(value);

    const normalized = /(?:Z|[+-]\d{2}:?\d{2})$/i.test(value) ? value : `${value}Z`;
    return new Date(normalized);
}

function isOnline(timestamp) {
    if (!timestamp) return false;
    const now = Date.now();
    const ts = parseServerUtcDate(timestamp)?.getTime();
    if (!ts || Number.isNaN(ts)) return false;
    return now - ts <= 300 * 1000;
}
const getLastAdviceField = (lastAdvice, field) => {
    if (!lastAdvice) return null;
    try {
        const adviceObj = JSON.parse(lastAdvice);
        return adviceObj[field] ?? null;
    } catch {
        return null;
    }
};
const parseTooltipJson = (tooltipJson) => {
    if (!tooltipJson) return {};
    try {
        return JSON.parse(tooltipJson);
    } catch {
        return {};
    }
};
</script>

<template>
    <div class="card">
        <div class="flex justify-between items-center mb-4">
            <div class="font-semibold text-xl">Decision method {{ decisionMethod }}, Report Bot</div>
            <small class="text-gray-500">{{ displayData.filter((e) => isOnline(e.dtUltimo)).length }} bot attivi</small>
        </div>

        <DataTable
            v-model:expandedRows="expandedRows"
            :value="displayData"
            dataKey="id"
            :paginator="true"
            :rows="10"
            :rowsPerPageOptions="[5, 10, 25]"
            paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
            currentPageReportTemplate="Showing {first} to {last} of {totalRecords} bots"
            responsiveLayout="scroll"
            stripedRows
            @rowExpand="onRowExpand"
            @rowCollapse="onRowCollapse"
        >
            <!-- Expand Colsumn -->
            <Column expander style="width: 3rem" />

            <Column field="computer" header="Computer" sortable style="min-width: 150px">
                <template #body="slotProps">
                    <div class="font-semibold flex items-center gap-2">
                        <!-- Cerchio stato online/offline use isOnline(slotProps.data.timestamp) -->
                        <i :class="['pi', isOnline(slotProps.data.dtUltimo) ? 'pi-circle-fill text-green-500' : 'pi-circle-fill text-gray-400', 'text-xs']"></i>
                        {{ slotProps.data.computer }}
                    </div>
                </template>
            </Column>
            <Column field="table" header="Tavolo" sortable style="min-width: 150px">
                <template #body="slotProps">
                    <div class="font-semibold flex items-center gap-2">
                        {{ slotProps.data.tavolo }}
                    </div>
                </template>
            </Column>

            <!-- Status -->
            <Column field="stato" header="Stato" sortable style="min-width: 120px" class="hidden md:table-cell">
                <template #body="slotProps">
                    <Tag :value="slotProps.data.stato" :severity="getStatusSeverity(slotProps.data.stato)" />
                </template>
            </Column>

            <!-- Margin (frozen) -->
            <Column field="margine" header="Margine" sortable frozen style="min-width: 120px">
                <template #body="slotProps">
                    <span :class="getMarginClass(slotProps.data.margine)">
                        {{ formatCurrency(slotProps.data.margine) }}
                    </span>
                </template>
            </Column>
            <!-- Margin (frozen) -->
            <Column field="mazzo" header="Mazzo" sortable frozen style="min-width: 120px" class="hidden md:table-cell">
                <template #body="slotProps">
                    <span>
                        {{ slotProps.data.mazzo }}
                    </span>
                </template>
            </Column>

            <Column field="pbt" header="PBT" sortable frozen style="min-width: 120px" class="hidden md:table-cell">
                <template #body="slotProps">
                    <span
                        :class="[
                            'd-flex p-1.5 rounded text-xs font-bold flex items-center justify-center w-fit',
                            slotProps.data.pbt === 'T' ? 'bg-green-800 text-white' : '',
                            slotProps.data.pbt === 'B' ? 'bg-red-800 text-white' : '',
                            slotProps.data.pbt === 'P' ? 'bg-blue-800 text-white' : '',
                            'border border-surface-300 dark:border-surface-700'
                        ]"
                    >
                        {{ slotProps.data.pbt }}
                    </span>
                </template>
            </Column>

            <!-- Saldo Iniziale -->
            <Column field="actioncode" header="Action Code:" sortable style="min-width: 140px" class="hidden lg:table-cell">
                <template #body="slotProps">
                    <span class="font-semibold">{{ slotProps.data.lastAction }}</span>
                </template>
            </Column>
            <!-- Saldo Iniziale -->
            <Column field="saldoIniziale" header="Saldo Iniziale" sortable style="min-width: 140px" class="hidden lg:table-cell">
                <template #body="slotProps">
                    <span class="font-semibold">{{ formatCurrency(slotProps.data.saldoIniziale) }}</span>
                </template>
            </Column>

            <!-- Saldo Istantaneo -->
            <Column field="saldoIstantaneo" header="Saldo Corrente" sortable style="min-width: 140px" class="hidden lg:table-cell">
                <template #body="slotProps">
                    <span class="font-semibold">{{ formatCurrency(slotProps.data.saldoIstantaneo) }}</span>
                </template>
            </Column>
            <Column field="colpoMartingala" header="Martingala" style="min-width: 120px" class="hidden xl:table-cell">
                <template #body="slotProps">
                    <div>
                        <Tag :value="parseInt(slotProps.data.colpoMartingala)" severity="info" />
                        <div class="mt-1">
                            <Tag :value="slotProps.data.reason" :severity="slotProps.data.reason === 'Default' ? 'success' : 'warn'" rounded />
                        </div>
                    </div>
                </template>
            </Column>

            <Column field="valoreGiocato" header="Valore Giocato" sortable style="min-width: 140px" class="hidden xl:table-cell">
                <template #body="slotProps">
                    <span class="text-primary font-semibold">{{ formatCurrency(slotProps.data.valoreGiocato) }}</span>
                </template>
            </Column>

            <Column field="chosenColor" header="Colore Giocato" sortable frozen style="min-width: 120px" class="hidden md:table-cell">
                <template #body="slotProps">
                    <span
                        :class="[
                            'd-flex p-1.5 rounded text-xs font-bold flex items-center justify-center w-fit',
                            slotProps.data.chosenColor === 'T' ? 'bg-green-800 text-white' : '',
                            slotProps.data.chosenColor === 'B' ? 'bg-red-800 text-white' : '',
                            slotProps.data.chosenColor === 'P' ? 'bg-blue-800 text-white' : '',
                            'border border-surface-300 dark:border-surface-700'
                        ]"
                    >
                        {{ slotProps.data.colore }}
                    </span>
                </template>
            </Column>

            <!-- Time -->
            <Column field="ore" header="Ore" sortable style="min-width: 100px" class="hidden md:table-cell">
                <template #body="slotProps">
                    <span class="font-semibold">{{ formatTime(slotProps.data.ore) }}</span>
                </template>
            </Column>

            <!-- Expanded Row Template -->
            <template #expansion="slotProps">
                <div class="p-2 bg-surface-50 dark:bg-surface-800">
                    <div class="mb-4 pb-3 border-b border-surface-200 dark:border-surface-700">
                        <h5 class="text-xl font-bold m-0 flex items-center gap-2">
                            <i class="pi pi-info-circle"></i>
                            Dettagli Completi - {{ slotProps.data.computer }} / {{ slotProps.data.account }}
                        </h5>
                    </div>

                    <div class="grid grid-cols-12 gap-4">
                        <!-- Informazioni Principali -->
                        <div class="col-span-12 lg:col-span-4">
                            <div class="card mb-0 h-full">
                                <h6 class="font-semibold mb-3 text-primary flex items-center gap-2">
                                    <i class="pi pi-id-card"></i>
                                    Informazioni Principali
                                </h6>
                                <div class="space-y-2 text-sm">
                                    <div class="flex justify-between py-1 border-b border-surface-100 dark:border-surface-700">
                                        <span class="text-muted-color">Minuti Passati:</span>
                                        <span class="font-semibold">{{ calculateMinutesPassed(slotProps.data.ore) }}</span>
                                    </div>
                                    <div class="flex justify-between py-1 border-b border-surface-100 dark:border-surface-700">
                                        <span class="text-muted-color">Tavolo:</span>
                                        <span class="font-semibold">{{ slotProps.data.tavolo || '-' }}</span>
                                    </div>
                                    <div class="flex justify-between py-1 border-b border-surface-100 dark:border-surface-700">
                                        <span class="text-muted-color">Mazzo:</span>
                                        <span class="font-semibold">{{ slotProps.data.mazzo || '-' }}</span>
                                    </div>
                                    <div class="flex justify-between py-1 border-b border-surface-100 dark:border-surface-700">
                                        <span class="text-muted-color">Stato:</span>
                                        <Tag :value="slotProps.data.stato" :severity="getStatusSeverity(slotProps.data.stato)" />
                                    </div>
                                    <div class="flex justify-between py-1">
                                        <span class="text-muted-color">Ore:</span>
                                        <span class="font-semibold">{{ formatTime(slotProps.data.ore) }}</span>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <!-- Bilanci e Valori -->
                        <div class="col-span-12 lg:col-span-4">
                            <div class="card mb-0 h-full">
                                <h6 class="font-semibold mb-3 text-green-600 flex items-center gap-2">
                                    <i class="pi pi-wallet"></i>
                                    Bilanci e Puntate
                                </h6>
                                <div class="space-y-2 text-sm">
                                    <div class="flex justify-between py-1 border-b border-surface-100 dark:border-surface-700">
                                        <span class="text-muted-color">Saldo Iniziale:</span>
                                        <span class="font-bold text-blue-600">{{ formatCurrency(slotProps.data.saldoIniziale) }}</span>
                                    </div>
                                    <div class="flex justify-between py-1 border-b border-surface-100 dark:border-surface-700">
                                        <span class="text-muted-color">Saldo Istantaneo:</span>
                                        <span class="font-bold text-green-600">{{ formatCurrency(slotProps.data.saldoIstantaneo) }}</span>
                                    </div>
                                    <div class="flex justify-between py-1 border-b border-surface-100 dark:border-surface-700">
                                        <span class="text-muted-color">Margine:</span>
                                        <span :class="getMarginClass(slotProps.data.margine)" class="text-lg">
                                            {{ formatCurrency(slotProps.data.margine) }}
                                        </span>
                                    </div>
                                    <div class="flex justify-between py-1 border-b border-surface-100 dark:border-surface-700">
                                        <span class="text-muted-color">Media/Ora:</span>
                                        <span class="font-semibold">{{ formatCurrency(slotProps.data.mediaOra) }}/ ora</span>
                                    </div>
                                    <div class="flex justify-between py-1 border-b border-surface-100 dark:border-surface-700">
                                        <span class="text-muted-color">Valore Giocato:</span>
                                        <span class="font-semibold">{{ formatCurrency(slotProps.data.valoreGiocato) }}</span>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <!-- Strategia e Predizioni -->
                        <div class="col-span-12 lg:col-span-4">
                            <div class="card mb-0 h-full">
                                <h6 class="font-semibold mb-3 text-orange-600 flex items-center gap-2">
                                    <i class="pi pi-chart-line"></i>
                                    Strategia e Analisi
                                </h6>
                                <div class="space-y-2 text-sm">
                                    <div class="flex justify-between py-1 border-b border-surface-100 dark:border-surface-700">
                                        <span class="text-muted-color">Martingala:</span>
                                        <Tag :value="slotProps.data.colpoMartingala ?? slotProps.data.martingale ?? '-'" severity="info" />
                                    </div>

                                    <div class="flex justify-between py-1 border-b border-surface-100 dark:border-surface-700">
                                        <span class="text-muted-color">Stop L6:</span>
                                        <Tag :value="getLastAdviceField(slotProps.data.lastAdvice, 'StopL6') ? 'Sì' : 'No'" :severity="getLastAdviceField(slotProps.data.lastAdvice, 'StopL6') ? 'danger' : 'success'" />
                                    </div>
                                    <!-- Nuovi campi da lastAdvice -->
                                    <div class="flex justify-between py-1 border-b border-surface-100 dark:border-surface-700">
                                        <span class="text-muted-color">Stato Tavolo:</span>
                                        <span class="font-semibold">{{ getLastAdviceField(slotProps.data.lastAdvice, 'State') || '-' }}</span>
                                    </div>
                                    <div class="flex justify-between py-1 border-b border-surface-100 dark:border-surface-700">
                                        <span class="text-muted-color">Martingala:</span>
                                        <span class="font-semibold">{{ getLastAdviceField(slotProps.data.lastAdvice, 'Martingala') ?? '-' }}</span>
                                    </div>
                                    <div class="flex justify-between py-1 border-b border-surface-100 dark:border-surface-700">
                                        <span class="text-muted-color">Margine Locale:</span>
                                        <span class="font-semibold">{{ formatCurrency(getLastAdviceField(slotProps.data.lastAdvice, 'LocalMargin')) }}</span>
                                    </div>
                                    <div class="flex justify-between py-1 border-b border-surface-100 dark:border-surface-700">
                                        <span class="text-muted-color">Margine Globale:</span>
                                        <span class="font-semibold">{{ formatCurrency(getLastAdviceField(slotProps.data.lastAdvice, 'GlobalMargin')) }}</span>
                                    </div>
                                    <div class="flex justify-between py-1 border-b border-surface-100 dark:border-surface-700">
                                        <span class="text-muted-color">Minuti Trascorsi:</span>
                                        <span class="font-semibold">{{ getLastAdviceField(slotProps.data.lastAdvice, 'Elapsed') != null ? calculateMinutesPassed(getLastAdviceField(slotProps.data.lastAdvice, 'Elapsed')) : '-' }}</span>
                                    </div>
                                    <div class="flex justify-between py-1 border-b border-surface-100 dark:border-surface-700">
                                        <span class="text-muted-color">HotZone:</span>
                                        <span class="font-semibold">{{ getLastAdviceField(slotProps.data.lastAdvice, 'HotZone') ? 'Sì' : 'No' }}</span>
                                    </div>
                                    <div class="flex justify-between py-1 border-b border-surface-100 dark:border-surface-700">
                                        <span class="text-muted-color">HotZone Label:</span>
                                        <span class="font-semibold">{{ getLastAdviceField(slotProps.data.lastAdvice, 'HotZoneLabel') || '-' }}</span>
                                    </div>
                                    <div class="flex justify-between py-1 border-b border-surface-100 dark:border-surface-700">
                                        <span class="text-muted-color">GlobalAuthL6Counter:</span>
                                        <span class="font-semibold">{{ getLastAdviceField(slotProps.data.lastAdvice, 'GlobalAuthL6Counter') ?? '-' }}</span>
                                    </div>
                                    <div class="flex justify-between py-1 border-b border-surface-100 dark:border-surface-700">
                                        <span class="text-muted-color">GlobalL5Loss:</span>
                                        <span class="font-semibold">{{ getLastAdviceField(slotProps.data.lastAdvice, 'GlobalL5Loss') ?? '-' }}</span>
                                    </div>
                                    <div class="flex justify-between py-1 border-b border-surface-100 dark:border-surface-700">
                                        <span class="text-muted-color">GlobalPBHandsPlayed:</span>
                                        <span class="font-semibold">{{ getLastAdviceField(slotProps.data.lastAdvice, 'GlobalPBHandsPlayed') ?? '-' }}</span>
                                    </div>
                                    <div class="flex justify-between py-1 border-b border-surface-100 dark:border-surface-700">
                                        <span class="text-muted-color">GlobalPauseScalping:</span>
                                        <span class="font-semibold">{{ getLastAdviceField(slotProps.data.lastAdvice, 'GlobalPauseScalping') ? 'Sì' : 'No' }}</span>
                                    </div>
                                    <div class="flex justify-between py-1 border-b border-surface-100 dark:border-surface-700">
                                        <span class="text-muted-color">GlobalPauseScalpingDuration:</span>
                                        <span class="font-semibold">{{ getLastAdviceField(slotProps.data.lastAdvice, 'GlobalPauseScalpingDuration') ?? '-' }}</span>
                                    </div>
                                    <!-- Security Filter -->
                                    <div class="flex justify-between py-1 border-b border-surface-100 dark:border-surface-700">
                                        <span class="text-muted-color">Security Filter:</span>
                                        <Tag
                                            :value="getLastAdviceField(slotProps.data.lastAdvice, 'SecurityFilterActive') ? `ATTIVO [${getLastAdviceField(slotProps.data.lastAdvice, 'SecurityRiskScore')}/4]` : `OFF [${getLastAdviceField(slotProps.data.lastAdvice, 'SecurityRiskScore') ?? 0}/4]`"
                                            :severity="getLastAdviceField(slotProps.data.lastAdvice, 'SecurityFilterActive') ? 'danger' : 'secondary'"
                                        />
                                    </div>
                                    <div class="flex justify-between py-1 border-b border-surface-100 dark:border-surface-700">
                                        <span class="text-muted-color">Streak / Avg / Delta:</span>
                                        <span class="font-semibold text-sm">
                                            {{ getLastAdviceField(slotProps.data.lastAdvice, 'CurrentStreak') ?? '-' }}
                                            /
                                            {{ getLastAdviceField(slotProps.data.lastAdvice, 'AvgHandSeconds') != null ? Number(getLastAdviceField(slotProps.data.lastAdvice, 'AvgHandSeconds')).toFixed(1) + 's' : '-' }}
                                            /
                                            {{ getLastAdviceField(slotProps.data.lastAdvice, 'LastHandDeltaSeconds') != null ? Number(getLastAdviceField(slotProps.data.lastAdvice, 'LastHandDeltaSeconds')).toFixed(1) + 's' : '-' }}
                                        </span>
                                    </div>
                                    <div class="flex justify-between py-1 border-b border-surface-100 dark:border-surface-700">
                                        <span class="text-muted-color">Motivo:</span>
                                        <span class="font-semibold">
                                            <Tag :value="slotProps.data.reason" :severity="slotProps.data.reason === 'Default' ? 'success' : 'warn'" rounded />
                                        </span>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <!-- Timing e Attività -->
                        <div class="col-span-12 lg:col-span-6">
                            <div class="card mb-0">
                                <h6 class="font-semibold mb-3 text-purple-600 flex items-center gap-2">
                                    <i class="pi pi-clock"></i>
                                    Timing e Attività
                                </h6>
                                <div class="space-y-2 text-sm">
                                    <div class="flex justify-between py-1 border-b border-surface-100 dark:border-surface-700">
                                        <span class="text-muted-color">Ore Attività:</span>
                                        <span class="font-bold text-purple-600">{{ slotProps.data.ore }}</span>
                                    </div>
                                    <div class="flex justify-between py-1 border-b border-surface-100 dark:border-surface-700">
                                        <span class="text-muted-color">Minuti Passati:</span>
                                        <span class="font-semibold">{{ calculateMinutesPassed(slotProps.data.ore) }} min</span>
                                    </div>
                                    <div class="flex justify-between py-1">
                                        <span class="text-muted-color">PBT:</span>
                                        <span class="flex gap-1">
                                            <template v-if="slotProps.data.pbt">
                                                <span
                                                    :class="[
                                                        'd-flex p-1.5 rounded text-xs font-bold flex items-center justify-center',
                                                        slotProps.data.pbt === 'T' ? 'bg-green-800 text-white' : '',
                                                        slotProps.data.pbt === 'B' ? 'bg-red-800 text-white' : '',
                                                        slotProps.data.pbt === 'P' ? 'bg-blue-800 text-white' : '',
                                                        'border border-surface-300 dark:border-surface-700'
                                                    ]"
                                                >
                                                    {{ slotProps.data.pbt }}
                                                </span>
                                            </template>
                                            <template v-else>
                                                <span class="font-semibold">-</span>
                                            </template>
                                        </span>
                                    </div>
                                    <div class="flex justify-between py-1">
                                        <span class="text-muted-color">PBT History:</span>
                                        <span class="flex gap-1">
                                            <template v-if="Array.isArray(slotProps.data.pbtHistory) && slotProps.data.pbtHistory.length">
                                                <span
                                                    v-for="(item, idx) in slotProps.data.pbtHistory"
                                                    :key="idx"
                                                    :class="[
                                                        'd-flex p-1.5 rounded text-xs font-bold flex items-center justify-center',
                                                        item === 'T' ? 'bg-green-800 text-white' : '',
                                                        item === 'B' ? 'bg-red-800 text-white' : '',
                                                        item === 'P' ? 'bg-blue-800 text-white' : '',
                                                        'border border-surface-300 dark:border-surface-700'
                                                    ]"
                                                >
                                                    {{ item }}
                                                </span>
                                            </template>
                                            <template v-else>
                                                <span class="font-semibold">-</span>
                                            </template>
                                        </span>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <!-- Info Aggiuntive -->
                        <div class="col-span-12 lg:col-span-6">
                            <div class="card mb-0">
                                <h6 class="font-semibold mb-3 text-cyan-600 flex items-center gap-2">
                                    <i class="pi pi-info"></i>
                                    Informazioni Aggiuntive
                                </h6>
                                <div class="space-y-2 text-sm">
                                    <div class="py-1 space-y-1">
                                        <template v-if="slotProps.data.lastAdvice && getLastAdviceField(slotProps.data.lastAdvice, 'ToolTipJson')">
                                            <template v-for="(value, key) in parseTooltipJson(getLastAdviceField(slotProps.data.lastAdvice, 'ToolTipJson'))" :key="key">
                                                <span class="text-muted-color block mb-1 mt-2">{{ key.replace('_', ' ') }}</span>
                                                <span class="font-semibold">{{ value }}</span>
                                            </template>
                                        </template>
                                        <template v-else>
                                            <span class="text-xs text-red-500">Tooltip non disponibile</span>
                                        </template>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <!-- Screenshot -->
                        <div class="col-span-12" v-if="slotProps.data.image">
                            <div class="card mb-0">
                                <h6 class="font-semibold mb-3 flex items-center gap-2">
                                    <i class="pi pi-image"></i>
                                    Screenshot Bot
                                </h6>
                                <img :src="slotProps.data.image" alt="Bot screenshot" class="w-full max-w-4xl mx-auto rounded-lg shadow-lg border-2 border-surface-200 dark:border-surface-700" />
                            </div>
                        </div>
                    </div>
                </div>
            </template>
        </DataTable>
    </div>
</template>

<style scoped>
@media screen and (max-width: 640px) {
    :deep(.p-datatable .p-datatable-tbody > tr.p-datatable-row-expansion > td) {
        padding: 0;
    }

    .card {
        padding: 1rem;
    }
}
</style>
