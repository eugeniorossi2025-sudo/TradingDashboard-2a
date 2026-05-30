import { ref } from 'vue';

export type ReportPeriodChip = 'day' | 'week' | 'month' | 'year';

export const REPORT_PERIOD_CHIPS: { id: ReportPeriodChip; label: string }[] = [
    { id: 'day', label: 'Day' },
    { id: 'week', label: 'Week' },
    { id: 'month', label: 'Month' },
    { id: 'year', label: 'Year' }
];

/** Demo missions in prod are anchored around early 2016 after one-by-one import. */
export const DEMO_REPORT_ANCHOR = '2016-02-18';

function toIsoDate(value: Date): string {
    const year = value.getFullYear();
    const month = String(value.getMonth() + 1).padStart(2, '0');
    const day = String(value.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
}

function parseIsoDate(value: string): Date {
    const [year, month, day] = value.split('-').map(Number);
    return new Date(year, month - 1, day, 12, 0, 0);
}

export function getPeriodRange(chip: ReportPeriodChip, anchor = new Date()): { from: string; to: string } {
    const end = new Date(anchor);
    const start = new Date(anchor);

    switch (chip) {
        case 'day':
            return { from: toIsoDate(end), to: toIsoDate(end) };
        case 'week': {
            const weekday = (end.getDay() + 6) % 7;
            start.setDate(end.getDate() - weekday);
            return { from: toIsoDate(start), to: toIsoDate(end) };
        }
        case 'month':
            start.setDate(1);
            return { from: toIsoDate(start), to: toIsoDate(end) };
        case 'year':
            start.setMonth(0, 1);
            return { from: toIsoDate(start), to: toIsoDate(end) };
        default:
            return { from: toIsoDate(end), to: toIsoDate(end) };
    }
}

export function formatPeriodLabel(from: string, to: string): string {
    const [, fm, fd] = from.split('-');
    const [, tm, td] = to.split('-');
    return from === to ? `${fd}/${fm}` : `${fd}/${fm} - ${td}/${tm}`;
}

export function useReportPeriod(initialChip: ReportPeriodChip = 'month') {
    const periodChip = ref<ReportPeriodChip>(initialChip);
    const from = ref('');
    const to = ref('');
    const demoFrom = ref('');
    const demoTo = ref('');

    function applyPeriodChip(chip: ReportPeriodChip) {
        periodChip.value = chip;
        const production = getPeriodRange(chip, new Date());
        const demo = getPeriodRange(chip, parseIsoDate(DEMO_REPORT_ANCHOR));
        from.value = production.from;
        to.value = production.to;
        demoFrom.value = demo.from;
        demoTo.value = demo.to;
    }

    applyPeriodChip(initialChip);

    return {
        periodChip,
        from,
        to,
        demoFrom,
        demoTo,
        applyPeriodChip,
        formatPeriod: () => formatPeriodLabel(from.value, to.value),
        formatDemoPeriod: () => formatPeriodLabel(demoFrom.value, demoTo.value)
    };
}
