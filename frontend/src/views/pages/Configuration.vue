<script setup>
import { ConfigurationService } from '@/service/ConfigurationService';
import { FinancialReportService } from '@/service/FinancialReportService';
import { useToast } from 'primevue/usetoast';
import { onMounted, ref } from 'vue';

const toast = useToast();
const dt = ref();
const configurations = ref([]);
const selectedConfigurations = ref([]);
const configurationDialog = ref(false);
const deleteDialog = ref(false);
const deleteConfigurationsDialog = ref(false);
const currentConfig = ref({
    k: '',
    description: '',
    value: '0',
    pos: 1
});
const submitted = ref(false);
const runtimeMode = ref({ runtimeMode: 'Production', isDemoMode: false });
const runtimeModeLoading = ref(false);

const filters = ref({
    global: { value: '', matchMode: 'contains' }
});

async function loadData() {
    configurations.value = await ConfigurationService.getConfigurations();
}

async function loadRuntimeMode() {
    runtimeMode.value = await FinancialReportService.getRuntimeMode();
}

async function setRuntimeMode(mode) {
    runtimeModeLoading.value = true;
    try {
        runtimeMode.value = await FinancialReportService.setRuntimeMode(mode);
        toast.add({
            severity: 'success',
            summary: 'Modalità aggiornata',
            detail: mode === 'Demo' ? 'Contabilità in DEMO' : 'Contabilità in PRODUZIONE',
            life: 2500
        });
    } catch (error) {
        console.error('Runtime mode update failed:', error);
        toast.add({
            severity: 'error',
            summary: 'Errore',
            detail: 'Impossibile aggiornare Production/Demo',
            life: 3500
        });
    } finally {
        runtimeModeLoading.value = false;
    }
}

function editConfig(config) {
    currentConfig.value = { ...config };
    configurationDialog.value = true;
}

async function saveConfig() {
    submitted.value = true;

    if (!currentConfig.value.k) return;

    try {
        if ('k' in currentConfig.value && currentConfig.value.k) {
            // Update existing configuration
            await ConfigurationService.updateConfiguration(currentConfig.value.k, {
                description: currentConfig.value.description,
                pos: currentConfig.value.pos,
                value: currentConfig.value.value
            });
            toast.add({
                severity: 'success',
                summary: 'Success',
                detail: 'Configuration updated successfully',
                life: 3000
            });
        } else {
            // Create new configuration
            await ConfigurationService.createConfiguration(currentConfig.value);
            toast.add({
                severity: 'success',
                summary: 'Success',
                detail: 'Configuration created successfully',
                life: 3000
            });
        }

        configurationDialog.value = false;
        await loadData();
    } catch (error) {
        console.error('Error saving configuration:', error);
        toast.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to save configuration',
            life: 3000
        });
    }
}

function confirmDelete(config) {
    currentConfig.value = { ...config };
    deleteDialog.value = true;
}

async function deleteConfig() {
    try {
        if ('k' in currentConfig.value && currentConfig.value.k) {
            await ConfigurationService.deleteConfiguration(currentConfig.value.k);
            toast.add({
                severity: 'success',
                summary: 'Success',
                detail: 'Configuration deleted successfully',
                life: 3000
            });
        }
        deleteDialog.value = false;
        await loadData();
    } catch (error) {
        console.error('Error deleting configuration:', error);
        toast.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to delete configuration',
            life: 3000
        });
    }
}

async function deleteSelectedConfigurations() {
    try {
        const deletePromises = selectedConfigurations.value.map((config) => ConfigurationService.deleteConfiguration(config.k));
        await Promise.all(deletePromises);

        deleteConfigurationsDialog.value = false;
        selectedConfigurations.value = [];
        toast.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Configurations deleted successfully',
            life: 3000
        });
        await loadData();
    } catch (error) {
        console.error('Error deleting configurations:', error);
        toast.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to delete configurations',
            life: 3000
        });
    }
}

function exportCSV() {
    dt.value.exportCSV();
}

onMounted(async () => {
    await Promise.all([loadData(), loadRuntimeMode()]);
});
</script>

<template>
    <div>
        <div class="card mb-4">
            <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
                <div>
                    <div class="text-xl font-semibold mb-1">Modalità contabile</div>
                    <div class="text-muted-color text-sm">In Demo i dati restano separati dalla reportistica ufficiale Production.</div>
                    <div class="mt-2 text-sm">
                        Stato corrente:
                        <strong :class="runtimeMode.isDemoMode ? 'text-orange-400' : 'text-green-500'">
                            {{ runtimeMode.isDemoMode ? 'DEMO' : 'PRODUZIONE' }}
                        </strong>
                    </div>
                </div>
                <div class="flex gap-2 flex-wrap">
                    <Button label="PRODUZIONE" :severity="runtimeMode.isDemoMode ? 'secondary' : 'success'" :outlined="runtimeMode.isDemoMode" :loading="runtimeModeLoading && !runtimeMode.isDemoMode" @click="setRuntimeMode('Production')" />
                    <Button label="DEMO" :severity="runtimeMode.isDemoMode ? 'warn' : 'secondary'" :outlined="!runtimeMode.isDemoMode" :loading="runtimeModeLoading && runtimeMode.isDemoMode" @click="setRuntimeMode('Demo')" />
                </div>
            </div>
        </div>

        <div class="card">
            <Toolbar class="mb-6">
                <template #end>
                    <Button label="Export" icon="pi pi-upload" severity="secondary" @click="exportCSV" class="w-full sm:w-auto" />
                </template>
            </Toolbar>

            <DataTable
                ref="dt"
                :value="configurations"
                v-model:selection="selectedConfigurations"
                dataKey="k"
                :paginator="true"
                :rows="10"
                :filters="filters"
                responsiveLayout="scroll"
                breakpoint="960px"
                :rowsPerPageOptions="[5, 10, 20]"
                paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
                currentPageReportTemplate="Showing {first} to {last} of {totalRecords} configurations"
            >
                <template #header>
                    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-3">
                        <h4 class="m-0 text-lg">Configurations</h4>

                        <IconField class="w-full sm:w-auto">
                            <InputIcon><i class="pi pi-search" /></InputIcon>
                            <InputText v-model="filters.global.value" placeholder="Search..." class="w-full sm:w-auto" />
                        </IconField>
                    </div>
                </template>

                <!-- Colonne principali: sempre visibili -->
                <Column field="k" header="Key" :style="{ minWidth: '150px' }" />

                <!-- Description: nascosta su mobile, visibile su tablet+ -->
                <Column field="description" header="Description" :class="'hidden sm:table-cell'" :style="{ minWidth: '200px' }">
                    <template #body="{ data }">
                        <span class="line-clamp-2">{{ data.description }}</span>
                    </template>
                </Column>

                <Column field="value" header="Value" :style="{ minWidth: '120px' }" />

                <!-- Pos: nascosta su mobile -->
                <Column field="pos" header="Pos" :class="'hidden lg:table-cell'" :style="{ width: '100px' }" />

                <!-- Actions: sempre visibile ma compatto su mobile -->
                <Column :exportable="false" header="Actions" :style="{ minWidth: '120px', width: '120px' }">
                    <template #body="{ data }">
                        <div class="flex gap-2">
                            <Button icon="pi pi-pencil" outlined rounded size="small" @click="editConfig(data)" v-tooltip.top="'Edit'" />
                            <Button v-if="false" icon="pi pi-trash" outlined rounded size="small" severity="danger" @click="confirmDelete(data)" v-tooltip.top="'Delete'" />
                        </div>
                    </template>
                </Column>

                <!-- Template per quando non ci sono dati -->
                <template #empty>
                    <div class="text-center py-6">
                        <i class="pi pi-inbox text-4xl text-gray-400 mb-3"></i>
                        <p class="text-gray-500">No configurations found.</p>
                    </div>
                </template>
            </DataTable>
        </div>

        <!-- DIALOG per create/edit -->
        <Dialog v-model:visible="configurationDialog" header="Configuration Details" :modal="true" :style="{ width: '90vw', maxWidth: '450px' }" :breakpoints="{ '960px': '75vw', '640px': '95vw' }">
            <div class="flex flex-col gap-4">
                <div v-if="!('k' in currentConfig)">
                    <label class="font-bold block mb-2">Key</label>
                    <InputText v-model="currentConfig.k" :invalid="submitted && !currentConfig.k" fluid />
                    <small v-if="submitted && !currentConfig.k" class="text-red-500">Key is required.</small>
                </div>

                <div>
                    <label class="font-bold block mb-2">Description</label>
                    <Textarea v-model="currentConfig.description" rows="3" fluid />
                </div>

                <div>
                    <label class="font-bold block mb-2">Value</label>
                    <InputText v-model="currentConfig.value" fluid />
                </div>

                <div>
                    <label class="font-bold block mb-2">Position</label>
                    <InputNumber v-model="currentConfig.pos" fluid />
                </div>
            </div>

            <template #footer>
                <div class="flex flex-col sm:flex-row gap-2 sm:gap-0 sm:justify-end">
                    <Button label="Cancel" icon="pi pi-times" text @click="configurationDialog = false" class="w-full sm:w-auto" />
                    <Button label="Save" icon="pi pi-check" @click="saveConfig" class="w-full sm:w-auto" />
                </div>
            </template>
        </Dialog>

        <!-- DIALOG per conferma delete -->
        <Dialog v-model:visible="deleteDialog" header="Confirm" :modal="true" :style="{ width: '90vw', maxWidth: '350px' }" :breakpoints="{ '640px': '95vw' }">
            <div class="flex items-center gap-4">
                <i class="pi pi-exclamation-triangle text-3xl" />
                <span
                    >Are you sure you want to delete <b>{{ currentConfig.k }}</b
                    >?</span
                >
            </div>

            <template #footer>
                <div class="flex flex-col sm:flex-row gap-2 sm:gap-0 sm:justify-end">
                    <Button label="No" icon="pi pi-times" text @click="deleteDialog = false" class="w-full sm:w-auto" />
                    <Button label="Yes" icon="pi pi-check" severity="danger" @click="deleteConfig" class="w-full sm:w-auto" />
                </div>
            </template>
        </Dialog>

        <!-- DIALOG per conferma delete multiplo -->
        <Dialog v-model:visible="deleteConfigurationsDialog" header="Confirm" :modal="true" :style="{ width: '90vw', maxWidth: '350px' }" :breakpoints="{ '640px': '95vw' }">
            <div class="flex items-center gap-4">
                <i class="pi pi-exclamation-triangle text-3xl" />
                <span>Are you sure you want to delete the selected configurations?</span>
            </div>

            <template #footer>
                <div class="flex flex-col sm:flex-row gap-2 sm:gap-0 sm:justify-end">
                    <Button label="No" icon="pi pi-times" text @click="deleteConfigurationsDialog = false" class="w-full sm:w-auto" />
                    <Button label="Yes" icon="pi pi-check" severity="danger" @click="deleteSelectedConfigurations" class="w-full sm:w-auto" />
                </div>
            </template>
        </Dialog>
    </div>
</template>

<style scoped>
/* Responsive utilities */
.line-clamp-2 {
    display: -webkit-box;
    -webkit-line-clamp: 2;
    line-clamp: 2;
    -webkit-box-orient: vertical;
    overflow: hidden;
    text-overflow: ellipsis;
}

/* Miglioramenti per mobile */
@media (max-width: 640px) {
    :deep(.p-datatable-table) {
        font-size: 0.875rem;
    }

    :deep(.p-datatable-header-cell),
    :deep(.p-datatable-tbody > tr > td) {
        padding: 0.5rem;
    }

    :deep(.p-paginator) {
        flex-wrap: wrap;
        gap: 0.5rem;
    }

    :deep(.p-paginator-current) {
        width: 100%;
        text-align: center;
    }
}

/* Ottimizzazione per tablet */
@media (min-width: 641px) and (max-width: 1024px) {
    :deep(.p-datatable-table) {
        font-size: 0.9375rem;
    }
}

/* Nascondere colonne su diversi breakpoint */
@media (max-width: 640px) {
    :deep(.hidden.sm\\:table-cell) {
        display: none !important;
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

@media (min-width: 641px) {
    :deep(.hidden.sm\\:table-cell) {
        display: table-cell !important;
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
