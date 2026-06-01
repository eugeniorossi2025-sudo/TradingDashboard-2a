export const ROME_TIME_ZONE = 'Europe/Rome';

const dateTimeFormatter = new Intl.DateTimeFormat('it-IT', {
    timeZone: ROME_TIME_ZONE,
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
});

const timeFormatter = new Intl.DateTimeFormat('it-IT', {
    timeZone: ROME_TIME_ZONE,
    hour: '2-digit',
    minute: '2-digit'
});

const dateFormatter = new Intl.DateTimeFormat('it-IT', {
    timeZone: ROME_TIME_ZONE,
    day: '2-digit',
    month: '2-digit',
    year: 'numeric'
});

const isoDatePartsFormatter = new Intl.DateTimeFormat('en-CA', {
    timeZone: ROME_TIME_ZONE,
    year: 'numeric',
    month: '2-digit',
    day: '2-digit'
});

function asDate(value: string | number | Date | null | undefined): Date | null {
    if (!value) return null;
    const date = value instanceof Date ? value : new Date(value);
    return Number.isNaN(date.getTime()) ? null : date;
}

export function formatRomeDateTime(value: string | number | Date | null | undefined): string {
    const date = asDate(value);
    return date ? dateTimeFormatter.format(date) : '-';
}

export function formatRomeTime(value: string | number | Date | null | undefined = new Date()): string {
    const date = asDate(value);
    return date ? timeFormatter.format(date) : '--';
}

export function formatRomeDate(value: string | number | Date | null | undefined): string {
    const date = asDate(value);
    return date ? dateFormatter.format(date) : '-';
}

export function toRomeIsoDate(value: string | number | Date | null | undefined = new Date()): string {
    const date = asDate(value) ?? new Date();
    const parts = isoDatePartsFormatter.formatToParts(date);
    const year = parts.find((part) => part.type === 'year')?.value ?? '1970';
    const month = parts.find((part) => part.type === 'month')?.value ?? '01';
    const day = parts.find((part) => part.type === 'day')?.value ?? '01';
    return `${year}-${month}-${day}`;
}
