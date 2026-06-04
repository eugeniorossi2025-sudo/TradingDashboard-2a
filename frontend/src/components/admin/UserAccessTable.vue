<script setup>
import { formatRomeDateTime } from '@/utils/romeTime';

defineProps({
    rows: { type: Array, default: () => [] },
    loading: { type: Boolean, default: false },
    adminBlock: { type: Boolean, default: false }
});

defineEmits(['details', 'report', 'toggle-enabled', 'delete']);

function formatDate(value) {
    return formatRomeDateTime(value);
}
</script>

<template>
    <DataTable :value="rows" :loading="loading" dataKey="userId" responsiveLayout="scroll" breakpoint="960px" class="mb-4">
        <Column field="status" header="Stato">
            <template #body="{ data }">
                <Tag :value="data.status" :severity="data.status === 'Online' ? 'success' : 'secondary'" />
            </template>
        </Column>
        <Column field="username" header="Username" />
        <Column field="role" header="Ruolo" />
        <Column field="accountType" header="Account type" />
        <Column field="lastLoginUtc" header="Ultimo accesso">
            <template #body="{ data }">{{ adminBlock ? '—' : formatDate(data.lastLoginUtc) }}</template>
        </Column>
        <Column field="lastIp" header="Ultimo IP">
            <template #body="{ data }">{{ adminBlock ? '—' : data.lastIp || '-' }}</template>
        </Column>
        <Column field="lastPage" header="Ultima pagina">
            <template #body="{ data }">{{ adminBlock ? '—' : data.lastPage || '-' }}</template>
        </Column>
        <Column field="lastEvent" header="Ultimo evento">
            <template #body="{ data }">{{ adminBlock ? '—' : data.lastEvent || '-' }}</template>
        </Column>
        <Column header="Azioni" :style="{ minWidth: '310px' }">
            <template #body="{ data }">
                <div class="flex flex-wrap gap-2">
                    <Button label="Dettagli" size="small" severity="secondary" outlined @click="$emit('details', data)" />
                    <Button v-if="!adminBlock" label="Report accessi" size="small" severity="secondary" outlined @click="$emit('report', data)" />
                    <template v-if="!data.isRootOwner">
                        <Button :label="data.enabled ? 'Disattiva' : 'Riattiva'" size="small" :severity="data.enabled ? 'warn' : 'success'" outlined @click="$emit('toggle-enabled', data)" />
                        <Button label="Elimina" size="small" severity="danger" outlined @click="$emit('delete', data)" />
                    </template>
                    <Tag v-else value="Root Owner protetto" severity="warning" />
                </div>
            </template>
        </Column>
        <template #empty>
            <div class="text-center py-5 text-muted-color">Nessun utente trovato.</div>
        </template>
    </DataTable>
</template>
