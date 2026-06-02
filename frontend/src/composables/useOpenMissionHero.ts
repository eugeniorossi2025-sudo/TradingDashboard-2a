import { FinancialReportService, type MissionLifecycleState } from '@/service/FinancialReportService';
import { useStopWinConfig } from '@/composables/useStopWinConfig';
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
