<script setup>
const featureDisabled = true;
const disabledReason = 'API /api/botsession non implementata nel backend DASH2A. Pagina disabilitata fino al deploy del controller BotSession.';
</script>

<template>
    <div class="card">
        <div class="flex justify-between items-center mb-4">
            <h2 class="text-2xl font-bold">Sessioni Bot Attive</h2>
        </div>

        <Message severity="warn" :closable="false" class="mb-4">
            {{ disabledReason }}
        </Message>

        <div v-if="featureDisabled" class="text-center p-8 text-muted-color">
            <i class="pi pi-exclamation-triangle text-4xl mb-3"></i>
            <p>Funzionalità non disponibile in locale/prod-like finché il backend non espone `/api/botsession/*`.</p>
        </div>
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
