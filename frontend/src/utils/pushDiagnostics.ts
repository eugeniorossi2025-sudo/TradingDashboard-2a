export type PushUiState =
    | 'active'
    | 'permission-default'
    | 'permission-denied'
    | 'unsupported'
    | 'invalid-subscription'
    | 'edge-android'
    | 'backend-not-ready'
    | 'ready-to-subscribe';

export function isEdgeAndroid(): boolean {
    if (typeof navigator === 'undefined') return false;
    const ua = navigator.userAgent || '';
    return /android/i.test(ua) && (/\bedg\//i.test(ua) || /\bedge\//i.test(ua) || /\bedga\//i.test(ua));
}

export function isValidPushEndpoint(endpoint: string | null | undefined): boolean {
    if (!endpoint || !endpoint.startsWith('https://')) return false;
    const lower = endpoint.toLowerCase();
    if (lower.includes('permanently-removed.invalid')) return false;
    return true;
}

export function resolvePushUiState(input: {
    supported: boolean;
    permission: NotificationPermission | 'unsupported';
    configured: boolean;
    hasSubscription: boolean;
    endpointValid: boolean;
    edgeAndroid: boolean;
}): PushUiState {
    const { supported, permission, configured, hasSubscription, endpointValid, edgeAndroid } = input;

    if (!supported || permission === 'unsupported') return 'unsupported';
    if (edgeAndroid) return 'edge-android';
    if (hasSubscription && !endpointValid) return 'invalid-subscription';
    if (hasSubscription && endpointValid && permission === 'granted' && configured) return 'active';
    if (permission === 'denied') return 'permission-denied';
    if (permission === 'default') return 'permission-default';
    if (!configured) return 'backend-not-ready';
    return 'ready-to-subscribe';
}

const UI_COPY: Record<
    PushUiState,
    { label: string; message: string; guidance?: string }
> = {
    active: {
        label: 'Notifiche attive',
        message: 'Subscription valida. Usa «Invia notifica di prova» per verificare subito la consegna.'
    },
    'permission-default': {
        label: 'Permesso non ancora richiesto',
        message: 'Tocca «Consenti notifiche» e accetta nel popup del browser.',
        guidance:
            'Android: consenti anche le notifiche dell’app browser (Impostazioni → App → Chrome → Notifiche). Consigliato: Chrome Android o app aggiunta alla schermata Home.'
    },
    'permission-denied': {
        label: 'Notifiche bloccate dal browser',
        message: 'Il permesso è stato negato. Il popup non riappare finché non cambi le impostazioni.',
        guidance:
            'Chrome Android: ⋮ → Impostazioni sito → Notifiche → Consenti. Poi Impostazioni Android → App → Chrome → Notifiche ON. Infine torna qui e tocca di nuovo «Consenti notifiche».'
    },
    unsupported: {
        label: 'Browser non supportato',
        message: 'Questo browser non espone Web Push (Notification, Service Worker, PushManager).',
        guidance: 'Usa Chrome Android su https://eugenio-dashboard-2a.web.app oppure installa la PWA dalla schermata Home.'
    },
    'invalid-subscription': {
        label: 'Subscription non valida',
        message: 'Il browser ha registrato un endpoint push non utilizzabile. Le notifiche non arriveranno.',
        guidance:
            'Passa a Chrome Android, revoca il permesso notifiche per questo sito, poi tocca di nuovo «Consenti notifiche».'
    },
    'edge-android': {
        label: 'Edge Android non consigliato',
        message: 'Edge Android non è consigliato per le notifiche push. Utilizzare Chrome Android.',
        guidance:
            'Su Edge le subscription spesso non funzionano (endpoint invalido). Apri lo stesso sito in Chrome, accedi e abilita le notifiche dalla sezione sotto.'
    },
    'backend-not-ready': {
        label: 'Backend push non pronto',
        message: 'Il server non espone ancora le chiavi VAPID. Riprova più tardi o contatta l’amministratore.'
    },
    'ready-to-subscribe': {
        label: 'Pronto per l’attivazione',
        message: 'Backend configurato. Concedi il permesso per salvare la subscription sul server.',
        guidance: 'Dopo «Consenti notifiche», invia subito una prova con il pulsante dedicato.'
    }
};

export function getPushUiCopy(state: PushUiState) {
    return UI_COPY[state];
}

export function isPushActive(state: PushUiState): boolean {
    return state === 'active';
}
