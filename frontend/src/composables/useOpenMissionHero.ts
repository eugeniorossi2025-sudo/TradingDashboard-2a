import { FinancialReportService, type MissionLifecycleState } from '@/service/FinancialReportService';
import { useStopWinConfig } from '@/composables/useStopWinConfig';
import { formatRomeDateTime } from '@/utils/romeTime';
import { computed, ref, type Ref } from 'vue';

/**
 * Mobile hero: live margin + progress vs current STOP_WIN from Configuration.
 * Period reports stay separate (no historical max target on operational screens).
 */
export function useOpenMissionHero(liveMarginSum: Ref<number>) {
    const currentMission = ref<MissionLifecycleState | null>(null);
    const { stopWinEuro, loadStopWin } = useStopWinConfig();

    async function loadCurrentMission(): Promise<void> {
        await loadStopWin();
        try {
            currentMission.value = await FinancialReportService.getCurrentMission();
        } catch (error) {
            console.warn('Open mission state unavailable:', error);
            currentMission.value = null;
        }
    }

    const hasOpenMission = computed(() => Boolean(currentMission.value?.hasOpenMission));

    const heroSessionId = computed(() => (hasOpenMission.value ? currentMission.value?.sessionId ?? null : null));

    function parseServerUtcDate(value?: string | null): Date | null {
        if (!value) return null;
        const normalized = /(?:Z|[+-]\d{2}:?\d{2})$/i.test(value) ? value : `${value}Z`;
        const date = new Date(normalized);
        return Number.isNaN(date.getTime()) ? null : date;
    }

    function formatDuration(ms: number): string {
        const totalSeconds = Math.max(0, Math.floor(ms / 1000));
        const hours = Math.floor(totalSeconds / 3600);
        const minutes = Math.floor((totalSeconds % 3600) / 60);
        const seconds = totalSeconds % 60;

        if (hours > 0) return `${hours}h ${String(minutes).padStart(2, '0')}m`;
        if (minutes > 0) return `${minutes}m ${String(seconds).padStart(2, '0')}s`;
        return `${seconds}s`;
    }

    const missionStartedAt = computed(() => (hasOpenMission.value ? currentMission.value?.startTime ?? null : null));

    const missionStartedAtLabel = computed(() => (missionStartedAt.value ? formatRomeDateTime(missionStartedAt.value) : '—'));

    const missionElapsedMs = computed(() => {
        if (!hasOpenMission.value) return 0;
        const start = parseServerUtcDate(currentMission.value?.startTime);
        if (!start) return 0;
        const end = parseServerUtcDate(currentMission.value?.endTime) ?? new Date();
        return end.getTime() - start.getTime();
    });

    const missionElapsedLabel = computed(() => (hasOpenMission.value && missionElapsedMs.value > 0 ? formatDuration(missionElapsedMs.value) : '—'));

    const heroMargin = computed(() => {
        if (hasOpenMission.value) {
            return Number(currentMission.value?.currentMargin ?? 0);
        }
        return liveMarginSum.value;
    });

    /** Operational target: always current STOP_WIN, not frozen MissionSessions.GlobalTarget. */
    const missionTarget = computed(() => stopWinEuro.value);

    const heroProgress = computed(() => {
        if (!hasOpenMission.value || missionTarget.value <= 0) return 0;
        return (heroMargin.value / missionTarget.value) * 100;
    });

    const heroProgressClamped = computed(() => Math.max(0, Math.min(100, heroProgress.value)));

    const heroRemaining = computed(() => (hasOpenMission.value ? Math.max(0, missionTarget.value - heroMargin.value) : 0));

    const chartTarget = computed(() => missionTarget.value);

    const heroNote = computed(() => {
        if (hasOpenMission.value) {
            return `Missione #${heroSessionId.value} aperta`;
        }
        return 'Nessuna missione aperta · margine dai tavoli live';
    });

    const heroProgressLabel = computed(() => {
        if (!hasOpenMission.value) return 'Avanzamento missione non disponibile';
        if (missionTarget.value <= 0) return 'Stop Win attuale non configurato';
        return `${heroProgress.value.toFixed(1)}% verso Stop Win attuale`;
    });

    return {
        currentMission,
        loadCurrentMission,
        hasOpenMission,
        heroSessionId,
        missionStartedAt,
        missionStartedAtLabel,
        missionElapsedMs,
        missionElapsedLabel,
        heroMargin,
        stopWinEuro,
        missionTarget,
        heroProgress,
        heroProgressClamped,
        heroRemaining,
        chartTarget,
        heroNote,
        heroProgressLabel
    };
}
