import { ConfigurationService } from '@/service/ConfigurationService';
import { ref } from 'vue';

const STOP_WIN_KEY = 'STOP_WIN';

export function parseStopWinEuro(value: string | null | undefined): number {
    if (value == null) return 0;
    const normalized = String(value).trim().replace(',', '.');
    const parsed = Number(normalized);
    return Number.isFinite(parsed) && parsed > 0 ? parsed : 0;
}

/**
 * Operational Stop Win from live Configuration (STOP_WIN key).
 * Used by mobile hero/progress — not historical mission GlobalTarget.
 */
export function useStopWinConfig() {
    const stopWinEuro = ref(0);

    async function loadStopWin(): Promise<void> {
        try {
            const configs = await ConfigurationService.getConfigurations();
            const row = configs.find((c) => c.k === STOP_WIN_KEY);
            stopWinEuro.value = parseStopWinEuro(row?.value);
        } catch (error) {
            console.warn('STOP_WIN config unavailable:', error);
            stopWinEuro.value = 0;
        }
    }

    return { stopWinEuro, loadStopWin };
}
