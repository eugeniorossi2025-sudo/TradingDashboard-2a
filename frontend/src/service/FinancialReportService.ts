import { apiClient } from '@/api/apiClient';

export interface MissionReportSample {
    dateTime: string;
    account?: string | null;
    tavolo?: number | null;
    margine: number;
    mediaOra?: number | null;
    stato?: string | null;
}

export interface MissionRangeReport {
    runtimeMode: string;
    isDemoMode: boolean;
    from: string;
    to: string;
    generatedAt?: string;
    totals: {
        totalMarginEuro: number;
        globalTargetEuro: number;
        progressPct: number;
        sampleCount: number;
        margineMin: number;
        margineMax: number;
        periodReturnPct?: number;
        annualisedReturnPct?: number;
        averageDailyPnl?: number;
        averageDailyReturnPct?: number;
        workingDays?: number;
        reportingDays?: number;
    };
    qualityMetrics?: Record<string, number>;
    dailyRows?: Array<Record<string, string | number>>;
    samples: MissionReportSample[];
}

export interface MissionReportIndexItem {
    sessionId: number;
    startUtc: string;
    endUtc?: string | null;
    completed: boolean;
    runtimeMode: string;
    totalMarginEuro: number;
    globalTargetEuro: number;
    kFactor: number;
    activeTables: number;
    realHandsCount: number;
    samplesCount: number;
}

export interface MissionReportsIndex {
    serverUtc: string;
    total: number;
    skip: number;
    limit: number;
    items: MissionReportIndexItem[];
}

export interface RuntimeModeInfo {
    runtimeMode: string;
    isDemoMode: boolean;
}

export interface MissionLifecycleState {
    hasOpenMission: boolean;
    sessionId?: number | null;
    runtimeMode: string;
    startTime?: string | null;
    endTime?: string | null;
    currentMargin: number;
    totalMargin: number;
    globalTarget: number;
    activeTables: number;
    realHandsCount: number;
    samplesCount: number;
    completed: boolean;
    finalizationReason?: string | null;
}

export interface MissionLifecycleResult {
    success: boolean;
    message: string;
    missionStarted: boolean;
    missionFinalized: boolean;
    missionSessionId?: number | null;
    emailSent: number;
    mission?: MissionLifecycleState | null;
}

function unwrap<T>(response: { data: T | { data?: T } }): T {
    const body = response.data as T | { data?: T };
    if (body && typeof body === 'object' && 'data' in body) {
        return (body as { data?: T }).data as T;
    }
    return body as T;
}

export const FinancialReportService = {
    async getRuntimeMode(): Promise<RuntimeModeInfo> {
        try {
            const response = await apiClient.get('/api/runtime-mode');
            return unwrap<RuntimeModeInfo>(response);
        } catch (error) {
            return {
                runtimeMode: 'Production',
                isDemoMode: false
            };
        }
    },

    async setRuntimeMode(runtimeMode: 'Production' | 'Demo'): Promise<RuntimeModeInfo> {
        const response = await apiClient.put('/api/runtime-mode', { runtimeMode });
        return unwrap<RuntimeModeInfo>(response);
    },

    async getCurrentMission(): Promise<MissionLifecycleState> {
        const response = await apiClient.get('/api/mission/current');
        return unwrap<MissionLifecycleState>(response);
    },

    async startCurrentMission(): Promise<MissionLifecycleResult> {
        const response = await apiClient.post('/api/mission/start-current', {});
        return unwrap<MissionLifecycleResult>(response);
    },

    async finalizeCurrentMission(reason = 'ManualFinalize'): Promise<MissionLifecycleResult> {
        const response = await apiClient.post('/api/mission/finalize-current', { reason });
        return unwrap<MissionLifecycleResult>(response);
    },

    async getRangeReport(runtimeMode: 'Production' | 'Demo', from: string, to: string): Promise<MissionRangeReport> {
        const response = await apiClient.get('/api/mission/report/range', {
            params: {
                runtimeMode,
                from,
                to,
                format: 'json',
                summary: true
            }
        });

        return unwrap<MissionRangeReport>(response);
    },

    async downloadCsv(runtimeMode: 'Production' | 'Demo', from: string, to: string): Promise<void> {
        let blob: Blob;

        const response = await apiClient.get('/api/mission/report/range', {
            params: {
                runtimeMode,
                from,
                to,
                format: 'csv',
                summary: false
            },
            responseType: 'blob'
        });

        blob = new Blob([response.data], { type: 'text/csv;charset=utf-8;' });

        const link = document.createElement('a');
        link.href = URL.createObjectURL(blob);
        link.setAttribute('download', `report_${runtimeMode}_${from}_${to}.csv`);
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(link.href);
    },

    async openHtmlReport(runtimeMode: 'Production' | 'Demo', from: string, to: string): Promise<void> {
        const response = await apiClient.get('/api/mission/report/range', {
            params: {
                runtimeMode,
                from,
                to,
                format: 'html',
                summary: false
            },
            responseType: 'blob'
        });

        const blob = new Blob([response.data], { type: 'text/html;charset=utf-8;' });
        const url = URL.createObjectURL(blob);
        window.open(url, '_blank', 'noopener,noreferrer');

        window.setTimeout(() => URL.revokeObjectURL(url), 60000);
    },

    async downloadJson(runtimeMode: 'Production' | 'Demo', from: string, to: string): Promise<void> {
        const response = await apiClient.get('/api/mission/report/range', {
            params: {
                runtimeMode,
                from,
                to,
                format: 'json',
                summary: false
            }
        });
        const report = unwrap<MissionRangeReport>(response);
        const blob = new Blob([JSON.stringify(report, null, 2)], { type: 'application/json;charset=utf-8;' });
        const link = document.createElement('a');
        link.href = URL.createObjectURL(blob);
        link.setAttribute('download', `report_${runtimeMode}_${from}_${to}.json`);
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(link.href);
    },

    async getReportsIndex(runtimeMode: 'Production' | 'Demo' | 'All', from: string, to: string, skip = 0, limit = 100, sessionId?: number): Promise<MissionReportsIndex> {
        const response = await apiClient.get('/api/mission/reports/index', {
            params: {
                runtimeMode,
                fromUtc: from,
                toUtc: to,
                skip,
                limit,
                sessionId,
                completedOnly: true
            }
        });

        return unwrap<MissionReportsIndex>(response);
    },

    async openSessionReport(sessionId: number, format: 'html' | 'json' | 'csv' = 'html'): Promise<void> {
        const response = await apiClient.get(`/api/mission/report/${encodeURIComponent(String(sessionId))}`, {
            params: { format },
            responseType: format === 'json' ? 'json' : 'blob'
        });

        if (format === 'json') {
            const report = unwrap<MissionRangeReport>(response);
            const blob = new Blob([JSON.stringify(report, null, 2)], { type: 'application/json;charset=utf-8;' });
            const link = document.createElement('a');
            link.href = URL.createObjectURL(blob);
            link.setAttribute('download', `mission_session_${sessionId}.json`);
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            URL.revokeObjectURL(link.href);
            return;
        }

        const contentType = format === 'csv' ? 'text/csv;charset=utf-8;' : 'text/html;charset=utf-8;';
        const blob = new Blob([response.data], { type: contentType });
        const url = URL.createObjectURL(blob);

        if (format === 'html') {
            window.open(url, '_blank', 'noopener,noreferrer');
            window.setTimeout(() => URL.revokeObjectURL(url), 60000);
            return;
        }

        const link = document.createElement('a');
        link.href = url;
        link.setAttribute('download', `mission_session_${sessionId}.csv`);
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(url);
    }
};
