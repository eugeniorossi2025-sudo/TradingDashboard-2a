import { apiClient } from '@/api/apiClient';

export interface PushStatus {
    supported: boolean;
    permission: NotificationPermission | 'unsupported';
    configured: boolean;
    subscribed: boolean;
    message: string;
}

function unsupported(message: string): PushStatus {
    return {
        supported: false,
        permission: 'unsupported',
        configured: false,
        subscribed: false,
        message
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

export const PushNotificationService = {
    async getStatus(): Promise<PushStatus> {
        if (!('Notification' in window) || !('serviceWorker' in navigator) || !('PushManager' in window)) {
            return unsupported('Push non supportate da questo browser.');
        }

        try {
            const publicKey = await getPublicKey();
            if (!publicKey) {
                return {
                    supported: true,
                    permission: Notification.permission,
                    configured: false,
                    subscribed: false,
                    message: 'Permesso browser verificato. Backend push/VAPID non configurato.'
                };
            }

            const registration = await registerWorker();
            const subscription = await registration.pushManager.getSubscription();

            return {
                supported: true,
                permission: Notification.permission,
                configured: true,
                subscribed: Boolean(subscription),
                message: subscription ? 'Subscription salvata sul browser. Push attive.' : 'Backend push configurato. Devi ancora autorizzare e salvare la subscription.'
            };
        } catch (error) {
            return {
                supported: true,
                permission: Notification.permission,
                configured: false,
                subscribed: false,
                message: 'Endpoint backend push non disponibile.'
            };
        }
    },

    async subscribe(): Promise<PushStatus> {
        if (!('Notification' in window) || !('serviceWorker' in navigator) || !('PushManager' in window)) {
            return unsupported('Push non supportate da questo browser.');
        }

        const publicKey = await getPublicKey();
        if (!publicKey) {
            return {
                supported: true,
                permission: Notification.permission,
                configured: false,
                subscribed: false,
                message: 'Permesso browser possibile, ma backend push/VAPID non configurato.'
            };
        }

        const permission = await Notification.requestPermission();
        if (permission !== 'granted') {
            return {
                supported: true,
                permission,
                configured: true,
                subscribed: false,
                message: 'Permesso notifiche non concesso.'
            };
        }

        const registration = await registerWorker();
        let subscription = await registration.pushManager.getSubscription();
        if (!subscription) {
            subscription = await registration.pushManager.subscribe({
                userVisibleOnly: true,
                applicationServerKey: urlBase64ToUint8Array(publicKey)
            });
        }

        await saveSubscription(subscription);

        return {
            supported: true,
            permission,
            configured: true,
            subscribed: true,
            message: 'Subscription salvata sul server. Push attive.'
        };
    }
};
