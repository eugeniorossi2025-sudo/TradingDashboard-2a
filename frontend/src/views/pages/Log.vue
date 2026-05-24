<script setup>
import { FinancialReportService } from '@/service/FinancialReportService';
import { LogService } from '@/service/LogService';
import { FilterMatchMode } from '@primevue/core/api';
import { InputGroup } from 'primevue';
import Button from 'primevue/button';
import DatePicker from 'primevue/datepicker';
import InputText from 'primevue/inputtext';
import { computed, onMounted, ref } from 'vue';

const logs = ref([]);
const totalRecords = ref(0);
const page = ref(1);
const pageSize = ref(10);
const showResetDialog = ref(false);
const resetLoading = ref(false);
const expandedRows = ref({});
const dt = ref();
const filters = ref({
    global: { value: null, matchMode: FilterMatchMode.CONTAINS }
});

const from = ref('');
const to = ref('');
const pc = ref('');
const action = ref('');
const description = ref();
const reportFrom = ref(new Date(new Date().getFullYear(), new Date().getMonth(), 1));
const reportTo = ref(new Date());
const reportRuntimeMode = ref('Production');
const reportLoading = ref(false);
const missionReports = ref([]);
const missionReportsTotal = ref(0);
const missionReportsLoading = ref(false);
const missionReportSkip = ref(0);
const missionReportLimit = ref(100);
const historicalImportFile = ref(null);
const historicalImportReplace = ref(false);
const historicalImportLoading = ref(false);
const historicalImportResult = ref(null);
const reportModeOptions = [
    { label: 'Production', value: 'Production' },
    { label: 'Demo', value: 'Demo' }
];

onMounted(() => {
    fetchLogs();
    loadMissionReports();
});

async function fetchLogs() {
    const res = await LogService.getLogs(from.value || undefined, to.value || undefined, pc.value || undefined, action.value || undefined, description.value || undefined, page.value, pageSize.value);
    logs.value = res.items || [];
    totalRecords.value = res.totalCount || 0;
}

function onPageChange(event) {
    page.value = event.page + 1;
    pageSize.value = event.rows;
    fetchLogs();
}

// Aggiorna la variabile 'first' per DataTable
const first = computed(() => (page.value - 1) * pageSize.value);

async function deleteLogs() {
    resetLoading.value = true;
    try {
        await LogService.resetLogs();
        logs.value = [];
    } catch (error) {
        console.error('Error deleting logs:', error);
    } finally {
        resetLoading.value = false;
        showResetDialog.value = false;
    }
}

function onSearch() {
    fetchLogs();
}

function formatDateParam(value) {
    const date = value instanceof Date ? value : new Date(value);
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
}

function getReportRange() {
    return {
        from: formatDateParam(reportFrom.value),
        to: formatDateParam(reportTo.value),
        mode: reportRuntimeMode.value === 'Demo' ? 'Demo' : 'Production'
    };
}

async function openFinancialReport() {
    reportLoading.value = true;
    try {
        const range = getReportRange();
        await FinancialReportService.openHtmlReport(range.mode, range.from, range.to);
    } finally {
        reportLoading.value = false;
    }
}

async function downloadFinancialJson() {
    reportLoading.value = true;
    try {
        const range = getReportRange();
        await FinancialReportService.downloadJson(range.mode, range.from, range.to);
    } finally {
        reportLoading.value = false;
    }
}

async function downloadFinancialCsv() {
    reportLoading.value = true;
    try {
        const range = getReportRange();
        await FinancialReportService.downloadCsv(range.mode, range.from, range.to);
    } finally {
        reportLoading.value = false;
    }
}

async function loadMissionReports() {
    missionReportsLoading.value = true;
    try {
        const range = getReportRange();
        const response = await FinancialReportService.getReportsIndex(range.mode, range.from, range.to, missionReportSkip.value, missionReportLimit.value);
        missionReports.value = response.items || [];
        missionReportsTotal.value = response.total || 0;
    } finally {
        missionReportsLoading.value = false;
    }
}

function openMissionSession(sessionId, format) {
    FinancialReportService.openSessionReport(sessionId, format);
}

function onHistoricalImportFileChange(event) {
    historicalImportFile.value = event.target.files?.[0] || null;
    historicalImportResult.value = null;
}

async function importHistoricalDemo() {
    if (!historicalImportFile.value) return;
    historicalImportLoading.value = true;
    try {
        historicalImportResult.value = await FinancialReportService.importHistoricalDemo(historicalImportFile.value, historicalImportReplace.value);
        reportRuntimeMode.value = 'Demo';
        await loadMissionReports();
    } finally {
        historicalImportLoading.value = false;
    }
}

function formatMoney(value) {
    const amount = Number(value || 0);
    const sign = amount > 0 ? '+' : amount < 0 ? '-' : '';
    return `${sign}${Math.abs(amount).toFixed(2)} €`;
}

async function onExportCSV() {
    // Chiamata fetchLogs con pageSize -1 per ottenere tutti i dati
    const res = await LogService.getLogs(from.value || undefined, to.value || undefined, pc.value || undefined, action.value || undefined, description.value || undefined, 1, 1000000000);
    const exportLogs = res.items || [];
    // Genera CSV dai dati
    exportCSV(exportLogs);
}

function exportCSV(data) {
    if (!data.length) return;
    // Intestazioni
    const headers = ['ID', 'Datetime', 'Descrizione', 'Action Code', 'PC'];
    // Righe
    const rows = data.map((log) => [log.id, formatLocalDate(log.createdAt), log.description || '-', log.action, log.category || '-']);
    // CSV string
    const csvContent = [headers, ...rows].map((e) => e.map((v) => `"${String(v).replace(/"/g, '""')}"`).join(',')).join('\n');
    // Download
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.setAttribute('download', `logs_export_${new Date().toISOString().slice(0, 10)}.csv`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

function formatLocalDate(date) {
    const dateObj = new Date(date);
    // Correggi l'offset se la data è in UTC
    const localDate = new Date(dateObj.getTime() - dateObj.getTimezoneOffset() * 60000);
    return localDate.toLocaleString('it-IT', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
}
</script>
<template>
    <div>
        <div class="card mb-4">
            <div class="flex flex-col gap-4">
                <div>
                    <h4 class="m-0 text-lg">Report finanziario</h4>
                    <p class="text-muted-color mt-2 mb-0">Seleziona periodo e modalità contabile per aprire il report stampabile come PDF.</p>
                </div>

                <div class="grid grid-cols-1 md:grid-cols-5 gap-3 items-end">
                    <div class="flex flex-col gap-2">
                        <label class="font-semibold">Dal</label>
                        <DatePicker v-model="reportFrom" dateFormat="yy-mm-dd" showIcon fluid />
                    </div>
                    <div class="flex flex-col gap-2">
                        <label class="font-semibold">Al</label>
                        <DatePicker v-model="reportTo" dateFormat="yy-mm-dd" showIcon fluid />
                    </div>
                    <div class="flex flex-col gap-2">
                        <label class="font-semibold">Modalità</label>
                        <Select v-model="reportRuntimeMode" :options="reportModeOptions" optionLabel="label" optionValue="value" fluid />
                    </div>
                    <Button label="Carica archivio" icon="pi pi-refresh" severity="secondary" outlined :loading="missionReportsLoading" @click="loadMissionReports" />
                    <Button label="Apri report" icon="pi pi-external-link" :loading="reportLoading" @click="openFinancialReport" />
                    <div class="flex gap-2">
                        <Button label="JSON" icon="pi pi-download" severity="secondary" outlined class="flex-1" :disabled="reportLoading" @click="downloadFinancialJson" />
                        <Button label="CSV" icon="pi pi-download" severity="secondary" outlined class="flex-1" :disabled="reportLoading" @click="downloadFinancialCsv" />
                    </div>
                </div>
            </div>
        </div>

        <div class="card mb-4">
            <div class="flex flex-col gap-4">
                <div>
                    <h4 class="m-0 text-lg">Import storico Demo</h4>
                    <p class="text-muted-color mt-2 mb-0">Importa CSV/Excel storico come backup Demo: una missione per giorno, senza toccare Production.</p>
                </div>

                <div class="grid grid-cols-1 md:grid-cols-4 gap-3 items-end">
                    <div class="flex flex-col gap-2 md:col-span-2">
                        <label class="font-semibold">File CSV/Excel log storico</label>
                        <input type="file" accept=".csv,.txt,.xlsx,.xls" class="p-inputtext p-component" @change="onHistoricalImportFileChange" />
                    </div>
                    <label class="flex items-center gap-2">
                        <input v-model="historicalImportReplace" type="checkbox" />
                        <span>Replace esplicito giorni già importati</span>
                    </label>
                    <Button label="Importa Demo" icon="pi pi-upload" severity="secondary" :loading="historicalImportLoading" :disabled="!historicalImportFile" @click="importHistoricalDemo" />
                </div>

                <div v-if="historicalImportResult" class="rounded border border-surface-200 dark:border-surface-700 p-3 text-sm">
                    <div><strong>Runtime:</strong> {{ historicalImportResult.runtimeMode }}</div>
                    <div><strong>Righe lette:</strong> {{ historicalImportResult.totalRows }}</div>
                    <div><strong>Giorni importati:</strong> {{ historicalImportResult.imported }}</div>
                    <div><strong>Giorni saltati:</strong> {{ historicalImportResult.skipped }}</div>
                    <div v-if="historicalImportResult.skippedDays?.length" class="text-muted-color mt-2">Saltati: {{ historicalImportResult.skippedDays.join(', ') }}</div>
                </div>
            </div>
        </div>

        <div class="card mb-4">
            <div class="flex flex-col gap-4">
                <div class="flex flex-col md:flex-row md:items-center gap-2">
                    <div>
                        <h4 class="m-0 text-lg">Archivio Rapporti Missione</h4>
                        <p class="text-muted-color mt-2 mb-0">Dataset contabile separato dai log runtime: sessioni, margini, mani reali e tavoli.</p>
                    </div>
                    <div class="md:ml-auto text-sm text-muted-color">Totale: {{ missionReportsTotal }}</div>
                </div>

                <DataTable :value="missionReports" :loading="missionReportsLoading" dataKey="sessionId" responsiveLayout="scroll" breakpoint="960px">
                    <Column field="sessionId" header="Sessione" :style="{ width: '110px' }">
                        <template #body="{ data }">
                            <strong>#{{ data.sessionId }}</strong>
                        </template>
                    </Column>
                    <Column field="startUtc" header="Start">
                        <template #body="{ data }">
                            {{ formatLocalDate(data.startUtc) }}
                        </template>
                    </Column>
                    <Column field="endUtc" header="End">
                        <template #body="{ data }">
                            {{ data.endUtc ? formatLocalDate(data.endUtc) : '-' }}
                        </template>
                    </Column>
                    <Column field="runtimeMode" header="Mode" />
                    <Column field="totalMarginEuro" header="Margin €">
                        <template #body="{ data }">
                            <span :class="Number(data.totalMarginEuro) >= 0 ? 'text-green-500' : 'text-red-500'">{{ formatMoney(data.totalMarginEuro) }}</span>
                        </template>
                    </Column>
                    <Column field="realHandsCount" header="Mani reali" />
                    <Column field="activeTables" header="Tavoli" />
                    <Column field="samplesCount" header="Samples" />
                    <Column header="Apri">
                        <template #body="{ data }">
                            <div class="flex gap-2">
                                <Button label="HTML" size="small" severity="secondary" outlined @click="openMissionSession(data.sessionId, 'html')" />
                                <Button label="JSON" size="small" severity="secondary" outlined @click="openMissionSession(data.sessionId, 'json')" />
                                <Button label="CSV" size="small" severity="secondary" outlined @click="openMissionSession(data.sessionId, 'csv')" />
                            </div>
                        </template>
                    </Column>
                    <template #empty>
                        <div class="text-center py-6 text-muted-color">Nessuna missione contabile trovata per il periodo selezionato.</div>
                    </template>
                </DataTable>
            </div>
        </div>

        <div class="card">
            <div class="mb-4">
                <h4 class="m-0 text-lg">Log runtime tecnici</h4>
                <p class="text-muted-color mt-2 mb-0">Supporto operativo separato dai report finanziari. Queste righe non entrano nei PDF contabili.</p>
            </div>
            <Toolbar class="mb-6">
                <template #start>
                    <div class="flex flex-col sm:flex-row gap-2">
                        <Button label="Delete Logs" icon="pi pi-trash" severity="danger" @click="showResetDialog = true" class="w-full sm:w-auto" />
                        <Dialog v-model:visible="showResetDialog" :closable="!resetLoading" :modal="true" :dismissableMask="!resetLoading" :style="{ width: '350px' }">
                            <template #header>
                                <span>Conferma Reset</span>
                            </template>
                            <div class="mb-4">Sei sicuro di voler cancellare i log?</div>
                            <div class="flex justify-end gap-2">
                                <button class="p-button p-component" @click="showResetDialog = false" :disabled="resetLoading">Annulla</button>
                                <button class="p-button p-component p-button-danger" @click="deleteLogs" :disabled="resetLoading">
                                    <span v-if="resetLoading" class="pi pi-spin pi-spinner mr-2"></span>
                                    <span v-else class="pi pi-check mr-2"></span>
                                    Conferma
                                </button>
                            </div>
                        </Dialog>
                    </div>
                </template>

                <template #end>
                    <Button label="Export" icon="pi pi-upload" severity="secondary" @click="onExportCSV()" class="w-full sm:w-auto" />
                </template>
            </Toolbar>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4" :style="{ margin: '20px 0px' }">
                <!-- Date range -->
                <InputGroup class="flex flex-col md:flex-row gap-2">
                    <DatePicker v-model="from" dateFormat="yyyy-MM-dd HH:mm" showTime showIcon placeholder="From" :style="{ width: '100%' }" />
                    <DatePicker v-model="to" dateFormat="yyyy-MM-dd HH:mm" showTime showIcon placeholder="To" :style="{ width: '100%' }" />
                </InputGroup>

                <!-- PC + Description -->
                <InputGroup class="flex flex-col md:flex-row gap-2">
                    <InputText v-model="pc" placeholder="PC" :style="{ width: '100%' }" />
                    <InputNumber v-model="action" placeholder="Action" :style="{ width: '100%' }" />
                    <InputText v-model="description" placeholder="Description" :style="{ width: '100%' }" />
                    <Button label="Search" icon="pi pi-search" @click="onSearch" class="p-button-sm w-full md:w-auto" :style="{ width: '100%' }" />
                </InputGroup>
            </div>

            <DataTable
                ref="dt"
                :value="logs"
                dataKey="id"
                :paginator="true"
                :rows="pageSize"
                :totalRecords="totalRecords"
                :first="first"
                :filters="filters"
                v-model:expandedRows="expandedRows"
                responsiveLayout="scroll"
                breakpoint="960px"
                paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
                :rowsPerPageOptions="[5, 10, 25]"
                currentPageReportTemplate="Showing {first} to {last} of {totalRecords} Logs"
                @page="onPageChange"
                :lazy="true"
            >
                <Column field="id" header="ID" sortable :style="{ width: '100px' }">
                    <template #body="{ data }">
                        {{ data.id }}
                    </template>
                </Column>
                <Column field="createdAt" header="Datetime" sortable :style="{ minWidth: '180px' }">
                    <template #body="{ data }">
                        {{ formatLocalDate(data.createdAt) }}
                    </template>
                </Column>

                <Column field="description" header="Descrizione" :style="{ minWidth: '150px' }">
                    <template #body="{ data }">
                        <span style="white-space: pre">
                            {{ data.description || '-' }}
                        </span>
                    </template>
                </Column>

                <Column field="action" header="Action Code" :style="{ minWidth: '150px' }">
                    <template #body="{ data }">
                        <span class="line-clamp-1">{{ data.action }}</span>
                    </template>
                </Column>
                <Column field="category" header="PC" :style="{ minWidth: '150px' }">
                    <template #body="{ data }">
                        <span class="line-clamp-1">{{ data.category || '-' }}</span>
                    </template>
                </Column>

                <template #empty>
                    <div class="text-center py-6">
                        <i class="pi pi-inbox text-4xl text-gray-400 mb-3"></i>
                        <p class="text-gray-500">No logs found.</p>
                    </div>
                </template>
            </DataTable>
        </div>
    </div>
</template>

<style scoped>
.line-clamp-1 {
    display: -webkit-box;
    -webkit-line-clamp: 1;
    line-clamp: 1;
    -webkit-box-orient: vertical;
    overflow: hidden;
    text-overflow: ellipsis;
}

@media (max-width: 640px) {
    :deep(.p-datatable-table) {
        font-size: 0.875rem;
    }

    :deep(.p-datatable-header-cell),
    :deep(.p-datatable-tbody > tr > td) {
        padding: 0.5rem;
        white-space: pre;
    }
}

@media (min-width: 641px) and (max-width: 1024px) {
    :deep(.p-datatable-table) {
        font-size: 0.9375rem;
    }
}

@media (max-width: 768px) {
    :deep(.hidden.md\\:table-cell) {
        display: none !important;
    }
}

@media (max-width: 1024px) {
    :deep(.hidden.lg\\:table-cell) {
        display: none !important;
    }
}

@media (min-width: 769px) {
    :deep(.hidden.md\\:table-cell) {
        display: table-cell !important;
    }
}

@media (min-width: 1025px) {
    :deep(.hidden.lg\\:table-cell) {
        display: table-cell !important;
    }
}
</style>
