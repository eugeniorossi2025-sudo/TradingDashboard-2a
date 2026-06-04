<script setup>
import FloatingConfigurator from '@/components/FloatingConfigurator.vue';
import { AuthService } from '@/service/AuthService';
import { TokenService } from '@/service/TokenService';
import { useToast } from 'primevue/usetoast';
import { onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';

const router = useRouter();
const route = useRoute();
const toast = useToast();

const username = ref('');
const password = ref('');
const rememberMe = ref(false);
const isLoading = ref(false);
const showResetPassword = ref(false);
const resetEmail = ref('');

const isMobileViewport = () => {
    if (typeof window === 'undefined') return false;
    return window.matchMedia('(max-width: 768px), (pointer: coarse)').matches;
};

const getMobileHome = () => {
    if (AuthService.isRootOwner.value) return '/admin/root-owner';
    return AuthService.isAdmin.value ? '/admin/mobile-live' : '/client/mobile';
};

const resolvePostLoginRoute = () => {
    const savedRedirect = TokenService.getAndClearRedirectPath();
    const requestedRedirect = savedRedirect || route.query.redirect;
    const redirectTo = Array.isArray(requestedRedirect) ? requestedRedirect[0] : requestedRedirect;

    if (isMobileViewport()) {
        const explicitMobile =
            redirectTo === '/admin/mobile-live' ||
            redirectTo === '/admin/root-owner' ||
            redirectTo === '/client/mobile';
        return explicitMobile ? redirectTo : getMobileHome();
    }

    return redirectTo || '/';
};

// Mostra messaggio di sessione scaduta se presente
onMounted(() => {
    if (route.query.expired === 'true') {
        toast.add({
            severity: 'warn',
            summary: 'Sessione Scaduta',
            detail: 'La tua sessione è scaduta. Effettua nuovamente il login.',
            life: 5000
        });
    }
});

// Gestione login
const handleLogin = async () => {
    // Validazione
    if (!username.value || !password.value) {
        toast.add({
            severity: 'warn',
            summary: 'Campi richiesti',
            detail: 'Inserisci username e password',
            life: 3000
        });
        return;
    }

    isLoading.value = true;

    try {
        // Chiama il servizio di autenticazione
        await AuthService.login(username.value, password.value, rememberMe.value);

        toast.add({
            severity: 'success',
            summary: 'Login effettuato',
            detail: 'Benvenuto!',
            life: 2000
        });

        // Piccolo delay per mostrare il toast
        setTimeout(() => {
            router.push(resolvePostLoginRoute());
        }, 500);
    } catch (error) {
        console.error('Login error:', error);

        // Se la risposta è 204 mostra messaggio specifico
        if (error?.response?.status === 401 || error?.response?.status === 403 || error?.response?.status === 404 || error?.response?.status === 400 || error?.response?.status === 204) {
            toast.add({
                severity: 'error',
                summary: 'Credenziali sbagliate',
                detail: 'Username o password non corretti',
                life: 5000
            });
        } else {
            toast.add({
                severity: 'error',
                summary: 'Errore di login',
                detail: error?.message || 'Username o password non corretti',
                life: 5000
            });
        }
    } finally {
        isLoading.value = false;
    }
};

// Gestione reset password
const handleForgotPassword = () => {
    showResetPassword.value = true;
};

const handleResetPasswordSubmit = async () => {
    if (!resetEmail.value) {
        toast.add({
            severity: 'warn',
            summary: 'Email richiesta',
            detail: 'Inserisci la tua email per il reset password',
            life: 3000
        });
        return;
    }

    isLoading.value = true;

    try {
        await AuthService.resetPasswordRequest(resetEmail.value);

        toast.add({
            severity: 'success',
            summary: 'Email inviata',
            detail: 'Controlla la tua email per le istruzioni di reset',
            life: 5000
        });

        showResetPassword.value = false;
        resetEmail.value = '';
    } catch (error) {
        console.error('Reset password error:', error);

        toast.add({
            severity: 'error',
            summary: 'Errore',
            detail: error?.message || 'Errore durante il reset password',
            life: 5000
        });
    } finally {
        isLoading.value = false;
    }
};

const cancelResetPassword = () => {
    showResetPassword.value = false;
    resetEmail.value = '';
};
</script>

<template>
    <Toast />
    <FloatingConfigurator />
    <div class="bg-surface-50 dark:bg-surface-950 flex items-center justify-center min-h-screen min-w-[100vw] overflow-hidden">
        <div class="flex flex-col items-center justify-center">
            <div style="border-radius: 56px; padding: 0.3rem; background: linear-gradient(180deg, var(--primary-color) 10%, rgba(33, 150, 243, 0) 30%)">
                <div class="w-full bg-surface-0 dark:bg-surface-900 py-20 px-8 sm:px-20" style="border-radius: 53px">
                    <!-- Login Form -->
                    <div v-if="!showResetPassword">
                        <div class="text-center mb-8">
                            <div class="text-surface-900 dark:text-surface-0 text-3xl font-medium mb-4">Benvenuti in Eugenio!</div>
                            <span class="text-muted-color font-medium">Accedi per continuare</span>
                        </div>

                        <form @submit.prevent="handleLogin">
                            <label for="username1" class="block text-surface-900 dark:text-surface-0 text-xl font-medium mb-2">Username</label>
                            <InputText id="username1" type="text" placeholder="Username" class="w-full md:w-[30rem] mb-8" v-model="username" :disabled="isLoading" required />

                            <label for="password1" class="block text-surface-900 dark:text-surface-0 font-medium text-xl mb-2">Password</label>
                            <Password id="password1" v-model="password" placeholder="Password" :toggleMask="true" class="mb-4" fluid :feedback="false" :disabled="isLoading" required />

                            <div class="flex items-center justify-between mt-2 mb-8 gap-8">
                                <div class="flex items-center">
                                    <Checkbox v-model="rememberMe" id="rememberme1" binary class="mr-2" :disabled="isLoading"></Checkbox>
                                    <label for="rememberme1">Ricordami</label>
                                </div>
                                <span class="font-medium no-underline ml-2 text-right cursor-pointer text-primary" @click="handleForgotPassword"> Password dimenticata? </span>
                            </div>

                            <Button type="submit" label="Accedi" class="w-full" :loading="isLoading" :disabled="isLoading" />
                        </form>
                    </div>

                    <!-- Reset Password Form -->
                    <div v-else>
                        <div class="text-center mb-8">
                            <div class="text-surface-900 dark:text-surface-0 text-3xl font-medium mb-4">Reset Password</div>
                            <span class="text-muted-color font-medium">Inserisci la tua email per reimpostare la password</span>
                        </div>

                        <form @submit.prevent="handleResetPasswordSubmit">
                            <label for="resetEmail" class="block text-surface-900 dark:text-surface-0 text-xl font-medium mb-2">Email</label>
                            <InputText id="resetEmail" type="email" placeholder="Email" class="w-full md:w-[30rem] mb-8" v-model="resetEmail" :disabled="isLoading" required />

                            <div class="flex gap-4">
                                <Button type="button" label="Annulla" severity="secondary" class="w-full" :disabled="isLoading" @click="cancelResetPassword" />
                                <Button type="submit" label="Invia" class="w-full" :loading="isLoading" :disabled="isLoading" />
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped>
.pi-eye {
    transform: scale(1.6);
    margin-right: 1rem;
}

.pi-eye-slash {
    transform: scale(1.6);
    margin-right: 1rem;
}
</style>
