import { ref } from 'vue';
import { toRomeIsoDate } from '@/utils/romeTime';

export type ReportPeriodChip = 'day' | 'week' | 'month' | 'year';

export const REPORT_PERIOD_CHIPS: { id: ReportPeriodChip; label: string }[] = [
    { id: 'day', label: 'Day' },
    { id: 'week', label: 'Week' },
    { id: 'month', label: 'Month' },
    { id: 'year', label: 'Year' }
];

function toIsoDate(value: Date): string {
    return toRomeIsoDate(value);
}

export function getPeriodRange(chip: ReportPeriodChip, anchor = new Date()): { from: string; to: string } {
    const [romeYear, romeMonth, romeDay] = toRomeIsoDate(anchor).split('-').map(Number);
    const end = new Date(Date.UTC(romeYear, romeMonth - 1, romeDay, 12, 0, 0));
    const start = new Date(end);

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
        from.value = production.from;
        to.value = production.to;
        demoFrom.value = production.from;
        demoTo.value = production.to;
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
