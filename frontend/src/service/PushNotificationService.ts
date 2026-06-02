import { apiClient } from '@/api/apiClient';
import {
    getPushUiCopy,
    isEdgeAndroid,
    isPushActive,
    isValidPushEndpoint,
    resolvePushUiState,
    type PushUiState
} from '@/utils/pushDiagnostics';

export interface PushStatus {
    supported: boolean;
    permission: NotificationPermission | 'unsupported';
    configured: boolean;
    subscribed: boolean;
    uiState: PushUiState;
    edgeAndroid: boolean;
    invalidEndpoint: boolean;
    message: string;
    stateLabel: string;
    guidance: string;
}

function unsupported(message: string): PushStatus {
    const uiState = 'unsupported' as const;
    const copy = getPushUiCopy(uiState);
    return {
        supported: false,
        permission: 'unsupported',
        configured: false,
        subscribed: false,
        uiState,
        edgeAndroid: false,
        invalidEndpoint: false,
        message: message || copy.message,
        stateLabel: copy.label,
        guidance: copy.guidance || ''
    };
}

function buildStatus(input: {
    supported: boolean;
    permission: NotificationPermission | 'unsupported';
    configured: boolean;
    hasSubscription: boolean;
    endpoint: string | null;
    messageOverride?: string;
}): PushStatus {
    const edgeAndroid = isEdgeAndroid();
    const endpointValid = isValidPushEndpoint(input.endpoint);
    const invalidEndpoint = Boolean(input.endpoint) && !endpointValid;
    const uiState = resolvePushUiState({
        supported: input.supported,
        permission: input.permission,
        configured: input.configured,
        hasSubscription: input.hasSubscription,
        endpointValid,
        edgeAndroid
    });
    const copy = getPushUiCopy(uiState);
    const subscribed = isPushActive(uiState);

    return {
        supported: input.supported,
        permission: input.permission,
        configured: input.configured,
        subscribed,
        uiState,
        edgeAndroid,
        invalidEndpoint,
        message: input.messageOverride || copy.message,
        stateLabel: copy.label,
        guidance: copy.guidance || ''
    };
}

function urlBase64ToUint8Array(base64String: string): Uint8Array {
    const padding = '='.repeat((4 - (base64String.length % 4)) % 4);
    const base64 = (base64String + padding).replace(/-/g, '+').replace(/_/g, '/');
    const rawData = window.atob(base64);
    const outputArray = new Uint8Array(rawData.length);

    for (let i = 0; i < rawData.length; i += 1) {
        outputArray[i] = rawData.charCodeAt(i);
    }

    return outputArray;
}

async function getPublicKey(): Promise<string | null> {
    const response = await apiClient.get('/api/push/vapid-public-key');
    const data = response.data?.data || response.data;
    return data?.enabled && data?.publicKey ? data.publicKey : null;
}

async function registerWorker(): Promise<ServiceWorkerRegistration> {
    return navigator.serviceWorker.register('/service-worker.js');
}

async function saveSubscription(subscription: PushSubscription): Promise<void> {
    await apiClient.post('/api/push/subscribe', subscription.toJSON());
}

async function clearInvalidSubscription(registration: ServiceWorkerRegistration): Promise<void> {
    const existing = await registration.pushManager.getSubscription();
    if (!existing) return;
    if (!isValidPushEndpoint(existing.endpoint)) {
        await existing.unsubscribe();
    }
}

export const PushNotificationService = {
    async getStatus(): Promise<PushStatus> {
        if (!('Notification' in window) || !('serviceWorker' in navigator) || !('PushManager' in window)) {
            return unsupported('Push non supportate da questo browser.');
        }

        const permission = Notification.permission;

        try {
            const publicKey = await getPublicKey();
            if (!publicKey) {
                return buildStatus({
                    supported: true,
                    permission,
                    configured: false,
                    hasSubscription: false,
                    endpoint: null
                });
            }

            const registration = await registerWorker();
            await clearInvalidSubscription(registration);
            const subscription = await registration.pushManager.getSubscription();
            const endpoint = subscription?.endpoint ?? null;

            return buildStatus({
                supported: true,
                permission,
                configured: true,
                hasSubscription: Boolean(subscription),
                endpoint
            });
        } catch {
            return buildStatus({
                supported: true,
                permission,
                configured: false,
                hasSubscription: false,
                endpoint: null,
                messageOverride: 'Endpoint backend push non disponibile.'
            });
        }
    },

    async subscribe(): Promise<PushStatus> {
        if (!('Notification' in window) || !('serviceWorker' in navigator) || !('PushManager' in window)) {
            return unsupported('Push non supportate da questo browser.');
        }

        if (isEdgeAndroid()) {
            return buildStatus({
                supported: true,
                permission: Notification.permission,
                configured: true,
                hasSubscription: false,
                endpoint: null
            });
        }

        const publicKey = await getPublicKey();
        if (!publicKey) {
            return buildStatus({
                supported: true,
                permission: Notification.permission,
                configured: false,
                hasSubscription: false,
                endpoint: null
            });
        }

        if (Notification.permission === 'denied') {
            return buildStatus({
                supported: true,
                permission: 'denied',
                configured: true,
                hasSubscription: false,
                endpoint: null
            });
        }

        const permission = await Notification.requestPermission();
        if (permission !== 'granted') {
            return buildStatus({
                supported: true,
                permission,
                configured: true,
                hasSubscription: false,
                endpoint: null
            });
        }

        const registration = await registerWorker();
        await clearInvalidSubscription(registration);

        let subscription = await registration.pushManager.getSubscription();
        if (!subscription) {
            subscription = await registration.pushManager.subscribe({
                userVisibleOnly: true,
                applicationServerKey: urlBase64ToUint8Array(publicKey)
            });
        }

        if (!isValidPushEndpoint(subscription.endpoint)) {
            await subscription.unsubscribe();
            return buildStatus({
                supported: true,
                permission,
                configured: true,
                hasSubscription: true,
                endpoint: subscription.endpoint
            });
        }

        try {
            await saveSubscription(subscription);
        } catch (err: unknown) {
            const apiMessage =
                typeof err === 'object' &&
                err !== null &&
                'response' in err &&
                typeof (err as { response?: { data?: { message?: string } } }).response?.data?.message === 'string'
                    ? (err as { response: { data: { message: string } } }).response.data.message
                    : null;
            if (apiMessage) {
                await subscription.unsubscribe().catch(() => undefined);
                return buildStatus({
                    supported: true,
                    permission,
                    configured: true,
                    hasSubscription: true,
                    endpoint: subscription.endpoint,
                    messageOverride: apiMessage
                });
            }
            throw err;
        }

        return buildStatus({
            supported: true,
            permission,
            configured: true,
            hasSubscription: true,
            endpoint: subscription.endpoint
        });
    },

    async sendTest(deepLinkPath: string): Promise<{ sent: number; message: string }> {
        const response = await apiClient.post('/api/push/test', { url: deepLinkPath });
        const data = response.data?.data || response.data;
        const sent = Number(data?.sent ?? 0);
        const message =
            typeof response.data?.message === 'string'
                ? response.data.message
                : sent > 0
                  ? 'Notifica di prova inviata.'
                  : 'Nessuna subscription attiva trovata per il tuo utente.';
        return { sent, message };
    }
};
