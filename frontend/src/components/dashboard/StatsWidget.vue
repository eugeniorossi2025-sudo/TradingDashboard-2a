<script setup>
import { computed, ref, watch } from 'vue';
import { DashboardService } from '@/service/DashboardService';

const props = defineProps({
    telemetry: {
        type: String,
        default: null
    },
    telemetryParsed: {
        type: Object,
        default: null
    }
});

const selectedSecurityFilterBot = ref(null);
const securityFilterBotDetails = ref({});
const securityFilterDetailLoading = ref(false);
const securityFilterDetailUnavailable = ref(false);
const playerStepPulseByBot = ref({});
const playerStreakLastCountByBot = ref({});
const playerPulseTimersByBot = {};

const PLAYER_STEP_PULSE_MS = 1600;

function mergeTelemetryFromApi(api) {
    if (!api || typeof api !== 'object') return {};
    return {
        TotalAuthL6Authorized: api.totalAuthL6Authorized,
        TotalL5Played: api.totalL5Played,
        TotalL5Won: api.totalL5Won,
        TotalL5Lost: api.totalL5Lost,
        TotalPBHandsPlayed: api.totalPbHandsPlayed,
        TotalL8Played: api.totalL8Played,
        TotalL8Won: api.totalL8Won,
        TotalL8Lost: api.totalL8Lost,
        SpotID: api.spotId,
        SpotPBHandsPlayed: api.spotPbHandsPlayed,
        SpotAuthL6Counter: api.spotAuthL6Counter,
        SpotL5Loss: api.spotL5Loss,
        GlobalPauseScalping: api.globalPauseScalping,
        GlobalPauseScalpingDetails: api.globalPauseScalpingDetails,
        GlobalPauseScalpingDuration: api.globalPauseScalpingDuration,
        INC: api.inc,
        EWMA: api.ewma,
        TotalPauseScalpingSoglieActivated: api.totalPauseScalpingSoglieActivated,
        TotalPauseScalpingEWMAActivated: api.totalPauseScalpingEWMAActivated,
        SecurityFilterEnabled: api.securityFilterEnabled,
        SecurityFilterMinScore: api.securityFilterMinScore,
        SecurityFilterMinStreak: api.securityFilterMinStreak,
        SecurityFilterMaxShoeHand: api.securityFilterMaxShoeHand,
        SecurityFilterMaxAvgSeconds: api.securityFilterMaxAvgSeconds,
        SecurityFilterVeryFastSeconds: api.securityFilterVeryFastSeconds,
        SecurityFilterDeltaWindow: api.securityFilterDeltaWindow,
        SecurityFilterPlayerP1P5ThresholdSeconds: api.securityFilterPlayerP1P5ThresholdSeconds,
        TotalSecurityFilterActivated: api.totalSecurityFilterActivated,
        TotalSecurityFilterPreventedL6: api.totalSecurityFilterPreventedL6,
        LastAvgHandSeconds: api.lastAvgHandSeconds,
        ActiveSecurityFilterBots: api.activeSecurityFilterBots,
        SecurityFilterByBot: api.securityFilterByBot ?? api.SecurityFilterByBot
    };
}

function pickRowField(row, pascalKey, camelKey) {
    if (!row) return undefined;
    const pascalValue = row[pascalKey];
    if (pascalValue !== undefined && pascalValue !== null) return pascalValue;
    const camelValue = row[camelKey];
    if (camelValue !== undefined && camelValue !== null) return camelValue;
    return undefined;
}

function normalizeSecurityFilterBotRow(computer, row) {
    const base = row && typeof row === 'object' ? row : {};
    return {
        ...base,
        computer,
        Computer: computer,
        PlayerStreakCount: pickRowField(base, 'PlayerStreakCount', 'playerStreakCount'),
        playerStreakCount: pickRowField(base, 'playerStreakCount', 'PlayerStreakCount'),
        PlayerStreakP1ToP5TotalSeconds: pickRowField(base, 'PlayerStreakP1ToP5TotalSeconds', 'playerStreakP1ToP5TotalSeconds'),
        playerStreakP1ToP5TotalSeconds: pickRowField(base, 'playerStreakP1ToP5TotalSeconds', 'PlayerStreakP1ToP5TotalSeconds'),
        PlayerStreakMeanIntervalSeconds: pickRowField(base, 'PlayerStreakMeanIntervalSeconds', 'playerStreakMeanIntervalSeconds'),
        playerStreakMeanIntervalSeconds: pickRowField(base, 'playerStreakMeanIntervalSeconds', 'PlayerStreakMeanIntervalSeconds'),
        PlayerStreakIntervalSeconds: pickRowField(base, 'PlayerStreakIntervalSeconds', 'playerStreakIntervalSeconds'),
        playerStreakIntervalSeconds: pickRowField(base, 'playerStreakIntervalSeconds', 'PlayerStreakIntervalSeconds'),
        CurrentStreakOutcome: pickRowField(base, 'CurrentStreakOutcome', 'currentStreakOutcome'),
        currentStreakOutcome: pickRowField(base, 'currentStreakOutcome', 'CurrentStreakOutcome')
    };
}

function toPascalCaseKeys(obj) {
    if (!obj || typeof obj !== 'object' || Array.isArray(obj)) return obj;
    const out = {};
    for (const [key, value] of Object.entries(obj)) {
        if (!key) continue;
        out[key.charAt(0).toUpperCase() + key.slice(1)] = value;
    }
    return out;
}

const telemetryData = computed(() => {
    let fromRaw = {};
    if (props.telemetry) {
        try {
            fromRaw = JSON.parse(props.telemetry);
        } catch {
            fromRaw = {};
        }
    }

    const fromApi = mergeTelemetryFromApi(props.telemetryParsed);
    const merged = { ...fromApi, ...fromRaw };

    for (const [key, value] of Object.entries(fromApi)) {
        if (value !== undefined && value !== null && (merged[key] === undefined || merged[key] === null)) {
            merged[key] = value;
        }
    }

    const apiBots = fromApi.SecurityFilterByBot;
    const rawBots = fromRaw.SecurityFilterByBot ?? fromRaw.securityFilterByBot;
    if (apiBots || rawBots) {
        const keys = new Set([...Object.keys(rawBots || {}), ...Object.keys(apiBots || {})]);
        const mergedBots = {};
        for (const key of keys) {
            mergedBots[key] = normalizeSecurityFilterBotRow(key, {
                ...(rawBots?.[key] || {}),
                ...(apiBots?.[key] || {})
            });
        }
        merged.SecurityFilterByBot = mergedBots;
        merged.securityFilterByBot = mergedBots;
    }

    return merged;
});

const securityFilterRows = computed(() => {
    const byBot = telemetryData.value?.SecurityFilterByBot ?? telemetryData.value?.securityFilterByBot;
    if (!byBot || typeof byBot !== 'object') return [];

    return Object.entries(byBot)
        .map(([computer, row]) => normalizeSecurityFilterBotRow(computer, row))
        .sort((a, b) => {
            const riskDelta = getRiskRank(b) - getRiskRank(a);
            if (riskDelta !== 0) return riskDelta;

            const levelDelta = getNumber(b?.Martingala) - getNumber(a?.Martingala);
            if (levelDelta !== 0) return levelDelta;

            const avgDelta = getNumber(a?.AvgHandSeconds, Number.MAX_SAFE_INTEGER) - getNumber(b?.AvgHandSeconds, Number.MAX_SAFE_INTEGER);
            if (avgDelta !== 0) return avgDelta;

            return getBotName(a).localeCompare(getBotName(b));
        });
});

watch(
    () =>
        securityFilterRows.value.map((row) => {
            const metrics = getPlayerStreakMetrics(row);
            return {
                bot: getBotName(row),
                count: metrics.count,
                outcome: metrics.outcome
            };
        }),
    (snapshots) => {
        for (const snap of snapshots) {
            if (!snap.bot) continue;

            const prev = playerStreakLastCountByBot.value[snap.bot];
            const isPlayerStreak = snap.count > 0 && (snap.outcome === 'P' || snap.outcome === '');

            if (isPlayerStreak && prev !== undefined && snap.count > prev) {
                triggerPlayerStepPulse(snap.bot, Math.min(snap.count, 5));
            } else if (!isPlayerStreak && prev > 0) {
                clearPlayerStepPulse(snap.bot);
            }

            playerStreakLastCountByBot.value[snap.bot] = isPlayerStreak ? snap.count : 0;
        }
    },
    { deep: true }
);

const selectedSecurityFilterRow = computed(() => {
    if (!selectedSecurityFilterBot.value) return null;

    const summary = securityFilterRows.value.find((row) => getBotName(row) === selectedSecurityFilterBot.value);
    if (!summary) return null;

    const detail = securityFilterBotDetails.value[selectedSecurityFilterBot.value];
    if (!detail) return summary;

    return { ...summary, ...normalizeSecurityFilterBotRow(getBotName(summary), toPascalCaseKeys(detail)) };
});

const securityFilterRiskStrip = computed(() => {
    const riskyRows = securityFilterRows.value.filter((row) => getRiskRank(row) >= 1);
    if (!riskyRows.length) return 'Tutti i bot in stato normale';

    return riskyRows
        .slice(0, 4)
        .map((row) => `${getBotName(row)} ${getRiskStripReason(row)}`)
        .join(' · ');
});

const hasSecurityFilterRisk = computed(() => securityFilterRows.value.some((row) => getRiskRank(row) >= 1));

const securityFilterOperational = computed(() => isSecurityFilterEnabled());

const securityFilterSetup = computed(() => ({
    enabled: securityFilterOperational.value,
    minScore: telemetryData.value?.SecurityFilterMinScore ?? 3,
    minStreak: telemetryData.value?.SecurityFilterMinStreak ?? 5,
    maxShoeHand: telemetryData.value?.SecurityFilterMaxShoeHand ?? 20,
    maxAvgSeconds: telemetryData.value?.SecurityFilterMaxAvgSeconds ?? 25.85,
    veryFastSeconds: telemetryData.value?.SecurityFilterVeryFastSeconds ?? 23.1,
    deltaWindow: telemetryData.value?.SecurityFilterDeltaWindow ?? 8,
    playerP1P5Threshold: resolvePlayerP1P5Threshold(
        telemetryData.value?.SecurityFilterPlayerP1P5ThresholdSeconds
    ),
    preventedL6: telemetryData.value?.TotalSecurityFilterPreventedL6 ?? 0
}));

const securityFilterLastEvent = computed(() => {
    let best = null;
    for (const row of securityFilterRows.value) {
        const reason = row?.LastReason || row?.lastReason;
        const ts = row?.LastUpdatedUtc || row?.lastUpdatedUtc;
        if (!reason && !ts) continue;
        const tsMs = ts ? Date.parse(ts) : 0;
        if (!best || tsMs >= best.tsMs) {
            best = { bot: getBotName(row), reason: reason || '—', ts, tsMs };
        }
    }
    return best;
});

function formatSeconds(value) {
    if (value == null || Number(value) <= 0) return '-';
    return `${Number(value).toFixed(1)}s`;
}

function formatDuration(value) {
    const seconds = Number(value);
    if (!Number.isFinite(seconds) || seconds <= 0) return '-';
    if (seconds < 60) return `${seconds.toFixed(1)}s`;

    const minutes = Math.floor(seconds / 60);
    const remainingSeconds = Math.round(seconds % 60);
    if (minutes < 60) return remainingSeconds > 0 ? `${minutes}m ${remainingSeconds}s` : `${minutes}m`;

    const hours = Math.floor(minutes / 60);
    const remainingMinutes = minutes % 60;
    return remainingMinutes > 0 ? `${hours}h ${remainingMinutes}m` : `${hours}h`;
}

function formatDurationRange(minValue, maxValue) {
    const min = formatDuration(minValue);
    const max = formatDuration(maxValue);
    if (min === '-' && max === '-') return '-';
    return `${min} - ${max}`;
}

const PLAYER_P1P5_THRESHOLD_DEFAULT = 107;

function getNumber(value, fallback = 0) {
    const numberValue = Number(value);
    return Number.isFinite(numberValue) ? numberValue : fallback;
}

function resolvePlayerP1P5Threshold(value) {
    const parsed = getNumber(value, PLAYER_P1P5_THRESHOLD_DEFAULT);
    return parsed > 0 ? parsed : PLAYER_P1P5_THRESHOLD_DEFAULT;
}

function getBotName(row) {
    return String(row?.Computer || row?.computer || '-');
}

function getRiskPillClass(active) {
    return active ? 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-300' : 'bg-surface-100 text-muted-color dark:bg-surface-800';
}

function getScorePoint(active) {
    return active ? '+1' : '+0';
}

function isAvgFast(row) {
    const avg = getNumber(row?.AvgHandSeconds);
    return avg > 0 && avg < getNumber(securityFilterSetup.value.maxAvgSeconds);
}

function isVeryFast(row) {
    const avg = getNumber(row?.AvgHandSeconds);
    return avg > 0 && avg < getNumber(securityFilterSetup.value.veryFastSeconds);
}

function isLastL6AvgFast(row) {
    const avg = getNumber(row?.LastL6AuthorizationAvgHandSeconds);
    return avg > 0 && avg < getNumber(securityFilterSetup.value.maxAvgSeconds);
}

function isLastL6VeryFast(row) {
    const avg = getNumber(row?.LastL6AuthorizationAvgHandSeconds);
    return avg > 0 && avg < getNumber(securityFilterSetup.value.veryFastSeconds);
}

function isStreakRisk(row) {
    return getNumber(row?.CurrentStreak) >= getNumber(securityFilterSetup.value.minStreak);
}

function getPlayerStreakMetrics(row) {
    const count = getNumber(pickRowField(row, 'PlayerStreakCount', 'playerStreakCount'));
    const total = getNumber(pickRowField(row, 'PlayerStreakP1ToP5TotalSeconds', 'playerStreakP1ToP5TotalSeconds'));
    const mean = getNumber(pickRowField(row, 'PlayerStreakMeanIntervalSeconds', 'playerStreakMeanIntervalSeconds'));
    const rawIntervals = pickRowField(row, 'PlayerStreakIntervalSeconds', 'playerStreakIntervalSeconds') ?? [];
    const intervals = Array.isArray(rawIntervals) ? rawIntervals.map((value) => getNumber(value)) : [];
    const outcome = String(pickRowField(row, 'CurrentStreakOutcome', 'currentStreakOutcome') ?? '').trim().toUpperCase();
    const threshold = resolvePlayerP1P5Threshold(securityFilterSetup.value.playerP1P5Threshold);
    return { count, total, mean, intervals, outcome, threshold };
}

function isPlayerStreakRisk(row) {
    const metrics = getPlayerStreakMetrics(row);
    if (metrics.outcome !== 'P') return false;
    if (metrics.count < getNumber(securityFilterSetup.value.minStreak)) return false;
    if (metrics.total <= 0) return false;
    return metrics.total <= metrics.threshold;
}

function hasPlayerPaceTelemetry(row) {
    if (!row) return false;
    const candidates = [
        pickRowField(row, 'PlayerStreakCount', 'playerStreakCount'),
        pickRowField(row, 'PlayerStreakP1ToP5TotalSeconds', 'playerStreakP1ToP5TotalSeconds'),
        pickRowField(row, 'PlayerStreakMeanIntervalSeconds', 'playerStreakMeanIntervalSeconds'),
        pickRowField(row, 'PlayerStreakIntervalSeconds', 'playerStreakIntervalSeconds'),
        pickRowField(row, 'CurrentStreakOutcome', 'currentStreakOutcome')
    ];
    return candidates.some((value) => value !== undefined && value !== null);
}

function formatPlayerPaceSeconds(value) {
    const seconds = getNumber(value);
    if (seconds <= 0) return '--';
    return `${seconds.toFixed(1)}s`;
}

function getPlayerPaceVisual(row) {
    const metrics = getPlayerStreakMetrics(row);
    const available = hasPlayerPaceTelemetry(row);
    const active = available && metrics.count > 0 && (metrics.outcome === 'P' || metrics.outcome === '');

    const steps = [1, 2, 3, 4, 5].map((n) => ({
        label: `P${n}`,
        filled: active && metrics.count >= n
    }));

    const deltas = [1, 2, 3, 4].map((n) => {
        const idx = n - 1;
        const hasNextStep = active && metrics.count >= n + 1;
        const waitingForNext = active && metrics.count === n;
        const seconds = hasNextStep && metrics.intervals[idx] > 0 ? metrics.intervals[idx] : null;
        return {
            label: `P${n}→P${n + 1}`,
            seconds,
            visible: active && metrics.count >= n,
            pending: waitingForNext || (hasNextStep && !(metrics.intervals[idx] > 0))
        };
    });

    let status = 'inactive';
    let statusLabel = 'NORMALE';

    if (!available) {
        status = 'unavailable';
        statusLabel = '';
    } else if (!active) {
        status = 'inactive';
        statusLabel = 'Nessuna streak PLAYER';
    } else if (metrics.count < getNumber(securityFilterSetup.value.minStreak)) {
        status = 'partial';
        statusLabel = `In corso ${metrics.count}/${securityFilterSetup.value.minStreak}`;
    } else if (isPlayerStreakRisk(row)) {
        status = 'risk';
        statusLabel = 'RISCHIO PLAYER';
    } else {
        status = 'normal';
        statusLabel = 'NORMALE';
    }

    return {
        available,
        active,
        steps,
        deltas,
        status,
        statusLabel,
        ...metrics
    };
}

function getPlayerPacePanelClass(row) {
    const visual = getPlayerPaceVisual(row);
    if (visual.status === 'risk') {
        return 'border-orange-300 bg-orange-50 dark:border-orange-700 dark:bg-orange-950/30';
    }
    if (visual.status === 'partial') {
        return 'border-blue-300 bg-blue-50/60 dark:border-blue-800 dark:bg-blue-950/20';
    }
    if (visual.active) {
        return 'border-blue-200 bg-blue-50/40 dark:border-blue-900 dark:bg-blue-950/15';
    }
    return 'border-surface-200 bg-surface-0 dark:border-surface-700 dark:bg-surface-900';
}

function getPlayerPaceStatusClass(row) {
    const visual = getPlayerPaceVisual(row);
    if (visual.status === 'risk') return 'text-orange-700 dark:text-orange-300';
    if (visual.status === 'partial') return 'text-blue-700 dark:text-blue-300';
    if (visual.status === 'normal') return 'text-emerald-700 dark:text-emerald-300';
    return 'text-muted-color';
}

function getPlayerStepNumber(step) {
    return Number.parseInt(String(step?.label || '').replace('P', ''), 10) || 0;
}

function isPlayerStepPulsing(row, step) {
    const bot = getBotName(row);
    if (!bot) return false;
    return playerStepPulseByBot.value[bot] === getPlayerStepNumber(step);
}

function clearPlayerStepPulse(bot) {
    if (playerPulseTimersByBot[bot]) {
        clearTimeout(playerPulseTimersByBot[bot]);
        delete playerPulseTimersByBot[bot];
    }
    if (playerStepPulseByBot.value[bot] === undefined) return;
    const next = { ...playerStepPulseByBot.value };
    delete next[bot];
    playerStepPulseByBot.value = next;
}

function triggerPlayerStepPulse(bot, stepIndex) {
    if (!bot || stepIndex < 1) return;
    clearPlayerStepPulse(bot);
    playerStepPulseByBot.value = { ...playerStepPulseByBot.value, [bot]: stepIndex };
    playerPulseTimersByBot[bot] = setTimeout(() => clearPlayerStepPulse(bot), PLAYER_STEP_PULSE_MS);
}

function getPlayerStepClass(step, row) {
    const pulsing = row && isPlayerStepPulsing(row, step);
    if (step.filled) {
        const base = 'bg-blue-500 text-white shadow-md shadow-blue-500/30 border-blue-600 dark:bg-blue-500 dark:border-blue-400';
        if (pulsing) return `${base} player-step-pulse ring-4 ring-blue-300 dark:ring-blue-400`;
    }
    return 'bg-surface-200 text-muted-color border-surface-300 dark:bg-surface-700 dark:border-surface-600';
}

function getPlayerStepClassCompact(step, row) {
    const pulsing = row && isPlayerStepPulsing(row, step);
    if (step.filled) {
        const base = 'bg-blue-500 text-white';
        if (pulsing) return `${base} player-step-pulse ring-2 ring-blue-300 dark:ring-blue-400`;
        return base;
    }
    return 'bg-surface-200 text-muted-color dark:bg-surface-700';
}

function isShoeRisk(row) {
    return getNumber(row?.LastShoeHand, Number.MAX_SAFE_INTEGER) <= getNumber(securityFilterSetup.value.maxShoeHand);
}

function isAuthToL8PaceFast(row) {
    const secondsPerHand = getNumber(row?.LastAuthorizedL8LossSecondsPerHand);
    return secondsPerHand > 0 && secondsPerHand < getNumber(securityFilterSetup.value.maxAvgSeconds);
}

function formatHands(value, decimals = 0) {
    const hands = Number(value);
    if (!Number.isFinite(hands) || hands <= 0) return '-';
    return `${hands.toFixed(decimals)} mani`;
}

function formatHandsRange(minValue, maxValue) {
    const min = formatHands(minValue);
    const max = formatHands(maxValue);
    if (min === '-' && max === '-') return '-';
    return `${min} - ${max}`;
}

function isSecurityFilterEnabled() {
    const enabled =
        telemetryData.value?.SecurityFilterEnabled ?? telemetryData.value?.securityFilterEnabled;
    return enabled === true;
}

function getSecurityFilterStatus(row) {
    if (!isSecurityFilterEnabled()) return 'Disattivato';
    return row?.SecurityFilterActive ? 'Pausa bot' : 'In valutazione';
}

function getSecurityFilterStatusClass(row) {
    if (!isSecurityFilterEnabled()) return 'bg-surface-100 text-muted-color dark:bg-surface-800';
    if (row?.SecurityFilterActive) return 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-300';
    return 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-300';
}

function getSecurityFilterRowClass(row) {
    if (!isSecurityFilterEnabled()) return 'bg-surface-50/50 dark:bg-surface-900/30';
    if (row?.SecurityFilterActive) return 'bg-red-50 dark:bg-red-950/30';
    return 'bg-emerald-50/60 dark:bg-emerald-950/20';
}

function getScoreClass(row) {
    const score = Number(row?.SecurityRiskScore ?? 0);
    if (score >= 3) return 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-300';
    if (score >= 2) return 'bg-orange-100 text-orange-700 dark:bg-orange-900/30 dark:text-orange-300';
    if (score >= 1) return 'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-300';
    return 'bg-surface-100 text-muted-color dark:bg-surface-800';
}

function getPaceClass(value) {
    const seconds = getNumber(value);
    const veryFast = getNumber(securityFilterSetup.value.veryFastSeconds);
    const fast = getNumber(securityFilterSetup.value.maxAvgSeconds);

    if (seconds > 0 && seconds <= veryFast) return 'font-semibold text-red-500';
    if (seconds > 0 && seconds <= fast) return 'font-semibold text-orange-500';
    return 'font-semibold text-emerald-600 dark:text-emerald-400';
}

function getLastTwoDeltas(row) {
    return Array.isArray(row?.LastTwoHandDeltaSeconds) ? row.LastTwoHandDeltaSeconds : [];
}

function getRapidDeltaCount(row) {
    const veryFast = getNumber(securityFilterSetup.value.veryFastSeconds);
    return getLastTwoDeltas(row).filter((value) => getNumber(value) > 0 && getNumber(value) < veryFast).length;
}

function isRapidTriggerActive(row) {
    return row?.RapidL5TriggerActive || getRapidDeltaCount(row) >= 2;
}

function getRapidTriggerClass(row) {
    if (isRapidTriggerActive(row)) return 'font-semibold text-red-500 animate-pulse';
    if (getRapidDeltaCount(row) === 1) return 'font-semibold text-orange-500';
    return 'text-muted-color';
}

function formatLastTwoDeltas(row) {
    const deltas = getLastTwoDeltas(row);
    if (!deltas.length) return '-';
    return deltas.map((value) => formatSeconds(value)).join(' · ');
}

function getSummaryPill(row) {
    if (!isSecurityFilterEnabled()) return 'FILTRO OFF';
    if (row?.PauseBot || row?.SecurityFilterActive) return 'PAUSA ATTIVA';
    if (isRapidTriggerActive(row)) return 'COMPRESSIONE L5';
    if (isPlayerStreakRisk(row)) return 'RISCHIO PLAYER';
    return 'NORMALE';
}

function getSummaryPillClass(row) {
    const label = getSummaryPill(row);
    if (label === 'PAUSA ATTIVA' || label === 'COMPRESSIONE L5') return 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-300 animate-pulse';
    if (label === 'RISCHIO PLAYER') return 'bg-orange-100 text-orange-700 dark:bg-orange-900/30 dark:text-orange-300';
    return 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-300';
}

function shouldBlinkScore(row) {
    return Number(row?.SecurityRiskScore ?? 0) >= 3 || row?.PauseBot || row?.SecurityFilterActive || Number(row?.PreventedL6 ?? 0) > 0;
}

function getScoreDotClass(row, point) {
    const score = Number(row?.SecurityRiskScore ?? 0);
    const filled = score >= point;
    if (!filled) return 'bg-surface-200 dark:bg-surface-700';
    if (score >= 3) return 'bg-red-500';
    if (score === 2) return 'bg-orange-500';
    return 'bg-emerald-500';
}

function getRiskRank(row) {
    if (!isSecurityFilterEnabled()) return 0;
    const score = Number(row?.SecurityRiskScore ?? 0);
    if (row?.PauseBot || row?.SecurityFilterActive || isRapidTriggerActive(row) || score >= 3) return 2;
    if (isPlayerStreakRisk(row)) return 1;
    return 0;
}

function getRiskLabel(row) {
    if (!isSecurityFilterEnabled()) return 'DISATTIVATO';
    if (row?.PauseBot || row?.SecurityFilterActive) return 'PAUSA ATTIVA';
    if (isRapidTriggerActive(row) || Number(row?.SecurityRiskScore ?? 0) >= 3) return 'RISCHIO';
    if (isPlayerStreakRisk(row)) return 'RISCHIO PLAYER';
    return 'NORMALE';
}

function getRiskStripReason(row) {
    if (row?.PauseBot || row?.SecurityFilterActive) return 'pausa';
    if (isRapidTriggerActive(row)) return 'trigger L5';
    if (Number(row?.SecurityRiskScore ?? 0) >= 3) return `score ${row.SecurityRiskScore}/4`;
    if (isPlayerStreakRisk(row)) {
        const metrics = getPlayerStreakMetrics(row);
        return `PLAYER P1→P5 ${metrics.total.toFixed(0)}s ≤ ${metrics.threshold.toFixed(0)}s`;
    }
    return 'normale';
}

function getRiskDotClass(row) {
    const rank = getRiskRank(row);
    if (rank === 2) return 'bg-red-500';
    if (rank === 1) return 'bg-orange-500';
    return 'bg-emerald-500';
}

function getRiskCardClass(row) {
    const rank = getRiskRank(row);
    const selected = selectedSecurityFilterRow.value && getBotName(selectedSecurityFilterRow.value) === getBotName(row);
    if (rank === 2) return selected ? 'border-red-300 bg-red-50 dark:border-red-800 dark:bg-red-950/30' : 'border-red-200 bg-red-50/70 dark:border-red-900/70 dark:bg-red-950/20';
    if (rank === 1) return selected ? 'border-orange-300 bg-orange-50 dark:border-orange-800 dark:bg-orange-950/30' : 'border-orange-200 bg-orange-50/70 dark:border-orange-900/70 dark:bg-orange-950/20';
    return selected ? 'border-emerald-300 bg-emerald-50 dark:border-emerald-800 dark:bg-emerald-950/30' : 'border-emerald-200 bg-emerald-50/60 dark:border-emerald-900/60 dark:bg-emerald-950/15';
}

function getRiskStatusClass(row) {
    const rank = getRiskRank(row);
    if (rank === 2) return 'border-red-200 bg-red-100 text-red-700 dark:border-red-800 dark:bg-red-900/30 dark:text-red-300';
    if (rank === 1) return 'border-orange-200 bg-orange-100 text-orange-700 dark:border-orange-800 dark:bg-orange-900/30 dark:text-orange-300';
    return 'border-emerald-200 bg-emerald-100 text-emerald-700 dark:border-emerald-800 dark:bg-emerald-900/30 dark:text-emerald-300';
}

function getScoreBlockClass(row, point) {
    const score = Number(row?.SecurityRiskScore ?? 0);
    if (score < point) return 'bg-surface-200 dark:bg-surface-700';
    if (score >= 3) return 'bg-red-500';
    if (score === 2) return 'bg-orange-500';
    return 'bg-emerald-500';
}

function getTriggerLabel(row) {
    if (row?.PauseBot || row?.SecurityFilterActive) return 'PAUSA';
    if (isRapidTriggerActive(row)) return 'TRIGGER ON';
    if (isPlayerStreakRisk(row)) {
        const metrics = getPlayerStreakMetrics(row);
        return `P1→P5 ${metrics.total.toFixed(0)}s`;
    }
    return 'OFF';
}

function getTriggerClass(row) {
    const rank = getRiskRank(row);
    if (rank === 2) return 'text-red-600 dark:text-red-300';
    if (rank === 1) return 'text-orange-600 dark:text-orange-300';
    return 'text-muted-color';
}

function shouldPulseCard(row) {
    return isRapidTriggerActive(row) || row?.PauseBot || row?.SecurityFilterActive || Number(row?.SecurityRiskScore ?? 0) >= 3 || Number(row?.PreventedL6 ?? 0) > 0;
}

function selectSecurityFilterBot(row) {
    const botName = getBotName(row);
    if (selectedSecurityFilterBot.value === botName) {
        selectedSecurityFilterBot.value = null;
        securityFilterDetailUnavailable.value = false;
        return;
    }

    selectedSecurityFilterBot.value = botName;
    securityFilterDetailUnavailable.value = false;

    const summary = securityFilterRows.value.find((row) => getBotName(row) === botName);
    if (summary && isPlayerStreakRisk(summary)) {
        const metrics = getPlayerStreakMetrics(summary);
        console.debug('[PLAYER_STREAK_RISK]', botName, {
            playerStreak: metrics.count,
            p1_p5_total_sec: metrics.total,
            mean_interval_sec: metrics.mean,
            intervals_sec: metrics.intervals,
            threshold_sec: metrics.threshold,
            outcome: metrics.outcome
        });
    }

    if (securityFilterBotDetails.value[botName]) return;

    securityFilterDetailLoading.value = true;
    DashboardService.getSecurityFilterDetail(botName)
        .then((detail) => {
            if (detail) {
                securityFilterBotDetails.value = {
                    ...securityFilterBotDetails.value,
                    [botName]: detail
                };
            } else {
                securityFilterDetailUnavailable.value = true;
            }
        })
        .catch(() => {
            securityFilterDetailUnavailable.value = true;
        })
        .finally(() => {
            securityFilterDetailLoading.value = false;
        });
}
</script>

<template>
    <!-- ================= GLOBAL STATISTICS ================= -->
    <div class="col-span-12">
        <h3 class="text-xl font-semibold mb-4">Global Statistics</h3>
    </div>

    <div class="col-span-12 lg:col-span-6 xl:col-span-3">
        <div class="card">
            <span class="block text-muted-color font-medium mb-4">Total L6 Authorized</span>
            <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">
                {{ telemetryData?.TotalAuthL6Authorized }}
            </div>
        </div>
    </div>

    <div class="col-span-12 lg:col-span-6 xl:col-span-3">
        <div class="card">
            <span class="block text-muted-color font-medium mb-4">Total L5 Played</span>
            <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">
                {{ telemetryData?.TotalL5Played }}
            </div>
        </div>
    </div>

    <div class="col-span-12 lg:col-span-6 xl:col-span-3">
        <div class="card">
            <span class="block text-muted-color font-medium mb-4">Total L5 Won</span>
            <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">
                {{ telemetryData?.TotalL5Won }}
            </div>
        </div>
    </div>

    <div class="col-span-12 lg:col-span-6 xl:col-span-3">
        <div class="card">
            <span class="block text-muted-color font-medium mb-4">Total L5 Lost</span>
            <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">
                {{ telemetryData?.TotalL5Lost }}
            </div>
        </div>
    </div>

    <div class="col-span-12 lg:col-span-6 xl:col-span-3">
        <div class="card">
            <span class="block text-muted-color font-medium mb-4">Total PB Hands Played</span>
            <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">
                {{ telemetryData?.TotalPBHandsPlayed }}
            </div>
        </div>
    </div>

    <div class="col-span-12 lg:col-span-6 xl:col-span-3">
        <div class="card">
            <span class="block text-muted-color font-medium mb-4">Total L8 Played</span>
            <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">
                {{ telemetryData?.TotalL8Played }}
            </div>
        </div>
    </div>

    <div class="col-span-12 lg:col-span-6 xl:col-span-3">
        <div class="card">
            <span class="block text-muted-color font-medium mb-4">Total L8 Won</span>
            <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">
                {{ telemetryData?.TotalL8Won }}
            </div>
        </div>
    </div>

    <div class="col-span-12 lg:col-span-6 xl:col-span-3">
        <div class="card">
            <span class="block text-muted-color font-medium mb-4">Total L8 Lost</span>
            <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">
                {{ telemetryData?.TotalL8Lost }}
            </div>
        </div>
    </div>

    <!-- ================= SPOT METRICS ================= -->
    <div class="col-span-12 mt-6">
        <h3 class="text-xl font-semibold mb-4">Spot Metrics</h3>
    </div>

    <div class="col-span-12 lg:col-span-6 xl:col-span-3">
        <div class="card">
            <span class="block text-muted-color font-medium mb-4">Spot ID</span>
            <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">
                {{ telemetryData?.SpotID }}
            </div>
        </div>
    </div>

    <div class="col-span-12 lg:col-span-6 xl:col-span-3">
        <div class="card">
            <span class="block text-muted-color font-medium mb-4">Spot PB Hands Played</span>
            <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">
                {{ telemetryData?.SpotPBHandsPlayed }}
            </div>
        </div>
    </div>

    <div class="col-span-12 lg:col-span-6 xl:col-span-3">
        <div class="card">
            <span class="block text-muted-color font-medium mb-4">Spot L6 Authorized</span>
            <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">
                {{ telemetryData?.SpotAuthL6Counter }}
            </div>
        </div>
    </div>

    <div class="col-span-12 lg:col-span-6 xl:col-span-3">
        <div class="card">
            <span class="block text-muted-color font-medium mb-4">Spot L5 Loss</span>
            <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">
                {{ telemetryData?.SpotL5Loss }}
            </div>
        </div>
    </div>

    <!-- ================= INDICATORS & PAUSE ================= -->
    <div class="col-span-12 mt-6">
        <h3 class="text-xl font-semibold mb-4">Indicators & Pause Logic</h3>
    </div>

    <div class="col-span-12 lg:col-span-6 xl:col-span-3">
        <div class="card">
            <span class="block text-muted-color font-medium mb-2">Global Pause Scalping</span>
            <div class="text-xl font-medium">
                {{ telemetryData?.GlobalPauseScalping ? 'Attiva' : 'Non attiva' }}
            </div>
            <div class="text-sm text-muted-color mt-2">
                {{ telemetryData?.GlobalPauseScalpingDetails }}
            </div>
            <div class="text-sm text-muted-color">Durata: {{ telemetryData?.GlobalPauseScalpingDuration }}</div>
            <div class="text-sm text-muted-color">Pause Soglie: {{ telemetryData?.TotalPauseScalpingSoglieActivated }}</div>
            <div class="text-sm text-muted-color">Pause EWMA: {{ telemetryData?.TotalPauseScalpingEWMAActivated }}</div>
        </div>
    </div>

    <div class="col-span-12 lg:col-span-6 xl:col-span-3">
        <div class="card">
            <span class="block text-muted-color font-medium mb-4">INC</span>
            <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">
                {{ telemetryData?.INC }}
            </div>
        </div>
    </div>

    <div class="col-span-12 lg:col-span-6 xl:col-span-3">
        <div class="card">
            <span class="block text-muted-color font-medium mb-4">EWMA</span>
            <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">
                {{ telemetryData?.EWMA }}
            </div>
        </div>
    </div>

    <!-- ================= SECURITY FILTER ================= -->
    <div class="col-span-12 mt-6">
        <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-2 mb-4">
            <h3 class="text-xl font-semibold m-0">Security Filter (sperimentale)</h3>
            <span class="text-sm" :class="securityFilterOperational ? 'text-green-500' : 'text-red-500'">
                {{ securityFilterOperational ? 'Attivo da Config' : 'Disattivato da Config' }}
            </span>
        </div>
    </div>

    <div class="col-span-12 lg:col-span-6 xl:col-span-3">
        <div class="card">
            <span class="block text-muted-color font-medium mb-4">Security Filter Attivazioni</span>
            <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">
                {{ telemetryData?.TotalSecurityFilterActivated ?? 0 }}
            </div>
            <div class="text-sm text-muted-color mt-2">Avg hand: {{ telemetryData?.LastAvgHandSeconds != null ? Number(telemetryData.LastAvgHandSeconds).toFixed(1) + 's' : '-' }}</div>
            <div class="text-sm text-muted-color">Bot attivi: {{ telemetryData?.ActiveSecurityFilterBots ?? 0 }}</div>
        </div>
    </div>

    <div class="col-span-12 lg:col-span-6 xl:col-span-3">
        <div class="card">
            <span class="block text-muted-color font-medium mb-4">Security Filter L6 Prevenuti</span>
            <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">
                {{ telemetryData?.TotalSecurityFilterPreventedL6 ?? 0 }}
            </div>
            <div class="text-sm text-muted-color mt-2">Filtro a L5 con credito disponibile</div>
        </div>
    </div>

    <div class="col-span-12">
        <div class="card">
            <div class="flex flex-col gap-1 mb-4">
                <span class="block text-muted-color font-medium">Setup Security Filter</span>
                <span class="text-sm text-muted-color">Condizioni correnti usate per comporre lo score per singolo bot.</span>
            </div>
            <div class="grid grid-cols-12 gap-3 text-sm mb-4">
                <div class="col-span-12 md:col-span-6 xl:col-span-4 rounded-xl bg-surface-100 p-3 dark:bg-surface-900">
                    <div class="text-muted-color mb-1">Security Filter (runtime Decisore)</div>
                    <div class="font-semibold" :class="securityFilterSetup.enabled ? 'text-green-600 dark:text-green-400' : 'text-red-500'">
                        {{ securityFilterSetup.enabled ? 'ON' : 'OFF' }}
                    </div>
                </div>
                <div class="col-span-6 md:col-span-3 xl:col-span-2 rounded-xl bg-surface-50 p-3 dark:bg-surface-800">
                    <div class="text-muted-color mb-1">Max Avg Seconds</div>
                    <div class="font-semibold">{{ Number(securityFilterSetup.maxAvgSeconds).toFixed(2) }}s</div>
                </div>
                <div class="col-span-6 md:col-span-3 xl:col-span-2 rounded-xl bg-surface-50 p-3 dark:bg-surface-800">
                    <div class="text-muted-color mb-1">Very Fast Seconds</div>
                    <div class="font-semibold">{{ Number(securityFilterSetup.veryFastSeconds).toFixed(2) }}s</div>
                </div>
                <div class="col-span-6 md:col-span-3 xl:col-span-2 rounded-xl bg-surface-50 p-3 dark:bg-surface-800">
                    <div class="text-muted-color mb-1">Min Score</div>
                    <div class="font-semibold">{{ securityFilterSetup.minScore }}</div>
                </div>
                <div class="col-span-6 md:col-span-3 xl:col-span-2 rounded-xl bg-surface-50 p-3 dark:bg-surface-800">
                    <div class="text-muted-color mb-1">Delta Window</div>
                    <div class="font-semibold">{{ securityFilterSetup.deltaWindow }}</div>
                </div>
                <div class="col-span-6 md:col-span-3 xl:col-span-2 rounded-xl bg-surface-50 p-3 dark:bg-surface-800">
                    <div class="text-muted-color mb-1">Max Shoe Hand</div>
                    <div class="font-semibold">{{ securityFilterSetup.maxShoeHand }}</div>
                </div>
                <div class="col-span-6 md:col-span-3 xl:col-span-2 rounded-xl bg-surface-50 p-3 dark:bg-surface-800">
                    <div class="text-muted-color mb-1">Min Streak</div>
                    <div class="font-semibold">{{ securityFilterSetup.minStreak }}</div>
                </div>
                <div class="col-span-6 md:col-span-3 xl:col-span-2 rounded-xl bg-surface-50 p-3 dark:bg-surface-800">
                    <div class="text-muted-color mb-1">PLAYER P1→P5 soglia UI</div>
                    <div class="font-semibold">{{ Number(securityFilterSetup.playerP1P5Threshold).toFixed(0) }}s</div>
                </div>
                <div class="col-span-6 md:col-span-3 xl:col-span-2 rounded-xl bg-surface-50 p-3 dark:bg-surface-800">
                    <div class="text-muted-color mb-1">Prevented L6 (tot.)</div>
                    <div class="font-semibold">{{ securityFilterSetup.preventedL6 }}</div>
                </div>
                <div class="col-span-12 md:col-span-6 rounded-xl border border-surface-200 p-3 dark:border-surface-700" v-if="securityFilterLastEvent">
                    <div class="text-muted-color mb-1">Ultimo evento filtro (per bot)</div>
                    <div class="font-semibold">{{ securityFilterLastEvent.bot }} — {{ securityFilterLastEvent.reason }}</div>
                    <div class="text-xs text-muted-color mt-1" v-if="securityFilterLastEvent.ts">{{ securityFilterLastEvent.ts }}</div>
                </div>
            </div>
            <div class="grid grid-cols-12 gap-3 text-sm">
                <div class="col-span-12 md:col-span-6 xl:col-span-4 rounded-xl bg-surface-50 p-3 dark:bg-surface-800">
                    <div class="text-muted-color mb-1">Soglia attivazione</div>
                    <div class="font-semibold">Score minimo {{ securityFilterSetup.minScore }}/4</div>
                </div>
                <div class="col-span-12 md:col-span-6 xl:col-span-4 rounded-xl bg-surface-50 p-3 dark:bg-surface-800">
                    <div class="text-muted-color mb-1">Finestra avg mano</div>
                    <div class="font-semibold">Ultimi {{ securityFilterSetup.deltaWindow }} delta mano</div>
                </div>
                <div class="col-span-12 md:col-span-6 xl:col-span-4 rounded-xl bg-surface-50 p-3 dark:bg-surface-800">
                    <div class="text-muted-color mb-1">Media</div>
                    <div class="font-semibold">Trimmata quando ci sono almeno 3 campioni</div>
                </div>
                <div class="col-span-12 md:col-span-6 xl:col-span-3 rounded-xl border border-surface-200 p-3 dark:border-surface-700">
                    <div class="font-semibold">+1 streak</div>
                    <div class="text-muted-color">se streak &gt;= {{ securityFilterSetup.minStreak }}</div>
                </div>
                <div class="col-span-12 md:col-span-6 xl:col-span-3 rounded-xl border border-surface-200 p-3 dark:border-surface-700">
                    <div class="font-semibold">+1 avg veloce</div>
                    <div class="text-muted-color">se avg mano &lt; {{ Number(securityFilterSetup.maxAvgSeconds).toFixed(1) }}s</div>
                </div>
                <div class="col-span-12 md:col-span-6 xl:col-span-3 rounded-xl border border-surface-200 p-3 dark:border-surface-700">
                    <div class="font-semibold">+1 inizio shoe</div>
                    <div class="text-muted-color">se mano shoe &lt;= {{ securityFilterSetup.maxShoeHand }}</div>
                </div>
                <div class="col-span-12 md:col-span-6 xl:col-span-3 rounded-xl border border-surface-200 p-3 dark:border-surface-700">
                    <div class="font-semibold">+1 very fast</div>
                    <div class="text-muted-color">se avg mano &lt; {{ Number(securityFilterSetup.veryFastSeconds).toFixed(1) }}s</div>
                </div>
            </div>
        </div>
    </div>

    <div class="col-span-12" v-if="securityFilterRows.length">
        <div class="card">
            <div
                v-if="!securityFilterOperational"
                class="mb-4 rounded-xl border border-surface-300 bg-surface-100 p-4 text-center dark:border-surface-600 dark:bg-surface-900"
            >
                <div class="text-lg font-bold text-surface-900 dark:text-surface-0">Security Filter disattivato</div>
                <div class="mt-1 text-sm text-muted-color">
                    Config <code class="text-xs">SECURITY_FILTER_ENABLED=0</code> — nessun AC3, pausa o blocco L6 da filtro. Player pace resta visibile aprendo un bot.
                </div>
            </div>

            <template v-if="securityFilterOperational">
            <div class="mb-4 flex flex-col gap-3 lg:flex-row lg:items-end lg:justify-between">
                <div>
                    <span class="block text-muted-color font-medium">Security Filter Control Room</span>
                    <div class="mt-1 text-sm text-muted-color">Overview compatta, ordinata per rischio operativo. Click su un bot per aprire o chiudere il dettaglio.</div>
                </div>
                <div class="flex flex-wrap gap-2 text-xs font-semibold">
                    <span class="inline-flex items-center gap-2 rounded-full bg-red-100 px-2.5 py-1 text-red-700 dark:bg-red-900/30 dark:text-red-300">
                        <span class="h-2 w-2 rounded-full bg-red-500"></span>
                        {{ securityFilterRows.filter((row) => getRiskRank(row) === 2).length }} rossi
                    </span>
                    <span class="inline-flex items-center gap-2 rounded-full bg-orange-100 px-2.5 py-1 text-orange-700 dark:bg-orange-900/30 dark:text-orange-300">
                        <span class="h-2 w-2 rounded-full bg-orange-500"></span>
                        {{ securityFilterRows.filter((row) => getRiskRank(row) === 1).length }} arancioni
                    </span>
                    <span class="inline-flex items-center gap-2 rounded-full bg-emerald-100 px-2.5 py-1 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-300">
                        <span class="h-2 w-2 rounded-full bg-emerald-500"></span>
                        {{ securityFilterRows.filter((row) => getRiskRank(row) === 0).length }} verdi
                    </span>
                </div>
            </div>

            <div
                class="mb-4 flex flex-col gap-2 rounded-xl border p-3 text-sm md:flex-row md:items-center md:justify-between"
                :class="hasSecurityFilterRisk ? 'border-red-200 bg-red-50 text-red-800 dark:border-red-900/70 dark:bg-red-950/25 dark:text-red-200' : 'border-emerald-200 bg-emerald-50 text-emerald-800 dark:border-emerald-900/70 dark:bg-emerald-950/20 dark:text-emerald-200'"
            >
                <span class="font-semibold">{{ hasSecurityFilterRisk ? 'RISCHIO ATTIVO' : 'NESSUN RISCHIO ATTIVO' }}: {{ securityFilterRiskStrip }}</span>
                <span class="text-xs text-muted-color">Rossi prima, poi arancioni, poi verdi</span>
            </div>

            <div class="grid grid-cols-1 gap-3 md:grid-cols-2 xl:grid-cols-4">
                <button
                    v-for="row in securityFilterRows"
                    :key="getBotName(row)"
                    type="button"
                    class="min-h-40 rounded-xl border p-4 text-left transition-colors hover:border-primary-300 focus:outline-none focus:ring-2 focus:ring-primary-300 dark:hover:border-primary-700"
                    :class="[getRiskCardClass(row), shouldPulseCard(row) ? 'animate-pulse' : '']"
                    @click="selectSecurityFilterBot(row)"
                >
                    <div class="mb-3 flex items-center justify-between gap-3">
                        <span class="inline-flex items-center gap-2 text-lg font-bold text-surface-900 dark:text-surface-0">
                            <span class="h-2.5 w-2.5 rounded-full" :class="getRiskDotClass(row)"></span>
                            {{ getBotName(row) }}
                        </span>
                        <span class="font-bold text-muted-color">L{{ row.Martingala ?? '-' }}</span>
                    </div>

                    <div class="mb-3 grid grid-cols-2 gap-3">
                        <div>
                            <div class="text-xs uppercase tracking-wide text-muted-color">Avg</div>
                            <div class="font-bold" :class="getPaceClass(row.AvgHandSeconds)">{{ formatSeconds(row.AvgHandSeconds) }}</div>
                        </div>
                        <div>
                            <div class="text-xs uppercase tracking-wide text-muted-color">Shoe processato</div>
                            <div class="font-bold">{{ row.LastShoeHand ?? '-' }}</div>
                        </div>
                    </div>

                    <div class="mb-3 flex items-center justify-between gap-3">
                        <div class="flex gap-1">
                            <span v-for="point in 4" :key="point" class="inline-block h-2.5 w-6 rounded-sm" :class="getScoreBlockClass(row, point)"></span>
                        </div>
                        <span class="text-sm font-semibold text-muted-color">{{ row.SecurityRiskScore ?? 0 }}/4</span>
                    </div>

                    <div v-if="getPlayerPaceVisual(row).active" class="mb-3">
                        <div class="mb-1 text-[10px] font-semibold uppercase tracking-wide text-blue-600 dark:text-blue-300">Streak PLAYER</div>
                        <div class="flex items-center gap-1">
                            <span
                                v-for="step in getPlayerPaceVisual(row).steps"
                                :key="`${getBotName(row)}-${step.label}`"
                                class="inline-flex h-7 w-7 items-center justify-center rounded-md text-[10px] font-bold transition-transform"
                                :class="getPlayerStepClassCompact(step, row)"
                            >
                                {{ step.label.replace('P', '') }}
                            </span>
                        </div>
                        <div
                            v-if="getPlayerPaceVisual(row).count >= securityFilterSetup.minStreak && getPlayerPaceVisual(row).total > 0"
                            class="mt-1 text-sm font-bold"
                            :class="isPlayerStreakRisk(row) ? 'text-orange-600 dark:text-orange-300' : 'text-blue-700 dark:text-blue-300'"
                        >
                            P1→P5 {{ formatPlayerPaceSeconds(getPlayerPaceVisual(row).total) }}
                        </div>
                        <div v-else-if="getPlayerPaceVisual(row).count > 0" class="mt-1 text-xs font-semibold text-blue-700 dark:text-blue-300">
                            {{ getPlayerPaceVisual(row).count }}/{{ securityFilterSetup.minStreak }} PLAYER
                        </div>
                    </div>

                    <div class="flex items-center justify-between gap-3">
                        <span class="text-xs font-bold" :class="getTriggerClass(row)">{{ getTriggerLabel(row) }}</span>
                        <span class="rounded-full border px-2.5 py-1 text-xs font-bold" :class="getRiskStatusClass(row)">{{ getRiskLabel(row) }}</span>
                    </div>
                </button>
            </div>
            </template>

            <div v-else class="grid grid-cols-1 gap-3 md:grid-cols-2 xl:grid-cols-4">
                <button
                    v-for="row in securityFilterRows"
                    :key="`off-${getBotName(row)}`"
                    type="button"
                    class="min-h-28 rounded-xl border border-surface-200 bg-surface-50/80 p-4 text-left dark:border-surface-700 dark:bg-surface-900/40"
                    @click="selectSecurityFilterBot(row)"
                >
                    <div class="mb-2 flex items-center justify-between">
                        <span class="text-lg font-bold">{{ getBotName(row) }}</span>
                        <span class="rounded-full bg-surface-200 px-2 py-0.5 text-xs font-semibold text-muted-color dark:bg-surface-700">FILTRO OFF</span>
                    </div>
                    <div v-if="getPlayerPaceVisual(row).active" class="flex items-center gap-1">
                        <span
                            v-for="step in getPlayerPaceVisual(row).steps"
                            :key="`off-${getBotName(row)}-${step.label}`"
                            class="inline-flex h-7 w-7 items-center justify-center rounded-md text-[10px] font-bold"
                            :class="getPlayerStepClassCompact(step, row)"
                        >
                            {{ step.label.replace('P', '') }}
                        </span>
                    </div>
                    <div v-else class="text-xs text-muted-color">Apri per sequenza PLAYER</div>
                </button>
            </div>

            <div v-if="selectedSecurityFilterRow" class="mt-5">
                <div>
                    <div class="mb-3 flex flex-col gap-2 md:flex-row md:items-center md:justify-between">
                        <div>
                            <h4 class="m-0 text-xl font-semibold">Dettaglio aperto: {{ getBotName(selectedSecurityFilterRow) }}</h4>
                            <div class="text-sm text-muted-color">Un solo bot espanso alla volta. Il resto rimane in overview compatta.</div>
                            <div v-if="securityFilterDetailLoading" class="mt-1 text-xs text-muted-color">Caricamento dettaglio completo…</div>
                            <div v-else-if="securityFilterDetailUnavailable" class="mt-1 text-xs text-orange-600 dark:text-orange-300">Dettaglio esteso non disponibile — mostrata solo la sintesi live.</div>
                        </div>
                        <div class="flex flex-wrap items-center gap-2">
                            <span class="w-fit rounded-full border px-3 py-1 text-xs font-bold" :class="getRiskStatusClass(selectedSecurityFilterRow)">{{ getRiskLabel(selectedSecurityFilterRow) }}</span>
                            <button type="button" class="rounded-full border border-surface-200 px-3 py-1 text-xs font-semibold text-muted-color hover:bg-surface-100 dark:border-surface-700 dark:hover:bg-surface-800" @click="selectedSecurityFilterBot = null">
                                Chiudi dettaglio
                            </button>
                        </div>
                    </div>

                    <div class="mb-4 rounded-xl border p-4" :class="getPlayerPacePanelClass(selectedSecurityFilterRow)">
                        <div class="mb-3 text-lg font-bold text-surface-900 dark:text-surface-0">Sequenza PLAYER</div>

                        <div v-if="!getPlayerPaceVisual(selectedSecurityFilterRow).available" class="text-base font-semibold text-muted-color">
                            PLAYER pace: dati non disponibili
                        </div>

                        <div v-else-if="!getPlayerPaceVisual(selectedSecurityFilterRow).active" class="text-base font-semibold text-muted-color">
                            {{ getPlayerPaceVisual(selectedSecurityFilterRow).statusLabel }}
                        </div>

                        <template v-else>
                            <div class="overflow-x-auto pb-1">
                                <div class="flex min-w-max items-center justify-center gap-1 px-1 sm:gap-2">
                                    <template v-for="(step, stepIdx) in getPlayerPaceVisual(selectedSecurityFilterRow).steps" :key="step.label">
                                        <div
                                            v-if="stepIdx > 0 && getPlayerPaceVisual(selectedSecurityFilterRow).deltas[stepIdx - 1].visible"
                                            class="flex min-w-[5.5rem] flex-col items-center px-1 text-center"
                                        >
                                            <span class="text-xs font-semibold text-muted-color">
                                                {{ getPlayerPaceVisual(selectedSecurityFilterRow).deltas[stepIdx - 1].label }}
                                            </span>
                                            <span
                                                class="text-sm font-bold leading-tight sm:text-lg"
                                                :class="getPlayerPaceVisual(selectedSecurityFilterRow).deltas[stepIdx - 1].pending ? 'text-blue-700 dark:text-blue-300' : 'text-surface-900 dark:text-surface-0'"
                                            >
                                                {{
                                                    getPlayerPaceVisual(selectedSecurityFilterRow).deltas[stepIdx - 1].pending
                                                        ? 'delta in attesa'
                                                        : formatPlayerPaceSeconds(getPlayerPaceVisual(selectedSecurityFilterRow).deltas[stepIdx - 1].seconds)
                                                }}
                                            </span>
                                        </div>
                                        <div class="flex flex-col items-center">
                                            <div
                                                class="flex h-16 w-16 items-center justify-center rounded-xl border-2 text-lg font-bold transition-transform sm:h-[4.5rem] sm:w-[4.5rem] sm:text-xl"
                                                :class="getPlayerStepClass(step, selectedSecurityFilterRow)"
                                            >
                                                {{ step.label }}
                                            </div>
                                        </div>
                                    </template>
                                </div>
                            </div>

                            <div class="mt-4 space-y-1 text-center">
                                <div
                                    v-if="getPlayerPaceVisual(selectedSecurityFilterRow).count >= securityFilterSetup.minStreak && getPlayerPaceVisual(selectedSecurityFilterRow).total > 0"
                                    class="text-2xl font-bold text-surface-900 dark:text-surface-0 sm:text-3xl"
                                >
                                    PLAYER P1→P5:
                                    {{ formatPlayerPaceSeconds(getPlayerPaceVisual(selectedSecurityFilterRow).total) }}
                                    / soglia {{ getPlayerPaceVisual(selectedSecurityFilterRow).threshold.toFixed(0) }}s
                                </div>
                                <div
                                    v-else
                                    class="text-xl font-bold text-blue-700 dark:text-blue-300"
                                >
                                    Streak PLAYER {{ getPlayerPaceVisual(selectedSecurityFilterRow).count }}/{{ securityFilterSetup.minStreak }}
                                </div>
                                <div
                                    v-if="getPlayerPaceVisual(selectedSecurityFilterRow).mean > 0"
                                    class="text-lg font-semibold text-surface-800 dark:text-surface-100"
                                >
                                    Media: {{ formatPlayerPaceSeconds(getPlayerPaceVisual(selectedSecurityFilterRow).mean) }}
                                </div>
                                <div class="text-xl font-bold" :class="getPlayerPaceStatusClass(selectedSecurityFilterRow)">
                                    Stato: {{ getPlayerPaceVisual(selectedSecurityFilterRow).statusLabel }}
                                </div>
                            </div>
                        </template>
                    </div>

                    <div v-if="securityFilterOperational" class="grid grid-cols-1 gap-3 md:grid-cols-2">
                        <div class="rounded-xl bg-surface-0 p-3 text-sm dark:bg-surface-900">
                            <div class="mb-2 font-semibold">Ritmo completo</div>
                            <div class="flex flex-col gap-1 leading-tight">
                                <div>
                                    <span class="font-semibold">Avg ritmo</span>
                                    <span :class="getPaceClass(selectedSecurityFilterRow.AvgHandSeconds)">{{ formatSeconds(selectedSecurityFilterRow.AvgHandSeconds) }}</span>
                                    / {{ Number(securityFilterSetup.maxAvgSeconds).toFixed(1) }}s
                                    <span class="ml-1 rounded-full px-2 py-0.5 text-xs font-semibold" :class="getRiskPillClass(isAvgFast(selectedSecurityFilterRow))">{{ getScorePoint(isAvgFast(selectedSecurityFilterRow)) }}</span>
                                </div>
                                <div>
                                    <span class="font-semibold">Very fast</span>
                                    <span :class="getPaceClass(selectedSecurityFilterRow.AvgHandSeconds)">{{ formatSeconds(selectedSecurityFilterRow.AvgHandSeconds) }}</span>
                                    / {{ Number(securityFilterSetup.veryFastSeconds).toFixed(1) }}s
                                    <span class="ml-1 rounded-full px-2 py-0.5 text-xs font-semibold" :class="getRiskPillClass(isVeryFast(selectedSecurityFilterRow))">{{ getScorePoint(isVeryFast(selectedSecurityFilterRow)) }}</span>
                                </div>
                                <div class="text-xs text-muted-color">Ult <span :class="getPaceClass(selectedSecurityFilterRow.LastHandDeltaSeconds)">{{ formatSeconds(selectedSecurityFilterRow.LastHandDeltaSeconds) }}</span> / {{ Number(securityFilterSetup.maxAvgSeconds).toFixed(1) }}s</div>
                                <div class="text-xs text-muted-color">Range storico bot {{ formatSeconds(selectedSecurityFilterRow.MinHandDeltaSeconds) }} - {{ formatSeconds(selectedSecurityFilterRow.MaxHandDeltaSeconds) }}</div>
                                <div class="text-xs text-muted-color">Non usato direttamente nella media attuale</div>
                                <div class="text-xs text-muted-color">Mani bot {{ formatHands(selectedSecurityFilterRow.PBHandsPlayed) }}</div>
                            </div>
                        </div>

                        <div class="rounded-xl bg-surface-0 p-3 text-sm dark:bg-surface-900">
                            <div class="mb-2 font-semibold">Ultimi delta / Trigger rapido</div>
                            <div class="flex flex-col gap-1 leading-tight">
                                <div class="text-xs" :class="getRapidTriggerClass(selectedSecurityFilterRow)">Ultimi 2: {{ formatLastTwoDeltas(selectedSecurityFilterRow) }}</div>
                                <div class="text-xs" :class="getRapidTriggerClass(selectedSecurityFilterRow)">Trigger rapido L5: ultimi 2 &lt; {{ Number(securityFilterSetup.veryFastSeconds).toFixed(1) }}s · {{ isRapidTriggerActive(selectedSecurityFilterRow) ? 'Attivo' : 'Non attivo' }}</div>
                                <div class="mt-1 flex items-center gap-1 text-xs text-muted-color" :class="shouldBlinkScore(selectedSecurityFilterRow) ? 'animate-pulse' : ''">
                                    <span>Score filtro:</span>
                                    <span v-for="point in 4" :key="point" class="inline-block h-2.5 w-6 rounded-sm" :class="getScoreBlockClass(selectedSecurityFilterRow, point)"></span>
                                    <span class="ml-1">{{ selectedSecurityFilterRow.SecurityRiskScore ?? 0 }}/4</span>
                                </div>
                            </div>
                        </div>

                        <div class="rounded-xl bg-surface-0 p-3 text-sm dark:bg-surface-900">
                            <div class="mb-2 font-semibold">Streak e shoe vs soglia</div>
                            <div class="grid grid-cols-2 gap-3">
                                <div>
                                    <div class="text-xs text-muted-color">Streak</div>
                                    <div class="font-semibold">{{ selectedSecurityFilterRow.CurrentStreak ?? 0 }} / {{ securityFilterSetup.minStreak }}</div>
                                    <span class="mt-1 inline-flex rounded-full px-2 py-0.5 text-xs font-semibold" :class="getRiskPillClass(isStreakRisk(selectedSecurityFilterRow))">{{ getScorePoint(isStreakRisk(selectedSecurityFilterRow)) }}</span>
                                </div>
                                <div>
                                    <div class="text-xs text-muted-color">Mano processata</div>
                                    <div class="font-semibold">{{ selectedSecurityFilterRow.LastShoeHand ?? '-' }} / {{ securityFilterSetup.maxShoeHand }}</div>
                                    <span class="mt-1 inline-flex rounded-full px-2 py-0.5 text-xs font-semibold" :class="getRiskPillClass(isShoeRisk(selectedSecurityFilterRow))">{{ getScorePoint(isShoeRisk(selectedSecurityFilterRow)) }}</span>
                                </div>
                            </div>
                            <div class="mt-3 text-xs text-muted-color">Telemetry = ultima mano processata dal Decisore, non stato live bot.</div>
                        </div>

                        <div class="rounded-xl bg-surface-0 p-3 text-sm dark:bg-surface-900">
                            <div class="mb-2 font-semibold">Pausa / Scope</div>
                            <div class="grid grid-cols-1 gap-3 lg:grid-cols-2">
                                <div class="flex flex-col gap-1 leading-tight">
                                    <span :class="selectedSecurityFilterRow.PauseBot ? 'font-semibold text-red-500' : 'text-muted-color'">
                                        {{ selectedSecurityFilterRow.PauseBot ? `Solo ${selectedSecurityFilterRow.PauseComputer || getBotName(selectedSecurityFilterRow)}` : 'Nessuna' }}
                                    </span>
                                    <span class="text-xs text-muted-color">L6 prevenuti: {{ selectedSecurityFilterRow.PreventedL6 ?? 0 }}</span>
                                </div>
                                <div class="flex flex-col gap-1 leading-tight">
                                    <span><strong>Sintesi filtro</strong> {{ selectedSecurityFilterRow.SecurityRiskScore ?? 0 }}/4</span>
                                    <span class="text-xs text-muted-color">Pausa da {{ securityFilterSetup.minScore }}/4</span>
                                    <span class="text-xs text-muted-color">Stato {{ getSecurityFilterStatus(selectedSecurityFilterRow) }}</span>
                                    <span class="text-xs text-muted-color" v-if="selectedSecurityFilterRow.LastReason || selectedSecurityFilterRow.lastReason">
                                        Ultimo motivo: {{ selectedSecurityFilterRow.LastReason || selectedSecurityFilterRow.lastReason }}
                                    </span>
                                    <span class="text-xs text-muted-color">Scope corrente {{ selectedSecurityFilterRow.PauseScope === 'BOT' ? 'Singolo bot' : 'Nessuna' }} · L{{ selectedSecurityFilterRow.Martingala ?? '-' }}</span>
                                    <span class="text-xs text-muted-color">Stato corrente bot, non storico L6/L8</span>
                                </div>
                            </div>
                        </div>

                        <div class="rounded-xl bg-surface-0 p-3 text-sm dark:bg-surface-900 md:col-span-2">
                            <div class="mb-2 font-semibold">Ultimo L6 bot</div>
                            <div class="grid grid-cols-1 gap-3 md:grid-cols-2">
                                <div class="flex flex-col gap-1 leading-tight">
                                    <span><strong>L6 giocati</strong> {{ selectedSecurityFilterRow.L6PlayedCount ?? 0 }}</span>
                                    <span class="text-xs text-muted-color">Frequenza L6: ult {{ formatHands(selectedSecurityFilterRow.LastL6DeltaHands) }} · avg {{ formatHands(selectedSecurityFilterRow.AvgL6DeltaHands, 1) }}</span>
                                    <span class="text-xs text-muted-color">Range L6 {{ formatHandsRange(selectedSecurityFilterRow.MinL6DeltaHands, selectedSecurityFilterRow.MaxL6DeltaHands) }}</span>
                                </div>
                                <div class="flex flex-col gap-1 leading-tight">
                                    <div>
                                        <span class="font-semibold">Avg al L6</span>
                                        {{ formatSeconds(selectedSecurityFilterRow.LastL6AuthorizationAvgHandSeconds) }} / {{ Number(securityFilterSetup.maxAvgSeconds).toFixed(1) }}s
                                        <span class="ml-1 rounded-full px-2 py-0.5 text-xs font-semibold" :class="getRiskPillClass(isLastL6AvgFast(selectedSecurityFilterRow))">{{ getScorePoint(isLastL6AvgFast(selectedSecurityFilterRow)) }}</span>
                                    </div>
                                    <div>
                                        <span class="font-semibold">Very fast al L6</span>
                                        {{ formatSeconds(selectedSecurityFilterRow.LastL6AuthorizationAvgHandSeconds) }} / {{ Number(securityFilterSetup.veryFastSeconds).toFixed(1) }}s
                                        <span class="ml-1 rounded-full px-2 py-0.5 text-xs font-semibold" :class="getRiskPillClass(isLastL6VeryFast(selectedSecurityFilterRow))">{{ getScorePoint(isLastL6VeryFast(selectedSecurityFilterRow)) }}</span>
                                    </div>
                                    <span class="text-xs text-muted-color">Score al L6 {{ selectedSecurityFilterRow.LastL6AuthorizationScore ?? 0 }}/4</span>
                                    <span class="text-xs text-muted-color">Streak {{ selectedSecurityFilterRow.LastL6AuthorizationStreak ?? 0 }} · Mano shoe {{ selectedSecurityFilterRow.LastL6AuthorizationShoeHand ?? '-' }}</span>
                                </div>
                            </div>
                        </div>

                        <div class="rounded-xl bg-surface-0 p-3 text-sm dark:bg-surface-900 md:col-span-2">
                            <div class="mb-2 font-semibold">L8 auth perso bot</div>
                            <div class="grid grid-cols-1 gap-3 md:grid-cols-2">
                                <div class="flex flex-col gap-1 leading-tight">
                                    <span><strong>{{ selectedSecurityFilterRow.AuthorizedL8LostCount ?? 0 }}</strong> L8 persi auth</span>
                                    <span class="text-xs text-muted-color">Frequenza L8 persi: ult {{ formatHands(selectedSecurityFilterRow.LastAuthorizedL8LostDeltaHands) }} · avg {{ formatHands(selectedSecurityFilterRow.AvgAuthorizedL8LostDeltaHands, 1) }}</span>
                                    <span class="text-xs text-muted-color">Range L8 persi {{ formatHandsRange(selectedSecurityFilterRow.MinAuthorizedL8LostDeltaHands, selectedSecurityFilterRow.MaxAuthorizedL8LostDeltaHands) }}</span>
                                </div>
                                <div class="flex flex-col gap-1 leading-tight">
                                    <span><strong>Auth -> L8</strong> {{ formatDuration(selectedSecurityFilterRow.LastAuthorizedL8LossFromAuthorizationSeconds) }}</span>
                                    <div>
                                        <span class="font-semibold">Ritmo auth -> L8</span>
                                        {{ formatSeconds(selectedSecurityFilterRow.LastAuthorizedL8LossSecondsPerHand) }} / {{ Number(securityFilterSetup.maxAvgSeconds).toFixed(1) }}s
                                        <span class="ml-1 rounded-full px-2 py-0.5 text-xs font-semibold" :class="getRiskPillClass(isAuthToL8PaceFast(selectedSecurityFilterRow))">{{ getScorePoint(isAuthToL8PaceFast(selectedSecurityFilterRow)) }}</span>
                                    </div>
                                    <span class="text-xs text-muted-color">Avg auth -> L8 {{ formatDuration(selectedSecurityFilterRow.AvgAuthorizedL8LossFromAuthorizationSeconds) }}</span>
                                    <span class="text-xs text-muted-color">Range auth -> L8 {{ formatDurationRange(selectedSecurityFilterRow.MinAuthorizedL8LossFromAuthorizationSeconds, selectedSecurityFilterRow.MaxAuthorizedL8LossFromAuthorizationSeconds) }}</span>
                                    <span class="text-xs text-muted-color">Mani auth -> L8 {{ formatHands(selectedSecurityFilterRow.LastAuthorizedL8LossFromAuthorizationHands) }}</span>
                                    <span class="text-xs text-muted-color">Avg ritmo {{ formatSeconds(selectedSecurityFilterRow.AvgAuthorizedL8LossSecondsPerHand) }}</span>
                                    <span class="text-xs text-muted-color">Score auth {{ Number(selectedSecurityFilterRow.AuthorizedL8LostFromAuthorizationCount ?? 0) > 0 ? `${selectedSecurityFilterRow.LastAuthorizedL8LossAuthorizationScore ?? 0}/4` : '-' }}</span>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped>
@keyframes player-step-blink {
    0%,
    100% {
        opacity: 1;
        transform: scale(1);
        box-shadow: 0 0 0 0 rgb(59 130 246 / 0.65);
    }
    50% {
        opacity: 0.72;
        transform: scale(1.08);
        box-shadow: 0 0 0 10px rgb(59 130 246 / 0);
    }
}

.player-step-pulse {
    animation: player-step-blink 0.55s ease-in-out 3;
}
</style>
