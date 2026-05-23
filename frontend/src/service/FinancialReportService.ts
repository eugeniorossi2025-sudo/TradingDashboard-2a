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

export interface RuntimeModeInfo {
    runtimeMode: string;
    isDemoMode: boolean;
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

    async getRangeReport(runtimeMode: 'Production' | 'Demo', from: string, to: string): Promise<MissionRangeReport> {
        try {
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
        } catch (error) {
            return buildFallbackReport(runtimeMode, from, to);
        }
    },

    async downloadCsv(runtimeMode: 'Production' | 'Demo', from: string, to: string): Promise<void> {
        let blob: Blob;

        try {
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
        } catch (error) {
            const report = await buildFallbackReport(runtimeMode, from, to);
            const csv = [
                'RuntimeMode,DateTime,Account,Tavolo,Margine,MediaOra,Stato',
                ...report.samples.map(sample => [
                    runtimeMode,
                    sample.dateTime,
                    sample.account || '',
                    sample.tavolo || '',
                    sample.margine,
                    sample.mediaOra || '',
                    sample.stato || ''
                ].map(value => `"${String(value).replace(/"/g, '""')}"`).join(','))
            ].join('\n');
            blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
        }

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
    }
};

async function buildFallbackReport(runtimeMode: 'Production' | 'Demo', from: string, to: string): Promise<MissionRangeReport> {
    if (runtimeMode === 'Demo') {
        return emptyReport(runtimeMode, from, to);
    }

    try {
        const response = await apiClient.get('/api/Dashboard/margini-chart');
        const points = unwrap<any[]>(response) || [];
        const samples = points.map(point => ({
            dateTime: point.timestamp || point.dateTime || point.date || new Date().toISOString(),
            account: point.account || null,
            tavolo: point.tavolo || null,
            margine: Number(point.margine ?? point.margin ?? 0),
            mediaOra: point.mediaOra ?? null,
            stato: point.stato || null
        }));

        const totalMargin = samples.length ? samples[samples.length - 1].margine : 0;

        return {
            runtimeMode,
            isDemoMode: false,
            from,
            to,
            totals: {
                totalMarginEuro: totalMargin,
                globalTargetEuro: 0,
                progressPct: 0,
                sampleCount: samples.length,
                margineMin: samples.length ? Math.min(...samples.map(sample => sample.margine)) : 0,
                margineMax: samples.length ? Math.max(...samples.map(sample => sample.margine)) : 0
            },
            samples
        };
    } catch (error) {
        return emptyReport(runtimeMode, from, to);
    }
}

function emptyReport(runtimeMode: 'Production' | 'Demo', from: string, to: string): MissionRangeReport {
    return {
        runtimeMode,
        isDemoMode: runtimeMode === 'Demo',
        from,
        to,
        totals: {
            totalMarginEuro: 0,
            globalTargetEuro: 0,
            progressPct: 0,
            sampleCount: 0,
            margineMin: 0,
            margineMax: 0
        },
        samples: []
    };
}
