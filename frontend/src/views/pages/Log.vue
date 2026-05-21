<script setup>
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
const resetLoading = ref(false)
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


onMounted(() => {
    fetchLogs();
});


async function fetchLogs() {
    const res = await LogService.getLogs(
        from.value || undefined,
        to.value || undefined,
        pc.value || undefined,
        action.value || undefined,
        description.value || undefined,
        page.value,
        pageSize.value
    );
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

async function onExportCSV() {
    // Chiamata fetchLogs con pageSize -1 per ottenere tutti i dati
    const res = await LogService.getLogs(
        from.value || undefined,
        to.value || undefined,
        pc.value || undefined,
        action.value || undefined,
        description.value || undefined,
        1,
        1000000000
    );
    const exportLogs = res.items || [];
    // Genera CSV dai dati
    exportCSV(exportLogs);
}

function exportCSV(data) {
    if (!data.length) return;
    // Intestazioni
    const headers = ["ID", "Datetime", "Descrizione", "Action Code", "PC"];
    // Righe
    const rows = data.map(log => [
        log.id,
        formatLocalDate(log.createdAt),
        log.description || '-',
        log.action,
        log.category || '-'
    ]);
    // CSV string
    const csvContent = [headers, ...rows].map(e => e.map(v => `"${String(v).replace(/"/g, '""')}"`).join(",")).join("\n");
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
        <div class="card">
            <Toolbar class="mb-6">
                <template #start>
                    <div class="flex flex-col sm:flex-row gap-2">
                        <Button label="Delete Logs" icon="pi pi-trash" severity="danger" @click="showResetDialog = true"
                            class="w-full sm:w-auto" />
                        <Dialog v-model:visible="showResetDialog" :closable="!resetLoading" :modal="true"
                            :dismissableMask="!resetLoading" :style="{ width: '350px' }">
                            <template #header>
                                <span>Conferma Reset</span>
                            </template>
                            <div class="mb-4">Sei sicuro di voler cancellare i log?</div>
                            <div class="flex justify-end gap-2">
                                <button class="p-button p-component" @click="showResetDialog = false"
                                    :disabled="resetLoading">Annulla</button>
                                <button class="p-button p-component p-button-danger" @click="deleteLogs"
                                    :disabled="resetLoading">
                                    <span v-if="resetLoading" class="pi pi-spin pi-spinner mr-2"></span>
                                    <span v-else class="pi pi-check mr-2"></span>
                                    Conferma
                                </button>
                            </div>
                        </Dialog>
                    </div>
                </template>

                <template #end>
                    <Button label="Export" icon="pi pi-upload" severity="secondary" @click="onExportCSV()"
                        class="w-full sm:w-auto" />
                </template>
            </Toolbar>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4" :style="{ margin: '20px 0px' }">

                <!-- Date range -->
                <InputGroup class="flex flex-col md:flex-row gap-2">
                    <DatePicker v-model="from" dateFormat="yyyy-MM-dd HH:mm" showTime showIcon placeholder="From"
                        :style="{ width: '100%' }" />
                    <DatePicker v-model="to" dateFormat="yyyy-MM-dd HH:mm" showTime showIcon placeholder="To"
                        :style="{ width: '100%' }" />
                </InputGroup>

                <!-- PC + Description -->
                <InputGroup class="flex flex-col md:flex-row gap-2">
                    <InputText v-model="pc" placeholder="PC" :style="{ width: '100%' }" />
                    <InputNumber v-model="action" placeholder="Action" :style="{ width: '100%' }" />
                    <InputText v-model="description" placeholder="Description" :style="{ width: '100%' }" />
                    <Button label="Search" icon="pi pi-search" @click="onSearch" class="p-button-sm w-full md:w-auto"
                        :style="{ width: '100%' }" />
                </InputGroup>

            </div>

            <DataTable ref="dt" :value="logs" dataKey="id" :paginator="true" :rows="pageSize"
                :totalRecords="totalRecords" :first="first" :filters="filters" v-model:expandedRows="expandedRows"
                responsiveLayout="scroll" breakpoint="960px"
                paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
                :rowsPerPageOptions="[5, 10, 25]"
                currentPageReportTemplate="Showing {first} to {last} of {totalRecords} Logs" @page="onPageChange"
                :lazy="true">

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
                        <span style="white-space: pre;">
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
