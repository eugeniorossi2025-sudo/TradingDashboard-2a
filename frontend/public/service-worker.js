/* global clients */

self.addEventListener('push', (event) => {
    let payload = {};

    try {
        payload = event.data ? event.data.json() : {};
    } catch {
        payload = {
            title: 'EuGenio Trading',
            body: event.data ? event.data.text() : 'Nuova notifica missione'
        };
    }

    const title = payload.title || 'EuGenio Trading';
    const options = {
        body: payload.body || 'Nuova notifica missione',
        icon: payload.icon || '/demo/images/logo.svg',
        badge: payload.badge || '/demo/images/logo.svg',
        data: payload.data || {}
    };

    event.waitUntil(self.registration.showNotification(title, options));
});

self.addEventListener('notificationclick', (event) => {
    event.notification.close();
    const targetUrl = event.notification.data && event.notification.data.url ? event.notification.data.url : '/admin/mobile-live';

    event.waitUntil(
        (async () => {
            const clientsList = await clients.matchAll({ type: 'window', includeUncontrolled: true });
            for (const client of clientsList) {
                if ('focus' in client) {
                    client.navigate(targetUrl);
                    return client.focus();
                }
            }
            return clients.openWindow(targetUrl);
        })()
    );
});
