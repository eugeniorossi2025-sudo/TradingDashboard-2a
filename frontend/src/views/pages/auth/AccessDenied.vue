<script setup>
import { useAuth } from '@/composables/useAuth';
import { useRouter } from 'vue-router';

const router = useRouter();
const { isAuthenticated, currentUser } = useAuth();

const goBack = () => {
    router.go(-1);
};

const goToDashboard = () => {
    router.push('/');
};
</script>

<template>
    <div
        class="bg-surface-50 dark:bg-surface-950 flex items-center justify-center min-h-screen min-w-[100vw] overflow-hidden">
        <div class="flex flex-col items-center justify-center">
            <div
                style="border-radius: 56px; padding: 0.3rem; background: linear-gradient(180deg, rgba(255, 159, 64, 0.4) 10%, rgba(33, 150, 243, 0) 30%)">
                <div class="w-full bg-surface-0 dark:bg-surface-900 py-20 px-8 sm:px-20 flex flex-col items-center"
                    style="border-radius: 53px">
                    <div class="gap-4 flex flex-col items-center">
                        <div class="flex justify-center items-center border-2 border-orange-500 rounded-full"
                            style="height: 3.2rem; width: 3.2rem">
                            <i class="pi pi-fw pi-lock !text-2xl text-orange-500"></i>
                        </div>
                        <h1 class="text-surface-900 dark:text-surface-0 font-bold text-5xl mb-2">Accesso Negato</h1>
                        <span class="text-muted-color mb-4 text-center max-w-md">
                            Non hai i permessi necessari per accedere a questa risorsa.
                        </span>
                        <div v-if="isAuthenticated && currentUser" class="text-center mb-8">
                            <p class="text-surface-600 dark:text-surface-400">
                                Utente: <strong>{{ currentUser.username }}</strong>
                            </p>
                            <p class="text-surface-500 dark:text-surface-500 text-sm mt-2">
                                Contatta l'amministratore se ritieni di dover avere accesso a questa pagina.
                            </p>
                        </div>
                        <img src="/demo/images/error/asset-error.svg" alt="Access Denied" class="mb-8" width="80%" />
                        <div class="flex gap-4 mt-8">
                            <Button label="Torna Indietro" icon="pi pi-arrow-left" @click="goBack" severity="secondary"
                                outlined />
                            <Button label="Vai alla Dashboard" icon="pi pi-home" @click="goToDashboard"
                                severity="warning" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped>
/* Prevent layout shift */
.min-h-screen {
    min-height: 100vh;
}
</style>
