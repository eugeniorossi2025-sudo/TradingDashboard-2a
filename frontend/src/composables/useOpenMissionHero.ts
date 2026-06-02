import { FinancialReportService, type MissionLifecycleState } from '@/service/FinancialReportService';
import { computed, ref, type Ref } from 'vue';

/**
 * Hero mobile: missione aperta da /api/mission/current; altrimenti margine live dai tavoli.
 * I report periodo restano separati (target periodo/report).
 */
export function useOpenMissionHero(liveMarginSum: Ref<number>) {
    const currentMission = ref<MissionLifecycleState | null>(null);

    async function loadCurrentMission(): Promise<void> {
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

    const missionTarget = computed(() => (hasOpenMission.value ? Number(currentMission.value?.globalTarget ?? 0) : 0));

    const heroProgress = computed(() => {
        if (!hasOpenMission.value || missionTarget.value <= 0) return 0;
        return (heroMargin.value / missionTarget.value) * 100;
    });

    const heroProgressClamped = computed(() => Math.max(0, Math.min(100, heroProgress.value)));

    const heroRemaining = computed(() => (hasOpenMission.value ? Math.max(0, missionTarget.value - heroMargin.value) : 0));

    /** Target line on mission chart: mission target when open, else 0 (period target only in report cards). */
    const chartTarget = computed(() => missionTarget.value);

    const heroNote = computed(() => {
        if (hasOpenMission.value) {
            return `Missione #${heroSessionId.value} aperta · target missione ${missionTarget.value} €`;
        }
        return 'Nessuna missione aperta · margine dai tavoli live';
    });

    const heroProgressLabel = computed(() => {
        if (!hasOpenMission.value) return 'Avanzamento missione non disponibile';
        return `${heroProgress.value.toFixed(1)}% del target missione corrente`;
    });

    return {
        currentMission,
        loadCurrentMission,
        hasOpenMission,
        heroSessionId,
        heroMargin,
        missionTarget,
        heroProgress,
        heroProgressClamped,
        heroRemaining,
        chartTarget,
        heroNote,
        heroProgressLabel
    };
}
