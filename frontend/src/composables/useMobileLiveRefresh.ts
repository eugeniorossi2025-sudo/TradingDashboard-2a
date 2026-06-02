import { onMounted, onUnmounted } from 'vue';

const DEFAULT_LIVE_MS = 5000;
const DEFAULT_REPORT_MS = 60_000;

export type MobileLiveRefreshOptions = {
    /** mission/current + dashboard (+ chart) */
    onRefreshLive: () => void | Promise<void>;
    /** /api/mission/report/range (Production + Demo) */
    onRefreshReports: () => void | Promise<void>;
    liveIntervalMs?: number;
    reportIntervalMs?: number;
    /** Log refresh ticks in dev (Network tab / remote WebView console). */
    debug?: boolean;
};

/**
 * Polling + resume refresh for AdminMobileLive / ClientMobile.
 * Pauses intervals when the document is hidden (battery).
 */
export function useMobileLiveRefresh(options: MobileLiveRefreshOptions) {
    const liveMs = options.liveIntervalMs ?? DEFAULT_LIVE_MS;
    const reportMs = options.reportIntervalMs ?? DEFAULT_REPORT_MS;
    const debug = options.debug ?? import.meta.env.DEV;

    let liveTimer: ReturnType<typeof setInterval> | null = null;
    let reportTimer: ReturnType<typeof setInterval> | null = null;
    let liveInFlight = false;
    let reportInFlight = false;

    function log(message: string) {
        if (debug) {
            console.info(`[DASH2A mobile refresh] ${message}`);
        }
    }

    async function refreshLive() {
        if (liveInFlight) return;
        liveInFlight = true;
        log('tick → /api/mission/current + /api/Dashboard/*');
        try {
            await options.onRefreshLive();
        } catch (error) {
            console.warn('[DASH2A mobile refresh] live refresh failed', error);
        } finally {
            liveInFlight = false;
        }
    }

    async function refreshReports() {
        if (reportInFlight) return;
        reportInFlight = true;
        log('tick → /api/mission/report/range');
        try {
            await options.onRefreshReports();
        } catch (error) {
            console.warn('[DASH2A mobile refresh] report refresh failed', error);
        } finally {
            reportInFlight = false;
        }
    }

    function stopPolling() {
        if (liveTimer !== null) {
            clearInterval(liveTimer);
            liveTimer = null;
        }
        if (reportTimer !== null) {
            clearInterval(reportTimer);
            reportTimer = null;
        }
    }

    function startPolling() {
        stopPolling();
        liveTimer = setInterval(() => {
            void refreshLive();
        }, liveMs);
        reportTimer = setInterval(() => {
            void refreshReports();
        }, reportMs);
        log(`polling started (live ${liveMs}ms, reports ${reportMs}ms)`);
    }

    function onVisibilityChange() {
        if (document.visibilityState === 'hidden') {
            log('visibility hidden → pause polling');
            stopPolling();
            return;
        }
        log('visibility visible → refresh + resume polling');
        void refreshLive();
        void refreshReports();
        startPolling();
    }

    function onWindowFocus() {
        log('window focus → refresh live');
        void refreshLive();
    }

    function onPageShow(event: PageTransitionEvent) {
        if (event.persisted) {
            log('pageshow (bfcache) → refresh');
        } else {
            log('pageshow → refresh');
        }
        void refreshLive();
        void refreshReports();
        if (document.visibilityState === 'visible') {
            startPolling();
        }
    }

    onMounted(() => {
        startPolling();
        document.addEventListener('visibilitychange', onVisibilityChange);
        window.addEventListener('focus', onWindowFocus);
        window.addEventListener('pageshow', onPageShow);
    });

    onUnmounted(() => {
        stopPolling();
        document.removeEventListener('visibilitychange', onVisibilityChange);
        window.removeEventListener('focus', onWindowFocus);
        window.removeEventListener('pageshow', onPageShow);
    });

    return {
        refreshLive,
        refreshReports,
        startPolling,
        stopPolling
    };
}
