<script setup>
import { PCManagerService } from '@/service/PCManagerService';
import { FilterMatchMode } from '@primevue/core/api';
import { useToast } from 'primevue/usetoast';
import { onMounted, ref } from 'vue';

const toast = useToast();
const dt = ref();
const pcs = ref([]);
const loading = ref(false);

const pcDialog = ref(false);
const deletePCDialog = ref(false);
const deletePCsDialog = ref(false);
const isEditMode = ref(false);

const pc = ref({
    id: null,
    title: '',
    stato: 0,
    amount: 0
});
const selectedPCs = ref(null);

const filters = ref({
    global: { value: null, matchMode: FilterMatchMode.CONTAINS }
});

const submitted = ref(false);

onMounted(async () => {
    await loadPCs();
});

async function loadPCs() {
    try {
        loading.value = true;
        pcs.value = await PCManagerService.getPCs();
    } catch (error) {
        toast.add({
            severity: 'error',
            summary: 'Error',
            detail: error?.response?.data?.title || 'Failed to load devices',
            life: 3000
        });
    } finally {
        loading.value = false;
    }
}

function openNew() {
    pc.value = {
        id: null,
        title: '',
        stato: 0,
        amount: 0
    };
    isEditMode.value = false;
    submitted.value = false;
    pcDialog.value = true;
}

function hideDialog() {
    pcDialog.value = false;
    submitted.value = false;
}

async function savePC() {
    submitted.value = true;

    if (!pc.value.title?.trim()) {
        toast.add({
            severity: 'warn',
            summary: 'Warning',
            detail: 'Title is required',
            life: 3000
        });
        return;
    }

    try {
        if (isEditMode.value) {
            await PCManagerService.updatePC(pc.value.id, {
                title: pc.value.title,
                stato: pc.value.stato ?? 0,
                amount: pc.value.amount ?? 0
            });
            toast.add({
                severity: 'success',
                summary: 'Success',
                detail: 'Device Updated',
                life: 3000
            });
        } else {
            await PCManagerService.createPC({
                id: pc.value.id?.trim() || null,
                title: pc.value.title,
                stato: pc.value.stato ?? 0,
                amount: pc.value.amount ?? 0
            });
            toast.add({
                severity: 'success',
                summary: 'Success',
                detail: 'Device Created',
                life: 3000
            });
        }

        pcDialog.value = false;
        pc.value = { id: null, title: '', stato: 0, amount: 0 };
        await loadPCs();
    } catch (error) {
        console.error('Error saving device:', error);
        toast.add({
            severity: 'error',
            summary: 'Error',
            detail: error?.response?.data?.title || 'Failed to save device',
            life: 3000
        });
    }
}

function editPC(pcData) {
    pc.value = { ...pcData };
    isEditMode.value = true;
    pcDialog.value = true;
}

function confirmDeletePC(pcData) {
    pc.value = { ...pcData };
    deletePCDialog.value = true;
}

async function deletePC() {
    try {
        await PCManagerService.deletePC(pc.value.id);
        deletePCDialog.value = false;
        pc.value = { id: null, title: '', stato: 0, amount: 0 };
        toast.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Device Deleted',
            life: 3000
        });
        await loadPCs();
    } catch (error) {
        console.error('Error deleting device:', error);
        toast.add({
            severity: 'error',
            summary: 'Error',
            detail: error?.response?.data?.title || 'Failed to delete device',
            life: 3000
        });
    }
}

async function deleteSelectedPCs() {
    try {
        const deletePromises = selectedPCs.value.map(device =>
            PCManagerService.deletePC(device.id)
        );
        await Promise.all(deletePromises);

        deletePCsDialog.value = false;
        selectedPCs.value = null;
        toast.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Devices Deleted',
            life: 3000
        });
        await loadPCs();
    } catch (error) {
        console.error('Error deleting devices:', error);
        toast.add({
            severity: 'error',
            summary: 'Error',
            detail: error?.response?.data?.title || 'Failed to delete devices',
            life: 3000
        });
    }
}

</script>

<template>
    <div>
        <div class="card">
            <Toolbar class="mb-6">
                <template #start>
                    <div class="flex flex-col sm:flex-row gap-2">
                        <Button label="New" icon="pi pi-plus" severity="secondary" @click="openNew"
                            class="w-full sm:w-auto" />
                    </div>
                </template>

                <template #end>
                    <Button label="Export" icon="pi pi-upload" severity="secondary" @click="dt.exportCSV()"
                        class="w-full sm:w-auto" />
                </template>
            </Toolbar>

            <DataTable ref="dt" v-model:selection="selectedPCs" :value="pcs" dataKey="id" :paginator="true" :rows="10"
                :filters="filters" :loading="loading" responsiveLayout="scroll" breakpoint="960px"
                paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
                :rowsPerPageOptions="[5, 10, 25]"
                currentPageReportTemplate="Showing {first} to {last} of {totalRecords} PCs">
                <template #header>
                    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-3">
                        <h4 class="m-0 text-lg">Manage PCs</h4>
                        <IconField class="w-full sm:w-auto">
                            <InputIcon>
                                <i class="pi pi-search" />
                            </InputIcon>
                            <InputText v-model="filters['global'].value" placeholder="Search..."
                                class="w-full sm:w-auto" />
                        </IconField>
                    </div>
                </template>

                <Column field="title" header="Title" sortable :style="{ minWidth: '180px' }" />


                <Column field="amount" header="Amount" sortable :style="{ minWidth: '120px' }">
                    <template #body="slotProps">
                        {{ slotProps.data.amount }}
                    </template>
                </Column>

                <Column :exportable="false" :style="{ minWidth: '120px', width: '120px' }">
                    <template #body="slotProps">
                        <div class="flex gap-2">
                            <Button icon="pi pi-pencil" outlined rounded size="small" @click="editPC(slotProps.data)"
                                v-tooltip.top="'Edit'" />
                            <Button icon="pi pi-trash" outlined rounded severity="danger" size="small"
                                @click="confirmDeletePC(slotProps.data)" v-tooltip.top="'Delete'" />
                        </div>
                    </template>
                </Column>

                <template #empty>
                    <div class="text-center py-6">
                        <i class="pi pi-inbox text-4xl text-gray-400 mb-3"></i>
                        <p class="text-gray-500">No PCs found.</p>
                    </div>
                </template>
            </DataTable>
        </div>

        <!-- PC Dialog -->
        <Dialog v-model:visible="pcDialog" :style="{ width: '90vw', maxWidth: '450px' }"
            :header="isEditMode ? 'Edit Device' : 'New Device'" :modal="true"
            :breakpoints="{ '960px': '75vw', '640px': '95vw' }">
            <div class="flex flex-col gap-4">
                <div v-if="!isEditMode">
                    <label class="block font-bold mb-2">ID (Optional)</label>
                    <InputText v-model.trim="pc.id" fluid
                        placeholder="Enter device ID (leave empty for auto-generate)" />
                    <small class="text-gray-500">If not provided, an ID will be automatically generated.</small>
                </div>

                <div>
                    <label class="block font-bold mb-2">Title</label>
                    <InputText v-model.trim="pc.title" required :invalid="submitted && !pc.title" fluid
                        placeholder="Enter device title" />
                    <small v-if="submitted && !pc.title" class="text-red-500">Title is required.</small>
                </div>

                <div>
                    <label class="block font-bold mb-2">Amount</label>
                    <InputNumber v-model="pc.amount" :min="0" fluid placeholder="Enter amount" />
                    <small class="text-gray-500">Quantity or amount associated with device</small>
                </div>
            </div>

            <template #footer>
                <div class="flex flex-col sm:flex-row gap-2 sm:gap-0 sm:justify-end">
                    <Button label="Cancel" icon="pi pi-times" text @click="hideDialog" class="w-full sm:w-auto" />
                    <Button label="Save" icon="pi pi-check" @click="savePC" class="w-full sm:w-auto" />
                </div>
            </template>
        </Dialog>

        <!-- Delete PC -->
        <Dialog v-model:visible="deletePCDialog" :style="{ width: '90vw', maxWidth: '450px' }" header="Confirm Delete"
            :modal="true" :breakpoints="{ '640px': '95vw' }">
            <div class="flex items-center gap-4">
                <i class="pi pi-exclamation-triangle text-3xl text-orange-500" />
                <span>Are you sure you want to delete <b>{{ pc.title }}</b>?</span>
            </div>
            <template #footer>
                <div class="flex flex-col sm:flex-row gap-2 sm:gap-0 sm:justify-end">
                    <Button label="No" text icon="pi pi-times" @click="deletePCDialog = false"
                        class="w-full sm:w-auto" />
                    <Button label="Yes" icon="pi pi-check" severity="danger" @click="deletePC"
                        class="w-full sm:w-auto" />
                </div>
            </template>
        </Dialog>

        <!-- Delete selected -->
        <Dialog v-model:visible="deletePCsDialog" :style="{ width: '90vw', maxWidth: '450px' }"
            header="Confirm Delete Multiple" :modal="true" :breakpoints="{ '640px': '95vw' }">
            <div class="flex items-center gap-4">
                <i class="pi pi-exclamation-triangle text-3xl text-orange-500" />
                <span>Are you sure you want to delete <b>{{ selectedPCs?.length || 0 }}</b> selected device(s)?</span>
            </div>
            <template #footer>
                <div class="flex flex-col sm:flex-row gap-2 sm:gap-0 sm:justify-end">
                    <Button label="No" text icon="pi pi-times" @click="deletePCsDialog = false"
                        class="w-full sm:w-auto" />
                    <Button label="Yes" icon="pi pi-check" severity="danger" @click="deleteSelectedPCs"
                        class="w-full sm:w-auto" />
                </div>
            </template>
        </Dialog>
    </div>
</template>

<style scoped>
@media (max-width: 640px) {
    :deep(.p-datatable-table) {
        font-size: 0.875rem;
    }

    :deep(.p-datatable-header-cell),
    :deep(.p-datatable-tbody > tr > td) {
        padding: 0.5rem;
    }

    :deep(.p-toolbar) {
        flex-direction: row;
        gap: 1rem;
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

@media (min-width: 769px) {
    :deep(.hidden.md\\:table-cell) {
        display: table-cell !important;
    }
}
</style>
