<script setup>
import { UserService } from '@/service/UserService';
import UserAccessTable from '@/components/admin/UserAccessTable.vue';
import { useToast } from 'primevue/usetoast';
import { onMounted, ref } from 'vue';

const toast = useToast();
const loading = ref(false);
const notificationLoading = ref(false);
const overview = ref({ operative: [], bots: [], admins: [] });
const notificationSettings = ref([]);
const newUser = ref({ username: '', password: '', roleName: 'User' });
const accessDialog = ref(false);
const accessLoading = ref(false);
const selectedUser = ref(null);
const accessEvents = ref([]);
const roleOptions = ['User', 'Admin', 'BotOperator'];
const passwordHint = 'Min 8 caratteri, maiuscola, minuscola, numero e carattere speciale. Username: solo lettere, numeri e _';

function readApiError(error, fallback = 'Operazione non riuscita') {
    const errors = error?.response?.data?.errors;
    if (Array.isArray(errors) && errors.length) {
        return errors.join(' · ');
    }

    return error?.response?.data?.message || error?.message || fallback;
}

function validateNewUserInput() {
    const username = newUser.value.username.trim();
    const password = newUser.value.password;

    if (!username || !password) {
        return 'Username e password obbligatori';
    }

    if (username.length < 3) {
        return 'Username: minimo 3 caratteri';
    }

    if (!/^[a-zA-Z0-9_]+$/.test(username)) {
        return 'Username: solo lettere, numeri e underscore';
    }

    if (password.length < 8) {
        return 'Password: minimo 8 caratteri';
    }

    if (!/[A-Z]/.test(password) || !/[a-z]/.test(password) || !/[0-9]/.test(password) || !/[^a-zA-Z0-9]/.test(password)) {
        return passwordHint;
    }

    return '';
}

onMounted(async () => {
    await loadPage();
});

async function loadPage() {
    loading.value = true;
    notificationLoading.value = true;
    try {
        const [overviewResponse, settingsResponse] = await Promise.all([UserService.getAdminOverview(), UserService.getNotificationSettings()]);
        overview.value = overviewResponse;
        notificationSettings.value = settingsResponse;
    } catch (error) {
        console.error('Admin users load failed', error);
        toast.add({ severity: 'error', summary: 'Errore', detail: 'Impossibile caricare utenti e notifiche', life: 5000 });
    } finally {
        loading.value = false;
        notificationLoading.value = false;
    }
}

async function createUser() {
    const validationError = validateNewUserInput();
    if (validationError) {
        toast.add({ severity: 'warn', summary: 'Controlla i campi', detail: validationError, life: 6000 });
        return;
    }

    const username = newUser.value.username.trim();

    try {
        await UserService.createUser(
            {
                username,
                password: newUser.value.password,
                email: `${username}@dash2a.local`,
                description: '',
                isAdmin: newUser.value.roleName === 'Admin'
            },
            newUser.value.roleName
        );
        newUser.value = { username: '', password: '', roleName: 'User' };
        toast.add({ severity: 'success', summary: 'Utente creato', detail: 'Account aggiunto correttamente', life: 3000 });
        await loadPage();
    } catch (error) {
        console.error('Create user failed', error);
        toast.add({ severity: 'error', summary: 'Errore', detail: readApiError(error, 'Creazione utente fallita'), life: 7000 });
    }
}

async function saveNotification(setting) {
    try {
        await UserService.saveNotificationSetting(setting);
        toast.add({ severity: 'success', summary: 'Salvato', detail: `Notifiche aggiornate per ${setting.username}`, life: 2500 });
    } catch (error) {
        console.error('Save notification failed', error);
        toast.add({ severity: 'error', summary: 'Errore', detail: 'Salvataggio notifiche fallito', life: 5000 });
    }
}

async function testNotification(setting) {
    try {
        await saveNotification(setting);
        await UserService.sendNotificationTest(setting.userId);
        toast.add({ severity: 'success', summary: 'Test inviato', detail: setting.notificationEmail || setting.loginEmail, life: 4000 });
    } catch (error) {
        const detail = error?.response?.data?.message || error?.message || 'Invio test fallito';
        toast.add({ severity: 'error', summary: 'Email non inviata', detail, life: 7000 });
    }
}

async function openAccessReport(row) {
    selectedUser.value = row;
    accessDialog.value = true;
    accessLoading.value = true;
    try {
        accessEvents.value = await UserService.getAccessReport(row.userId);
    } catch (error) {
        console.error('Access report failed', error);
        toast.add({ severity: 'error', summary: 'Errore', detail: 'Report accessi non disponibile', life: 5000 });
    } finally {
        accessLoading.value = false;
    }
}

function showDetails(row) {
    toast.add({ severity: 'info', summary: row.username, detail: `${row.accountType} · ${row.role} · ${row.status}`, life: 4000 });
}

async function toggleEnabled(row) {
    const action = row.enabled ? 'disattivare' : 'riattivare';
    const confirmed = window.confirm(`Confermi di voler ${action} ${row.username}?`);
    if (!confirmed) return;

    try {
        if (row.enabled) {
            await UserService.disableUser(row.userId);
        } else {
            await UserService.enableUser(row.userId);
        }
        toast.add({ severity: 'success', summary: 'Utente aggiornato', detail: `${row.username} ${row.enabled ? 'disattivato' : 'riattivato'}`, life: 3000 });
        await loadPage();
    } catch (error) {
        const detail = error?.response?.data?.message || error?.response?.data?.error || error?.message || 'Operazione non riuscita';
        toast.add({ severity: 'error', summary: 'Errore', detail, life: 6000 });
    }
}

async function deleteRow(row) {
    const confirmed = window.confirm(`Eliminare definitivamente ${row.username}? Questa azione non si annulla.`);
    if (!confirmed) return;

    try {
        await UserService.deleteAdminUser(row.userId);
        toast.add({ severity: 'success', summary: 'Utente eliminato', detail: `${row.username} rimosso`, life: 3000 });
        await loadPage();
    } catch (error) {
        const detail = error?.response?.data?.message || error?.response?.data?.error || error?.message || 'Eliminazione non riuscita';
        toast.add({ severity: 'error', summary: 'Errore', detail, life: 6000 });
    }
}

function formatDate(value) {
    if (!value) return '-';
    return new Date(value).toLocaleString('it-IT');
}
</script>

<template>
    <div class="flex flex-col gap-4">
        <div class="card">
            <div class="flex flex-col md:flex-row md:items-center gap-2 mb-4">
                <div>
                    <h3 class="m-0">Impostazioni Email Utenti</h3>
                    <p class="text-muted-color mt-2 mb-0">Gestione destinatari notifiche per utente</p>
                </div>
                <Tag class="md:ml-auto" value="Impostazioni caricate" severity="success" />
            </div>

            <DataTable :value="notificationSettings" :loading="notificationLoading" dataKey="userId" responsiveLayout="scroll" breakpoint="960px">
                <Column field="userId" header="UserId" />
                <Column field="username" header="Username" />
                <Column field="loginEmail" header="Email login">
                    <template #body="{ data }">{{ data.loginEmail || '-' }}</template>
                </Column>
                <Column header="Email notifiche" :style="{ minWidth: '230px' }">
                    <template #body="{ data }">
                        <InputText v-model="data.notificationEmail" placeholder="email@dominio" class="w-full" />
                    </template>
                </Column>
                <Column header="Enabled"
                    ><template #body="{ data }"><Checkbox v-model="data.enabled" binary /></template
                ></Column>
                <Column header="Mission"
                    ><template #body="{ data }"><Checkbox v-model="data.mission" binary /></template
                ></Column>
                <Column header="System"
                    ><template #body="{ data }"><Checkbox v-model="data.system" binary /></template
                ></Column>
                <Column header="Errors"
                    ><template #body="{ data }"><Checkbox v-model="data.errors" binary /></template
                ></Column>
                <Column header="Azioni" :style="{ minWidth: '150px' }">
                    <template #body="{ data }">
                        <div class="flex gap-2">
                            <Button label="Salva" size="small" @click="saveNotification(data)" />
                            <Button label="Test" size="small" severity="secondary" outlined @click="testNotification(data)" />
                        </div>
                    </template>
                </Column>
            </DataTable>
        </div>

        <div class="card">
            <div class="mb-4">
                <h3 class="m-0">Gestione Utenti</h3>
                <p class="text-muted-color mt-2 mb-0">Creazione, accessi live (refresh ~30s); report con esclusione /admin di default.</p>
            </div>

            <div class="rounded border border-surface-200 dark:border-surface-700 p-4 mb-4">
                <h4 class="mt-0">Nuovo Utente</h4>
                <p class="text-muted-color mt-1">Username + password</p>
                <p class="text-muted-color mt-1 mb-0 text-sm">{{ passwordHint }}</p>
                <div class="grid grid-cols-1 md:grid-cols-4 gap-3 items-end mt-3">
                    <div class="flex flex-col gap-2">
                        <label class="font-semibold">Username</label>
                        <InputText v-model="newUser.username" placeholder="Username" />
                    </div>
                    <div class="flex flex-col gap-2">
                        <label class="font-semibold">Password</label>
                        <Password v-model="newUser.password" placeholder="Password" :feedback="false" toggleMask />
                    </div>
                    <div class="flex flex-col gap-2">
                        <label class="font-semibold">Ruolo</label>
                        <Select v-model="newUser.roleName" :options="roleOptions" />
                    </div>
                    <Button label="Aggiungi" icon="pi pi-plus" @click="createUser" />
                </div>
            </div>

            <section class="mb-5">
                <h4 class="m-0">Utenti — accessi operative</h4>
                <p class="text-muted-color mt-2">Colonne sintetiche; fonti registrazione e flag account in Dettagli. Report accessi nella colonna Azioni.</p>
                <UserAccessTable :rows="overview.operative" :loading="loading" @details="showDetails" @report="openAccessReport" @toggle-enabled="toggleEnabled" @delete="deleteRow" />
            </section>

            <section class="mb-5">
                <h4 class="m-0">Bot — telemetria account servizio</h4>
                <p class="text-muted-color mt-2">Profili di servizio; stesso layout compatto degli operative.</p>
                <UserAccessTable :rows="overview.bots" :loading="loading" @details="showDetails" @report="openAccessReport" @toggle-enabled="toggleEnabled" @delete="deleteRow" />
            </section>

            <section>
                <h4 class="m-0">Amministratori — gestione account</h4>
                <p class="text-muted-color mt-2">Senza report accessi operative da questo blocco (solo gestione ruoli/stato).</p>
                <UserAccessTable :rows="overview.admins" :loading="loading" admin-block @details="showDetails" @report="openAccessReport" @toggle-enabled="toggleEnabled" @delete="deleteRow" />
            </section>
        </div>

        <Dialog v-model:visible="accessDialog" :header="`Report accessi - ${selectedUser?.username || ''}`" modal :style="{ width: '900px', maxWidth: '95vw' }">
            <DataTable :value="accessEvents" :loading="accessLoading" responsiveLayout="scroll" breakpoint="960px">
                <Column field="occurredAtUtc" header="Quando"
                    ><template #body="{ data }">{{ formatDate(data.occurredAtUtc) }}</template></Column
                >
                <Column field="eventType" header="Evento" />
                <Column field="ipAddress" header="IP" />
                <Column field="page" header="Pagina" />
                <Column field="userAgent" header="User agent" />
                <template #empty><div class="text-center py-5 text-muted-color">Nessun accesso registrato.</div></template>
            </DataTable>
        </Dialog>
    </div>
</template>
