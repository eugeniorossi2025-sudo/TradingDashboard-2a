<script setup>
import { computed, onMounted, ref, watch } from 'vue';
import { apiClient } from '@/api/apiClient';
import { DashboardService } from '@/service/DashboardService';

const props = defineProps({
    telemetry: {
        type: String,
        default: null
    },
    telemetryParsed: {
        type: Object,
        default: null
    },
    /** Righe tabella dashboard (PcCurrentStatus) — fonte STATO BOT reale per PC. */
    tableRows: {
        type: Array,
        default: () => []
    }
});

const selectedSecurityFilterBot = ref(null);
const securityFilterBotDetails = ref({});
const securityFilterDetailLoading = ref(false);
const securityFilterDetailUnavailable = ref(false);
const playerStepPulseByBot = ref({});
const playerStreakLastCountByBot = ref({});
const playerPulseTimersByBot = {};
const playerRace5FilterEnabled = ref(null);
const playerRace5FilterLoading = ref(false);
const playerRace5FilterStatus = ref('');
const playerRace5Ac3Enabled = ref(null);
const playerRace5Ac3Loading = ref(false);
const playerRace5Ac3Status = ref('');
const playerRace8FilterEnabled = ref(null);
const playerRace8FilterLoading = ref(false);
const playerRace8FilterStatus = ref('');
const playerRace8Ac3Enabled = ref(null);
const playerRace8Ac3Loading = ref(false);
const playerRace8Ac3Status = ref('');
const spotResetThreshold = ref(2);
const spotResetThresholdDraft = ref(2);
const spotCyclePbHands = ref(600);
const spotCyclePbHandsDraft = ref(600);
const spotResetThresholdLoading = ref(false);
const spotResetThresholdStatus = ref('');
const spotL6PerBotEnabled = ref(null);
const spotL6PerBotLoading = ref(false);
const spotL6PerBotStatus = ref('');
const securityFilterModuleEnabled = ref(null);
const securityFilterLoading = ref(false);
const securityFilterStatus = ref('');
const securityFilterMaxAvg = ref(null);
const securityFilterVeryFast = ref(null);
const securityFilterMinScore = ref(null);
const securityFilterMaxAvgDraft = ref(23.5);
const securityFilterVeryFastDraft = ref(21);
const securityFilterMinScoreDraft = ref(3);
const operatorCommandLoadingByBot = ref({});
const operatorCommandStatus = ref('');

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
        SpotL6ThresholdL5: api.spotL6ThresholdL5 ?? api.spotResetThresholdL5,
        SpotResetThresholdL5: api.spotL6ThresholdL5 ?? api.spotResetThresholdL5,
        SpotCyclePbHandsLimit: api.spotCyclePbHandsLimit,
        SpotPerBotOnlyEnabled: api.spotPerBotOnlyEnabled,
        SpotL6PerBotEnabled: api.spotL6PerBotEnabled,
        SpotLegacyGlobalEnabled: api.spotLegacyGlobalEnabled,
        SpotId: api.spotId,
        GlobalPauseScalping: api.globalPauseScalping,
        GlobalPauseScalpingDetails: api.globalPauseScalpingDetails,
        GlobalPauseScalpingDuration: api.globalPauseScalpingDuration,
        INC: api.inc,
        EWMA: api.ewma,
        TotalPauseScalpingSoglieActivated: api.totalPauseScalpingSoglieActivated,
        TotalPauseScalpingEWMAActivated: api.totalPauseScalpingEWMAActivated,
        SecurityFilterEnabled: api.securityFilterEnabled,
        PlayerRace5FilterEnabled: api.playerRace5FilterEnabled ?? api.playerRace5Enabled,
        PlayerRace5Ac3Enabled: api.playerRace5Ac3Enabled,
        PlayerRace8FilterEnabled: api.playerRace8FilterEnabled ?? api.playerRace8Enabled,
        PlayerRace8Ac3Enabled: api.playerRace8Ac3Enabled ?? api.playerPaceFilterEnabled,
        PlayerRace5Enabled: api.playerRace5FilterEnabled ?? api.playerRace5Enabled,
        PlayerRace8Enabled: api.playerRace8FilterEnabled ?? api.playerRace8Enabled,
        PlayerPaceFilterEnabled: api.playerRace8Ac3Enabled ?? api.playerPaceFilterEnabled,
        TotalPlayerPaceAC3Activated: api.totalPlayerPaceAC3Activated,
        ActivePlayerPaceRiskBots: api.activePlayerPaceRiskBots,
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

function pickSpotL6Authorized(row) {
    const v = pickRowField(row, 'SpotL6Authorized', 'spotL6Authorized');
    if (v !== undefined) return v;
    return pickRowField(row, 'SpotResetAuthorized', 'spotResetAuthorized');
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
        currentStreakOutcome: pickRowField(base, 'currentStreakOutcome', 'CurrentStreakOutcome'),
        PlayerRace5Alert: pickRowField(base, 'PlayerRace5Alert', 'playerRace5Alert'),
        playerRace5Alert: pickRowField(base, 'playerRace5Alert', 'PlayerRace5Alert'),
        PlayerRace5Triggered: pickRowField(base, 'PlayerRace5Triggered', 'playerRace5Triggered'),
        playerRace5Triggered: pickRowField(base, 'playerRace5Triggered', 'PlayerRace5Triggered'),
        PlayerRace5Ac3Triggered: pickRowField(base, 'PlayerRace5Ac3Triggered', 'playerRace5Ac3Triggered'),
        playerRace5Ac3Triggered: pickRowField(base, 'playerRace5Ac3Triggered', 'PlayerRace5Ac3Triggered'),
        PlayerRace8Alert: pickRowField(base, 'PlayerRace8Alert', 'playerRace8Alert'),
        playerRace8Alert: pickRowField(base, 'playerRace8Alert', 'PlayerRace8Alert'),
        PlayerRace8Ac3Triggered: pickRowField(base, 'PlayerRace8Ac3Triggered', 'playerRace8Ac3Triggered'),
        playerRace8Ac3Triggered: pickRowField(base, 'playerRace8Ac3Triggered', 'PlayerRace8Ac3Triggered'),
        PlayerPaceRiskActive: pickRowField(base, 'PlayerPaceRiskActive', 'playerPaceRiskActive'),
        playerPaceRiskActive: pickRowField(base, 'playerPaceRiskActive', 'PlayerPaceRiskActive'),
        PlayerPaceTriggeredAC3: pickRowField(base, 'PlayerPaceTriggeredAC3', 'playerPaceTriggeredAC3'),
        playerPaceTriggeredAC3: pickRowField(base, 'playerPaceTriggeredAC3', 'PlayerPaceTriggeredAC3'),
        SpotCycleId: pickRowField(base, 'SpotCycleId', 'spotCycleId'),
        spotCycleId: pickRowField(base, 'spotCycleId', 'SpotCycleId'),
        SpotL5PlayedCount: pickRowField(base, 'SpotL5PlayedCount', 'spotL5PlayedCount'),
        spotL5PlayedCount: pickRowField(base, 'spotL5PlayedCount', 'SpotL5PlayedCount'),
        SpotL5LossCount: pickRowField(base, 'SpotL5LossCount', 'spotL5LossCount'),
        spotL5LossCount: pickRowField(base, 'spotL5LossCount', 'SpotL5LossCount'),
        SpotL6GrantedCount: pickRowField(base, 'SpotL6GrantedCount', 'spotL6GrantedCount'),
        spotL6GrantedCount: pickRowField(base, 'spotL6GrantedCount', 'SpotL6GrantedCount'),
        SpotL6Authorized: pickSpotL6Authorized(base),
        spotL6Authorized: pickSpotL6Authorized(base),
        NextL5LossWillAuthorizeL6: pickRowField(base, 'NextL5LossWillAuthorizeL6', 'nextL5LossWillAuthorizeL6'),
        nextL5LossWillAuthorizeL6: pickRowField(base, 'nextL5LossWillAuthorizeL6', 'NextL5LossWillAuthorizeL6')
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

function buildLocalCollaudoPreviewRows() {
    const preview = [
        {
            computer: 'PC1',
            martingala: 5,
            spotCycleId: 1,
            spotPb: 11,
            spotL5Played: 3,
            spotL5: 1,
            spotL6Granted: 0,
            spotAuth: false,
            nextL5: true,
            shoeHand: 7
        },
        {
            computer: 'PC2',
            martingala: 3,
            spotCycleId: 1,
            spotPb: 4,
            spotL5Played: 0,
            spotL5: 0,
            spotL6Granted: 0,
            spotAuth: false,
            nextL5: false,
            shoeHand: 12
        },
        {
            computer: 'PC3',
            martingala: 5,
            spotCycleId: 1,
            spotPb: 78,
            spotL5Played: 6,
            spotL5: 2,
            spotL6Granted: 1,
            spotAuth: true,
            nextL5: false,
            shoeHand: 22
        }
    ];
    return preview.map((item) =>
        normalizeSecurityFilterBotRow(item.computer, {
            Martingala: item.martingala,
            SecurityRiskScore: 0,
            SpotCycleId: item.spotCycleId,
            SpotPbHandsPlayed: item.spotPb,
            SpotL5PlayedCount: item.spotL5Played,
            SpotL5LossCount: item.spotL5,
            SpotL6GrantedCount: item.spotL6Granted,
            SpotL6Authorized: item.spotAuth,
            NextL5LossWillAuthorizeL6: item.nextL5,
            LastShoeHand: item.shoeHand,
            PlayerStreakCount: 0
        })
    );
}

const hasLiveSecurityFilterBots = computed(() => {
    const byBot = telemetryData.value?.SecurityFilterByBot ?? telemetryData.value?.securityFilterByBot;
    return !!byBot && typeof byBot === 'object' && Object.keys(byBot).length > 0;
});

const showLocalCollaudoPreview = computed(() => import.meta.env.DEV && !hasLiveSecurityFilterBots.value);

const showLegacySpotMetricsGlobal = computed(() => {
    const legacy =
        telemetryData.value?.SpotLegacyGlobalEnabled ??
        telemetryData.value?.spotLegacyGlobalEnabled;
    return legacy === true;
});

const securityFilterRows = computed(() => {
    const byBot = telemetryData.value?.SecurityFilterByBot ?? telemetryData.value?.securityFilterByBot;
    const liveRows =
        byBot && typeof byBot === 'object'
            ? Object.entries(byBot).map(([computer, row]) => normalizeSecurityFilterBotRow(computer, row))
            : [];

    const sourceRows = liveRows.length > 0 ? liveRows : showLocalCollaudoPreview.value ? buildLocalCollaudoPreviewRows() : [];

    return sourceRows.sort((a, b) => {
            const riskDelta = getRiskRank(b) - getRiskRank(a);
            if (riskDelta !== 0) return riskDelta;

            const levelDelta = getNumber(b?.Martingala) - getNumber(a?.Martingala);
            if (levelDelta !== 0) return levelDelta;

            const avgDelta = getNumber(a?.AvgHandSeconds, Number.MAX_SAFE_INTEGER) - getNumber(b?.AvgHandSeconds, Number.MAX_SAFE_INTEGER);
            if (avgDelta !== 0) return avgDelta;

            return getBotName(a).localeCompare(getBotName(b));
        });
});

const spotResetBotRows = computed(() =>
    [...securityFilterRows.value].sort((a, b) => getBotName(a).localeCompare(getBotName(b)))
);

function getSpotCyclePbHandsLimit() {
    const t = telemetryData.value?.SpotCyclePbHandsLimit ?? telemetryData.value?.spotCyclePbHandsLimit;
    if (typeof t === 'number' && t >= 1) return t;
    if (spotCyclePbHands.value >= 1) return spotCyclePbHands.value;
    return 600;
}

const spotOpsSummary = computed(() => {
    const rows = spotResetBotRows.value;
    const l6Threshold = getSpotL6Threshold();
    return {
        perBotOnly: isSpotL6PerBotEnabled(),
        legacyL5Frozen: (telemetryData.value?.SpotL5Loss ?? telemetryData.value?.spotL5Loss ?? 0) === 0,
        l6ThresholdLabel:
            typeof l6Threshold === 'number' && l6Threshold >= 1
                ? `${l6Threshold} L5 perse per bot`
                : '—',
        botsTracked: rows.length,
        botsL6Authorized: rows.filter((row) => isSpotL6Authorized(row)).length
    };
});

function getSpotHandCount(row) {
    const spotPb = pickRowField(row, 'SpotPbHandsPlayed', 'spotPbHandsPlayed');
    if (typeof spotPb === 'number') return spotPb;
    const v = pickRowField(row, 'PBHandsPlayed', 'pbHandsPlayed');
    return typeof v === 'number' ? v : 0;
}

function getSpotCycleId(row) {
    const id = pickRowField(row, 'SpotCycleId', 'spotCycleId');
    return typeof id === 'number' && id >= 1 ? id : 1;
}

function formatSpotPbCycle(row) {
    const limit = getSpotCyclePbHandsLimit();
    const pb = getSpotHandCount(row);
    return `${pb}/${limit >= 1 ? limit : '—'}`;
}

function getSpotShoeHand(row) {
    const v = pickRowField(row, 'LastShoeHand', 'lastShoeHand');
    return typeof v === 'number' ? v : 0;
}

function getSpotMartingala(row) {
    const v = pickRowField(row, 'Martingala', 'martingala');
    return typeof v === 'number' ? v : null;
}

const securityFilterOperational = computed(() => isSecurityFilterEnabled());

const securityFilterSetup = computed(() => ({
    enabled: securityFilterOperational.value,
    minScore:
        securityFilterMinScore.value ??
        telemetryData.value?.SecurityFilterMinScore ??
        3,
    minStreak: telemetryData.value?.SecurityFilterMinStreak ?? 5,
    maxShoeHand: telemetryData.value?.SecurityFilterMaxShoeHand ?? 20,
    maxAvgSeconds:
        securityFilterMaxAvg.value ??
        telemetryData.value?.SecurityFilterMaxAvgSeconds ??
        25.85,
    veryFastSeconds:
        securityFilterVeryFast.value ??
        telemetryData.value?.SecurityFilterVeryFastSeconds ??
        23.1,
    deltaWindow: telemetryData.value?.SecurityFilterDeltaWindow ?? 8,
    playerP1P5Threshold: resolvePlayerP1P5Threshold(
        telemetryData.value?.SecurityFilterPlayerP1P5ThresholdSeconds
    ),
    preventedL6: telemetryData.value?.TotalSecurityFilterPreventedL6 ?? 0
}));

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
                triggerPlayerStepPulse(snap.bot, Math.min(snap.count, PLAYER_BLOCK_COUNT));
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

const PLAYER_BLOCK_COUNT = 8;
const FILTER_5_MIN_STREAK = 5;
const FILTER_8_MIN_STREAK = 8;
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

function resolveTelemetryFlag(pascalKey, camelKey) {
    const enabled = telemetryData.value?.[pascalKey] ?? telemetryData.value?.[camelKey];
    if (enabled === true || enabled === false) return enabled;
    return null;
}

function isPlayerRace5FilterEnabled() {
    if (playerRace5FilterEnabled.value === true || playerRace5FilterEnabled.value === false) return playerRace5FilterEnabled.value;
    return resolveTelemetryFlag('PlayerRace5FilterEnabled', 'playerRace5FilterEnabled') === true;
}

function isPlayerRace5Ac3Enabled() {
    if (playerRace5Ac3Enabled.value === true || playerRace5Ac3Enabled.value === false) return playerRace5Ac3Enabled.value;
    return resolveTelemetryFlag('PlayerRace5Ac3Enabled', 'playerRace5Ac3Enabled') === true;
}

function isPlayerRace8FilterEnabled() {
    if (playerRace8FilterEnabled.value === true || playerRace8FilterEnabled.value === false) return playerRace8FilterEnabled.value;
    return resolveTelemetryFlag('PlayerRace8FilterEnabled', 'playerRace8FilterEnabled') === true;
}

function isPlayerRace8Ac3Enabled() {
    if (playerRace8Ac3Enabled.value === true || playerRace8Ac3Enabled.value === false) return playerRace8Ac3Enabled.value;
    const direct = resolveTelemetryFlag('PlayerRace8Ac3Enabled', 'playerRace8Ac3Enabled');
    if (direct !== null) return direct === true;
    return resolveTelemetryFlag('PlayerPaceFilterEnabled', 'playerPaceFilterEnabled') === true;
}

function isSpotL6PerBotEnabled() {
    if (spotL6PerBotEnabled.value === true || spotL6PerBotEnabled.value === false) return spotL6PerBotEnabled.value;
    const t = resolveTelemetryFlag('SpotL6PerBotEnabled', 'spotL6PerBotEnabled');
    if (t !== null) return t;
    return telemetryData.value?.SpotPerBotOnlyEnabled !== false;
}

async function loadSpotL6PerBot() {
    try {
        const state = await DashboardService.getSpotL6PerBot();
        spotL6PerBotEnabled.value = state?.enabled === true;
    } catch {
        const t = resolveTelemetryFlag('SpotL6PerBotEnabled', 'spotL6PerBotEnabled');
        if (t !== null) spotL6PerBotEnabled.value = t;
    }
}

async function loadSecurityFilter() {
    try {
        const state = await DashboardService.getSecurityFilter();
        securityFilterModuleEnabled.value = state?.enabled === true;
        const maxAvg = Number(state?.maxAvgSeconds);
        const veryFast = Number(state?.veryFastSeconds);
        const minScore = Number(state?.minScore);
        if (Number.isFinite(maxAvg)) {
            securityFilterMaxAvg.value = maxAvg;
            securityFilterMaxAvgDraft.value = maxAvg;
        }
        if (Number.isFinite(veryFast)) {
            securityFilterVeryFast.value = veryFast;
            securityFilterVeryFastDraft.value = veryFast;
        }
        if (Number.isFinite(minScore)) {
            securityFilterMinScore.value = minScore;
            securityFilterMinScoreDraft.value = minScore;
        }
    } catch {
        const enabled = resolveSecurityFilterEnabledFromTelemetry();
        if (enabled !== null) securityFilterModuleEnabled.value = enabled;
        const t = telemetryData.value;
        if (t) {
            const maxAvg = Number(t.SecurityFilterMaxAvgSeconds ?? t.securityFilterMaxAvgSeconds);
            const veryFast = Number(t.SecurityFilterVeryFastSeconds ?? t.securityFilterVeryFastSeconds);
            const minScore = Number(t.SecurityFilterMinScore ?? t.securityFilterMinScore);
            if (Number.isFinite(maxAvg)) {
                securityFilterMaxAvgDraft.value = maxAvg;
                if (securityFilterMaxAvg.value === null) securityFilterMaxAvg.value = maxAvg;
            }
            if (Number.isFinite(veryFast)) {
                securityFilterVeryFastDraft.value = veryFast;
                if (securityFilterVeryFast.value === null) securityFilterVeryFast.value = veryFast;
            }
            if (Number.isFinite(minScore)) {
                securityFilterMinScoreDraft.value = minScore;
                if (securityFilterMinScore.value === null) securityFilterMinScore.value = minScore;
            }
        }
    }
}

async function setSecurityFilterModule(enabled) {
    securityFilterLoading.value = true;
    try {
        const state = await DashboardService.setSecurityFilterEnabled(enabled);
        securityFilterModuleEnabled.value = state?.enabled === true;
        securityFilterStatus.value = enabled ? 'Security Filter attivo' : 'Security Filter spento';
    } catch {
        securityFilterStatus.value = 'Errore Security Filter';
    } finally {
        securityFilterLoading.value = false;
    }
}

async function saveSecurityFilterParameters() {
    const maxAvg = Number(securityFilterMaxAvgDraft.value);
    const veryFast = Number(securityFilterVeryFastDraft.value);
    const minScore = Number(securityFilterMinScoreDraft.value);
    securityFilterLoading.value = true;
    try {
        const saved = await DashboardService.saveSecurityFilterParameters({
            maxAvgSeconds: maxAvg,
            veryFastSeconds: veryFast,
            minScore
        });
        securityFilterMaxAvg.value = Number(saved.maxAvgSeconds);
        securityFilterVeryFast.value = Number(saved.veryFastSeconds);
        securityFilterMinScore.value = Number(saved.minScore);
        securityFilterMaxAvgDraft.value = securityFilterMaxAvg.value;
        securityFilterVeryFastDraft.value = securityFilterVeryFast.value;
        securityFilterMinScoreDraft.value = securityFilterMinScore.value;
        securityFilterStatus.value = 'Parametri Security Filter salvati';
    } catch (err) {
        const msg = err?.response?.data?.message ?? 'Parametri non validi';
        securityFilterStatus.value = msg;
    } finally {
        securityFilterLoading.value = false;
    }
}

async function setSpotL6PerBot(enabled) {
    spotL6PerBotLoading.value = true;
    try {
        const state = await DashboardService.setSpotL6PerBot(enabled);
        spotL6PerBotEnabled.value = state?.enabled === true;
        spotL6PerBotStatus.value = enabled ? 'SPOT L6 per bot attivo' : 'SPOT L6 per bot spento';
    } catch {
        spotL6PerBotStatus.value = 'Errore SPOT L6 per bot';
    } finally {
        spotL6PerBotLoading.value = false;
    }
}

function anyPlayerRaceCommandEnabled() {
    return isPlayerRace5FilterEnabled() || isPlayerRace5Ac3Enabled() || isPlayerRace8FilterEnabled() || isPlayerRace8Ac3Enabled();
}

function isSpotThresholdConfigured() {
    const t =
        telemetryData.value?.SpotL6ThresholdL5 ??
        telemetryData.value?.spotL6ThresholdL5 ??
        telemetryData.value?.SpotResetThresholdL5 ??
        telemetryData.value?.spotResetThresholdL5;
    if (typeof t === 'number' && t >= 1) return true;
    return spotResetThreshold.value >= 1;
}

function getSpotL6Threshold() {
    if (!isSpotThresholdConfigured()) return '—';
    const t =
        telemetryData.value?.SpotL6ThresholdL5 ??
        telemetryData.value?.spotL6ThresholdL5 ??
        telemetryData.value?.SpotResetThresholdL5 ??
        telemetryData.value?.spotResetThresholdL5;
    if (typeof t === 'number' && t >= 1) return t;
    if (spotResetThreshold.value >= 1) return spotResetThreshold.value;
    return 2;
}

function getSpotResetThreshold() {
    return getSpotL6Threshold();
}

function getSpotL5PlayedCount(row) {
    const count = pickRowField(row, 'SpotL5PlayedCount', 'spotL5PlayedCount');
    return typeof count === 'number' ? count : 0;
}

function getSpotL5LossCount(row) {
    const count = pickRowField(row, 'SpotL5LossCount', 'spotL5LossCount');
    return typeof count === 'number' ? count : 0;
}

function getSpotL6GrantedCount(row) {
    const count = pickRowField(row, 'SpotL6GrantedCount', 'spotL6GrantedCount');
    return typeof count === 'number' ? count : 0;
}

function getNextL5LossPreviewLabel(row) {
    if (!isSpotThresholdConfigured()) return '—';
    if (isSpotL6Authorized(row)) return 'NON SERVE: L6 GIÀ AUTORIZZATO';
    const nextFlag = pickRowField(row, 'NextL5LossWillAuthorizeL6', 'nextL5LossWillAuthorizeL6');
    if (nextFlag === true) return 'AUTORIZZA L6';
    const threshold = getSpotL6Threshold();
    const losses = getSpotL5LossCount(row);
    if (typeof threshold === 'number' && losses === threshold - 1) return 'AUTORIZZA L6';
    return 'NON autorizza L6';
}

function isSpotL6Authorized(row) {
    if (!isSpotThresholdConfigured()) return false;
    return pickSpotL6Authorized(row) === true;
}

function isSpotResetAuthorized(row) {
    return isSpotL6Authorized(row);
}

function getSpotL6StatusLabel(row) {
    if (!isSpotThresholdConfigured()) return 'SOGLIA NON CONFIGURATA';
    return isSpotL6Authorized(row) ? 'L6 AUTORIZZATO' : 'L6 NON AUTORIZZATO';
}

function getSpotResetStatusLabel(row) {
    return getSpotL6StatusLabel(row);
}

function getSpotResetBlockClass(row) {
    if (!isSpotThresholdConfigured()) {
        return 'border-surface-200 bg-surface-50/60 dark:border-surface-700 dark:bg-surface-900/40';
    }
    if (isSpotL6Authorized(row)) {
        return 'border-emerald-300 bg-emerald-50 ring-1 ring-emerald-300/80 dark:border-emerald-700 dark:bg-emerald-950/35 dark:ring-emerald-800';
    }
    return 'border-surface-200 bg-surface-50/80 dark:border-surface-700 dark:bg-surface-900/30';
}

function getSpotResetStatusClass(row) {
    if (!isSpotThresholdConfigured()) return 'text-muted-color';
    if (isSpotL6Authorized(row)) return 'text-emerald-700 dark:text-emerald-300';
    return 'text-muted-color';
}

function getSpotResetBadgeClass(row) {
    if (!isSpotThresholdConfigured()) return 'border-surface-300 text-muted-color';
    if (isSpotL6Authorized(row)) return 'border-emerald-400 bg-emerald-100 text-emerald-800 dark:border-emerald-700 dark:bg-emerald-900/50 dark:text-emerald-200';
    return 'border-surface-300 text-muted-color dark:border-surface-600';
}

async function loadSpotResetThreshold() {
    try {
        const state = await DashboardService.getSpotResetThreshold();
        const t = Number(state?.threshold);
        if (t >= 1) {
            spotResetThreshold.value = t;
            spotResetThresholdDraft.value = t;
        }
    } catch {
        const t =
            telemetryData.value?.SpotL6ThresholdL5 ??
            telemetryData.value?.spotL6ThresholdL5 ??
            telemetryData.value?.SpotResetThresholdL5 ??
            telemetryData.value?.spotResetThresholdL5;
        if (typeof t === 'number' && t >= 1) {
            spotResetThreshold.value = t;
            spotResetThresholdDraft.value = t;
        }
    }
    try {
        const cycle = await DashboardService.getSpotCyclePbHands();
        const h = Number(cycle?.hands);
        if (h >= 1) {
            spotCyclePbHands.value = h;
            spotCyclePbHandsDraft.value = h;
        }
    } catch {
        const h = telemetryData.value?.SpotCyclePbHandsLimit ?? telemetryData.value?.spotCyclePbHandsLimit;
        if (typeof h === 'number' && h >= 1) {
            spotCyclePbHands.value = h;
            spotCyclePbHandsDraft.value = h;
        }
    }
}

async function saveSpotL6ThresholdOnly() {
    const l5Value = Number(spotResetThresholdDraft.value);
    spotL6PerBotLoading.value = true;
    try {
        const state = await DashboardService.setSpotResetThreshold(l5Value);
        const t = Number(state?.threshold);
        spotResetThreshold.value = t;
        spotResetThresholdDraft.value = t;
        spotL6PerBotStatus.value = `Soglia L6 salvata: ${t}`;
    } catch (err) {
        const msg = err?.response?.data?.message ?? err?.response?.data?.errors?.[0] ?? 'Soglia non valida (1–99)';
        spotL6PerBotStatus.value = msg;
    } finally {
        spotL6PerBotLoading.value = false;
    }
}

async function saveSpotCyclePbHandsOnly() {
    const cycleValue = Number(spotCyclePbHandsDraft.value);
    spotResetThresholdLoading.value = true;
    try {
        const cycleState = await DashboardService.setSpotCyclePbHands(cycleValue);
        const h = Number(cycleState?.hands);
        spotCyclePbHands.value = h;
        spotCyclePbHandsDraft.value = h;
        spotResetThresholdStatus.value = `Ciclo SPOT PB salvato: ${h} mani (reset alla mano ${h + 1})`;
    } catch (err) {
        const msg = err?.response?.data?.message ?? 'Ciclo PB non valido (1–99999)';
        spotResetThresholdStatus.value = msg;
    } finally {
        spotResetThresholdLoading.value = false;
    }
}

async function loadPlayerRace5Filter() {
    try {
        const state = await DashboardService.getPlayerRace5Filter();
        playerRace5FilterEnabled.value = state?.enabled === true;
    } catch {
        const t = resolveTelemetryFlag('PlayerRace5FilterEnabled', 'playerRace5FilterEnabled');
        if (t !== null) playerRace5FilterEnabled.value = t;
    }
}

async function loadPlayerRace5Ac3() {
    try {
        const state = await DashboardService.getPlayerRace5Ac3();
        playerRace5Ac3Enabled.value = state?.enabled === true;
    } catch {
        const t = resolveTelemetryFlag('PlayerRace5Ac3Enabled', 'playerRace5Ac3Enabled');
        if (t !== null) playerRace5Ac3Enabled.value = t;
    }
}

async function loadPlayerRace8Filter() {
    try {
        const state = await DashboardService.getPlayerRace8Filter();
        playerRace8FilterEnabled.value = state?.enabled === true;
    } catch {
        const t = resolveTelemetryFlag('PlayerRace8FilterEnabled', 'playerRace8FilterEnabled');
        if (t !== null) playerRace8FilterEnabled.value = t;
    }
}

async function loadPlayerRace8Ac3() {
    try {
        const state = await DashboardService.getPlayerRace8Ac3();
        playerRace8Ac3Enabled.value = state?.enabled === true;
    } catch {
        const t = resolveTelemetryFlag('PlayerRace8Ac3Enabled', 'playerRace8Ac3Enabled');
        if (t !== null) playerRace8Ac3Enabled.value = t;
    }
}

async function setPlayerRace5Filter(enabled) {
    playerRace5FilterLoading.value = true;
    try {
        const state = await DashboardService.setPlayerRace5Filter(enabled);
        playerRace5FilterEnabled.value = state?.enabled === true;
        playerRace5FilterStatus.value = enabled ? 'Filtro 5 attivo' : 'Filtro 5 spento';
    } catch {
        playerRace5FilterStatus.value = 'Errore Filtro 5';
    } finally {
        playerRace5FilterLoading.value = false;
    }
}

async function setPlayerRace5Ac3(enabled) {
    playerRace5Ac3Loading.value = true;
    try {
        const state = await DashboardService.setPlayerRace5Ac3(enabled);
        playerRace5Ac3Enabled.value = state?.enabled === true;
        playerRace5Ac3Status.value = enabled ? 'AC3 Filtro 5 attivo' : 'AC3 Filtro 5 spento';
    } catch {
        playerRace5Ac3Status.value = 'Errore AC3 Filtro 5';
    } finally {
        playerRace5Ac3Loading.value = false;
    }
}

async function setPlayerRace8Filter(enabled) {
    playerRace8FilterLoading.value = true;
    try {
        const state = await DashboardService.setPlayerRace8Filter(enabled);
        playerRace8FilterEnabled.value = state?.enabled === true;
        playerRace8FilterStatus.value = enabled ? 'Filtro 8 attivo' : 'Filtro 8 spento';
    } catch {
        playerRace8FilterStatus.value = 'Errore Filtro 8';
    } finally {
        playerRace8FilterLoading.value = false;
    }
}

async function setPlayerRace8Ac3(enabled) {
    playerRace8Ac3Loading.value = true;
    try {
        const state = await DashboardService.setPlayerRace8Ac3(enabled);
        playerRace8Ac3Enabled.value = state?.enabled === true;
        playerRace8Ac3Status.value = enabled ? 'AC3 Filtro 8 attivo' : 'AC3 Filtro 8 spento';
    } catch {
        playerRace8Ac3Status.value = 'Errore AC3 Filtro 8';
    } finally {
        playerRace8Ac3Loading.value = false;
    }
}

onMounted(() => {
    loadSecurityFilter();
    loadSpotResetThreshold();
    loadSpotL6PerBot();
    loadPlayerRace5Filter();
    loadPlayerRace5Ac3();
    loadPlayerRace8Filter();
    loadPlayerRace8Ac3();
});

watch(
    () => props.telemetryParsed,
    () => {
        const r5f = resolveTelemetryFlag('PlayerRace5FilterEnabled', 'playerRace5FilterEnabled');
        if (r5f !== null && playerRace5FilterEnabled.value === null) playerRace5FilterEnabled.value = r5f;
        const sl6 = resolveTelemetryFlag('SpotL6PerBotEnabled', 'spotL6PerBotEnabled');
        if (sl6 !== null && spotL6PerBotEnabled.value === null) spotL6PerBotEnabled.value = sl6;
        const sf = resolveSecurityFilterEnabledFromTelemetry();
        if (sf !== null && securityFilterModuleEnabled.value === null) securityFilterModuleEnabled.value = sf;
        const r5a = resolveTelemetryFlag('PlayerRace5Ac3Enabled', 'playerRace5Ac3Enabled');
        if (r5a !== null && playerRace5Ac3Enabled.value === null) playerRace5Ac3Enabled.value = r5a;
        const r8f = resolveTelemetryFlag('PlayerRace8FilterEnabled', 'playerRace8FilterEnabled');
        if (r8f !== null && playerRace8FilterEnabled.value === null) playerRace8FilterEnabled.value = r8f;
        const r8a = resolveTelemetryFlag('PlayerRace8Ac3Enabled', 'playerRace8Ac3Enabled');
        if (r8a !== null && playerRace8Ac3Enabled.value === null) playerRace8Ac3Enabled.value = r8a;
    },
    { deep: true }
);

function getPlayerStreakSteps(row) {
    const metrics = getPlayerStreakMetrics(row);
    const active = metrics.count > 0 && metrics.outcome === 'P';
    return Array.from({ length: PLAYER_BLOCK_COUNT }, (_, i) => ({
        label: `P${i + 1}`,
        filled: active && metrics.count >= i + 1
    }));
}

function isPlayerRace5Alert(row) {
    if (!isPlayerRace5FilterEnabled()) return false;
    const alert = pickRowField(row, 'PlayerRace5Alert', 'playerRace5Alert');
    if (alert === true) return true;
    const triggered = pickRowField(row, 'PlayerRace5Triggered', 'playerRace5Triggered');
    if (triggered === true) return true;
    const metrics = getPlayerStreakMetrics(row);
    return metrics.outcome === 'P' && metrics.count >= FILTER_5_MIN_STREAK;
}

function isPlayerRace5Ac3(row) {
    if (!isPlayerRace5Ac3Enabled()) return false;
    const triggered = pickRowField(row, 'PlayerRace5Ac3Triggered', 'playerRace5Ac3Triggered');
    if (triggered === true) return true;
    const metrics = getPlayerStreakMetrics(row);
    return metrics.outcome === 'P' && metrics.count >= FILTER_5_MIN_STREAK;
}

function isPlayerRace8Alert(row) {
    if (!isPlayerRace8FilterEnabled()) return false;
    const alert = pickRowField(row, 'PlayerRace8Alert', 'playerRace8Alert');
    if (alert === true) return true;
    const metrics = getPlayerStreakMetrics(row);
    return metrics.outcome === 'P' && metrics.count >= FILTER_8_MIN_STREAK;
}

function isPlayerRace8Ac3(row) {
    if (!isPlayerRace8Ac3Enabled()) return false;
    const triggered = pickRowField(row, 'PlayerRace8Ac3Triggered', 'playerRace8Ac3Triggered');
    if (triggered === true) return true;
    const metrics = getPlayerStreakMetrics(row);
    return metrics.outcome === 'P' && metrics.count >= FILTER_8_MIN_STREAK;
}

function isPlayerPaceAc3Triggered(row) {
    return isPlayerRace5Ac3(row) || isPlayerRace8Ac3(row);
}

function getPlayerRaceProgressLines(row) {
    if (!row) return [];
    const metrics = getPlayerStreakMetrics(row);
    const active = metrics.count > 0 && metrics.outcome === 'P';
    const lines = [];

    if (isPlayerRace8Ac3(row)) {
        lines.push({
            key: 'ac3-8',
            label: 'AC3 Filtro 8',
            class: 'text-sm font-bold text-orange-600 dark:text-orange-300'
        });
    } else if (isPlayerRace5Ac3(row)) {
        lines.push({
            key: 'ac3-5',
            label: 'AC3 Filtro 5',
            class: 'text-sm font-bold text-orange-600 dark:text-orange-300'
        });
    }

    if (active) {
        if (isPlayerRace5FilterEnabled()) {
            lines.push({
                key: 'filter-5',
                label: `Filtro 5: ${metrics.count}/${FILTER_5_MIN_STREAK}`,
                class: 'text-xs font-semibold text-amber-700 dark:text-amber-300'
            });
        }
        if (isPlayerRace8FilterEnabled()) {
            lines.push({
                key: 'filter-8',
                label: `Filtro 8: ${metrics.count}/${FILTER_8_MIN_STREAK}`,
                class: 'text-xs font-semibold text-blue-700 dark:text-blue-300'
            });
        }
    }

    return lines;
}

function formatPlayerRaceProgressSummary(row) {
    const filterLines = getPlayerRaceProgressLines(row).filter((line) => line.key.startsWith('filter-'));
    if (!filterLines.length) return '';
    return filterLines.map((line) => line.label).join(' · ');
}

function findDashboardTableRow(row) {
    const pc = getBotName(row);
    if (!pc || pc === '-') return null;
    const normalized = pc.trim().toUpperCase();
    return (
        props.tableRows.find((tableRow) => {
            const key = String(tableRow?.computer ?? tableRow?.Computer ?? '').trim().toUpperCase();
            return key === normalized;
        }) ?? null
    );
}

function getRealBotStateLabel(row) {
    const tableRow = findDashboardTableRow(row);
    const raw = tableRow?.stato ?? tableRow?.Stato;
    if (raw === undefined || raw === null || String(raw).trim() === '') {
        return 'non disponibile';
    }
    const cleaned = String(raw)
        .replace(/^Stato Bot:\s*/i, '')
        .trim();
    return cleaned || 'non disponibile';
}

function getRealBotStateClass(row) {
    const label = getRealBotStateLabel(row);
    if (label === 'non disponibile') return 'text-muted-color italic';
    return 'text-surface-900 dark:text-surface-0';
}

/** Player Race / Security Filter risk — NON stato macchina bot. */
function getFilterStateLabel(row) {
    const label = getRiskLabel(row);
    const mapped = {
        'RACE OFF': 'Race Off',
        'PAUSA ATTIVA': 'Pausa Security Filter',
        RISCHIO: 'Rischio Security Filter',
        'RISCHIO PLAYER — AC3': 'AC3',
        'AC3 P8': 'AC3',
        'AC3 P5': 'AC3',
        'Filtro 8': 'Filtro 8',
        'Filtro 5': 'Filtro 5',
        NORMALE: 'Nessuno'
    };
    return mapped[label] ?? label;
}

function getFilterStateClass(row) {
    const label = getFilterStateLabel(row);
    if (label === 'Pausa Security Filter' || label === 'Rischio Security Filter') return 'text-red-600 dark:text-red-300';
    if (label === 'AC3') return 'text-orange-600 dark:text-orange-300';
    if (label === 'Filtro 8') return 'text-blue-700 dark:text-blue-300';
    if (label === 'Filtro 5') return 'text-amber-700 dark:text-amber-300';
    if (label === 'Race Off') return 'text-muted-color';
    if (label === 'Nessuno') return 'text-emerald-700 dark:text-emerald-300';
    return 'text-muted-color';
}

function operatorCommandLoadingKey(pc, kind) {
    return `${pc}:${kind}`;
}

function isOperatorCommandLoading(row, kind) {
    const pc = getBotName(row);
    return operatorCommandLoadingByBot.value[operatorCommandLoadingKey(pc, kind)] === true;
}

async function sendControlRoomCommand(row, endpoint, kind, label) {
    const pc = getBotName(row);
    if (!pc || pc === '-') return;

    const loadingKey = operatorCommandLoadingKey(pc, kind);
    operatorCommandLoadingByBot.value = { ...operatorCommandLoadingByBot.value, [loadingKey]: true };
    operatorCommandStatus.value = '';

    try {
        const response = await apiClient.post(endpoint, { pc });
        const data = response?.data;
        if (data?.success === false) {
            throw new Error(data?.message || 'Errore comando');
        }
        operatorCommandStatus.value = `Comando ${label} inviato a ${pc}`;
    } catch (err) {
        const msg =
            err?.response?.data?.message ??
            err?.response?.data?.error ??
            err?.message ??
            `Errore invio comando a ${pc}`;
        operatorCommandStatus.value = msg;
    } finally {
        operatorCommandLoadingByBot.value = { ...operatorCommandLoadingByBot.value, [loadingKey]: false };
    }
}

function continueBot(row) {
    return sendControlRoomCommand(row, '/api/control-room/commands/continue', 'continue', 'CONTINUA');
}

function resetBotMartingale(row) {
    return sendControlRoomCommand(row, '/api/control-room/commands/reset-martingale', 'reset', 'AZZERA MARTINGALA');
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
    const threshold = resolvePlayerP1P5Threshold(securityFilterSetup.value.playerP1P5Threshold);
    const metrics = getPlayerStreakMetrics(row);
    const available = hasPlayerPaceTelemetry(row);
    const active = metrics.count > 0 && metrics.outcome === 'P';
    const steps = getPlayerStreakSteps(row);

    const deltas = Array.from({ length: PLAYER_BLOCK_COUNT - 1 }, (_, i) => {
        const n = i + 1;
        const idx = n - 1;
        const intervalReady = active && metrics.count >= n + 1;
        const seconds = intervalReady && metrics.intervals[idx] > 0 ? metrics.intervals[idx] : null;
        return {
            label: `P${n}→P${n + 1}`,
            seconds,
            visible: intervalReady,
            pending: intervalReady && seconds == null
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
    } else {
        const progressLines = getPlayerRaceProgressLines(row);
        status = isPlayerRace8Ac3(row) || isPlayerRace5Ac3(row) ? 'risk' : 'partial';
        statusLabel = progressLines.length
            ? progressLines.map((line) => line.label).join(' · ')
            : `Streak ${metrics.count}/${PLAYER_BLOCK_COUNT}`;
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

function resolveSecurityFilterEnabledFromTelemetry() {
    const enabled =
        telemetryData.value?.SecurityFilterEnabled ?? telemetryData.value?.securityFilterEnabled;
    if (enabled === true || enabled === false) return enabled;
    return null;
}

function isSecurityFilterEnabled() {
    if (securityFilterModuleEnabled.value === true || securityFilterModuleEnabled.value === false) {
        return securityFilterModuleEnabled.value;
    }
    return resolveSecurityFilterEnabledFromTelemetry() === true;
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
    if (!isSecurityFilterEnabled()) {
        if (!anyPlayerRaceCommandEnabled()) return 'RACE OFF';
        if (isPlayerPaceAc3Triggered(row)) return 'RISCHIO PLAYER — AC3';
        if (isPlayerRace8Ac3(row)) return 'AC3 P8';
        if (isPlayerRace5Ac3(row)) return 'AC3 P5';
        if (isPlayerRace8Alert(row)) return 'Filtro 8';
        if (isPlayerRace5Alert(row)) return 'Filtro 5';
        return 'NORMALE';
    }
    if (row?.PauseBot || row?.SecurityFilterActive) return 'PAUSA ATTIVA';
    if (isRapidTriggerActive(row)) return 'COMPRESSIONE L5';
    if (isPlayerPaceAc3Triggered(row)) return 'RISCHIO PLAYER — AC3';
    if (isPlayerRace8Ac3(row)) return 'AC3 P8';
    if (isPlayerRace5Ac3(row)) return 'AC3 P5';
    if (isPlayerRace8Alert(row)) return 'Filtro 8';
    if (isPlayerRace5Alert(row)) return 'Filtro 5';
    return 'NORMALE';
}

function getSummaryPillClass(row) {
    const label = getSummaryPill(row);
    if (label === 'PAUSA ATTIVA' || label === 'COMPRESSIONE L5') return 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-300 animate-pulse';
    if (label === 'RISCHIO PLAYER — AC3' || label === 'RISCHIO PLAYER') return 'bg-orange-100 text-orange-700 dark:bg-orange-900/30 dark:text-orange-300';
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
    if (!isSecurityFilterEnabled()) {
        if (isPlayerPaceAc3Triggered(row)) return 2;
        if (isPlayerRace8Ac3(row) || isPlayerRace5Ac3(row) || isPlayerRace8Alert(row) || isPlayerRace5Alert(row)) return 1;
        return 0;
    }
    const score = Number(row?.SecurityRiskScore ?? 0);
    if (row?.PauseBot || row?.SecurityFilterActive || isRapidTriggerActive(row) || score >= 3) return 2;
    if (isPlayerPaceAc3Triggered(row)) return 2;
    if (isPlayerRace8Ac3(row) || isPlayerRace5Ac3(row) || isPlayerRace8Alert(row) || isPlayerRace5Alert(row)) return 1;
    return 0;
}

function getRiskLabel(row) {
    if (!isSecurityFilterEnabled()) {
        if (!anyPlayerRaceCommandEnabled()) return 'RACE OFF';
        if (isPlayerPaceAc3Triggered(row)) return 'RISCHIO PLAYER — AC3';
        if (isPlayerRace8Ac3(row)) return 'AC3 P8';
        if (isPlayerRace5Ac3(row)) return 'AC3 P5';
        if (isPlayerRace8Alert(row)) return 'Filtro 8';
        if (isPlayerRace5Alert(row)) return 'Filtro 5';
        return 'NORMALE';
    }
    if (row?.PauseBot || row?.SecurityFilterActive) return 'PAUSA ATTIVA';
    if (isRapidTriggerActive(row) || Number(row?.SecurityRiskScore ?? 0) >= 3) return 'RISCHIO';
    if (isPlayerPaceAc3Triggered(row)) return 'RISCHIO PLAYER — AC3';
    if (isPlayerRace8Ac3(row)) return 'AC3 P8';
    if (isPlayerRace5Ac3(row)) return 'AC3 P5';
    if (isPlayerRace8Alert(row)) return 'Filtro 8';
    if (isPlayerRace5Alert(row)) return 'Filtro 5';
    return 'NORMALE';
}

function getRiskStripReason(row) {
    if (row?.PauseBot || row?.SecurityFilterActive) return 'pausa';
    if (isRapidTriggerActive(row)) return 'trigger L5';
    if (Number(row?.SecurityRiskScore ?? 0) >= 3) return `score ${row.SecurityRiskScore}/4`;
    if (isPlayerRace8Ac3(row)) return 'AC3 8 PLAYER';
    if (isPlayerRace5Ac3(row)) return 'AC3 5 PLAYER';
    if (isPlayerRace8Alert(row)) return '8 PLAYER';
    if (isPlayerRace5Alert(row)) return '5 PLAYER';
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
    if (isPlayerPaceAc3Triggered(row)) return 'AC3';
    const progressSummary = formatPlayerRaceProgressSummary(row);
    if (progressSummary) return progressSummary;
    const metrics = getPlayerStreakMetrics(row);
    if (anyPlayerRaceCommandEnabled() && metrics.count > 0) {
        return `Streak ${metrics.count}/${PLAYER_BLOCK_COUNT}`;
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
    if (summary && isPlayerRace8Ac3(summary)) {
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

    <!-- ================= SPOT METRICS LEGACY (solo se globale attivo) ================= -->
    <template v-if="showLegacySpotMetricsGlobal">
    <div class="col-span-12 mt-6">
        <h3 class="text-xl font-semibold mb-4">SPOT LEGACY GLOBALE</h3>
        <p class="text-sm text-muted-color mb-2">Contatori globali legacy — non usare con SPOT L6 per bot attivo.</p>
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
        <div class="card opacity-60">
            <span class="block text-muted-color font-medium mb-4">Spot L5 Loss (legacy globale)</span>
            <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">
                {{ telemetryData?.SpotL5Loss ?? 0 }}
            </div>
            <div class="text-xs text-muted-color mt-2">Disattivato — usare SPOT L6 per bot in Control Room</div>
        </div>
    </div>
    </template>

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
        <h3 class="text-xl font-semibold m-0 mb-4">Security Filter</h3>
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

    <!-- ================= SPOT L6 PER BOT (operativo) ================= -->
    <div class="col-span-12">
        <div class="card border-2 border-cyan-300/80 dark:border-cyan-800">
            <div class="rounded-xl border border-cyan-200 bg-gradient-to-br from-cyan-50/90 to-surface-0 p-5 dark:border-cyan-900 dark:from-cyan-950/30 dark:to-surface-900">
                <div class="mb-4">
                    <h3 class="m-0 text-xl font-bold uppercase tracking-wide text-cyan-900 dark:text-cyan-100">SPOT L6 PER BOT</h3>
                </div>

                <div class="mb-4 rounded-lg border border-cyan-200/80 bg-white/70 p-4 dark:border-cyan-900/60 dark:bg-surface-950/40">
                    <div class="text-xs font-bold uppercase tracking-wide text-muted-color mb-2">Ciclo SPOT</div>
                    <p class="text-sm leading-relaxed text-surface-800 dark:text-surface-100">
                        Ogni bot ha il proprio ciclo SPOT PB (es. 598/600).
                        Alla mano
                        <strong>{{ getSpotCyclePbHandsLimit() }}</strong>
                        vedi
                        <strong>{{ getSpotCyclePbHandsLimit() }}/{{ getSpotCyclePbHandsLimit() }}</strong>.
                        Alla mano successiva (
                        <strong>{{ getSpotCyclePbHandsLimit() + 1 }}</strong>
                        ): nuovo Spot ciclo ID, contatore PB a
                        <strong>0</strong>
                        e azzeramento L5/L6 solo per quel bot.
                    </p>
                    <div class="mt-3 flex flex-wrap items-center gap-2 text-base font-semibold text-surface-900 dark:text-surface-0">
                        <span>Mani PB per ciclo:</span>
                        <input
                            v-model.number="spotCyclePbHandsDraft"
                            type="number"
                            min="1"
                            max="99999"
                            class="w-20 rounded-lg border border-cyan-400 bg-white px-2 py-1 text-center text-xl font-bold dark:border-cyan-700 dark:bg-surface-900"
                        />
                    </div>

                    <p class="mt-3 text-xs font-semibold text-muted-color">
                        ATTIVA/SPEGNI e soglia L6: pannello coordinato sopra con Filtro 5/8 e AC3.
                        I bot non si sommano tra loro.
                    </p>
                    <div class="mt-3 flex flex-wrap items-center gap-3">
                        <button
                            type="button"
                            class="rounded-lg bg-cyan-700 px-5 py-2 text-sm font-bold uppercase tracking-wide text-white hover:bg-cyan-800 disabled:opacity-60"
                            :disabled="spotResetThresholdLoading"
                            @click="saveSpotCyclePbHandsOnly"
                        >
                            Salva ciclo PB
                        </button>
                        <span v-if="spotResetThresholdStatus" class="text-xs text-primary">{{ spotResetThresholdStatus }}</span>
                    </div>
                </div>

                <div class="mb-4 grid grid-cols-2 gap-2 text-sm md:grid-cols-2 xl:grid-cols-4">
                    <div class="rounded-lg bg-surface-100 px-3 py-2 dark:bg-surface-800">
                        <div class="text-[10px] uppercase text-muted-color">Modalità</div>
                        <div class="text-sm font-bold" :class="isSpotL6PerBotEnabled() ? 'text-emerald-700 dark:text-emerald-300' : 'text-red-500'">
                            {{ isSpotL6PerBotEnabled() ? 'SPOT PER BOT ON' : 'SPOT PER BOT OFF' }}
                        </div>
                    </div>
                    <div class="rounded-lg bg-surface-100 px-3 py-2 dark:bg-surface-800">
                        <div class="text-[10px] uppercase text-muted-color">Soglia L6</div>
                        <div class="text-sm font-bold tabular-nums">{{ spotOpsSummary.l6ThresholdLabel }}</div>
                    </div>
                    <div class="rounded-lg bg-surface-100 px-3 py-2 dark:bg-surface-800">
                        <div class="text-[10px] uppercase text-muted-color">Bot tracciati</div>
                        <div class="text-lg font-bold">{{ spotOpsSummary.botsTracked }}</div>
                    </div>
                    <div class="rounded-lg bg-surface-100 px-3 py-2 dark:bg-surface-800">
                        <div class="text-[10px] uppercase text-muted-color">L5/L6 globale legacy</div>
                        <div class="text-sm font-bold text-emerald-600 dark:text-emerald-400">SPENTO</div>
                    </div>
                </div>

                <div
                    v-if="showLocalCollaudoPreview"
                    class="mb-3 rounded-lg border border-amber-300 bg-amber-50 px-3 py-2 text-xs font-semibold text-amber-900 dark:border-amber-800 dark:bg-amber-950/40 dark:text-amber-200"
                >
                    Anteprima collaudo locale — dati esempio PC1–PC4 finché Decisore non scrive telemetry in DB.
                </div>

                <div v-if="spotResetBotRows.length === 0" class="rounded-lg border border-dashed border-cyan-300 px-4 py-6 text-center text-muted-color">
                    Nessun bot in telemetry. Avvia Decisore e attendi feed per vedere Spot mano / L5 / stato per PC.
                </div>

                <div v-else class="grid grid-cols-1 gap-3 sm:grid-cols-2 xl:grid-cols-4">
                    <div
                        v-for="row in spotResetBotRows"
                        :key="`spot-${getBotName(row)}`"
                        class="rounded-xl border p-4"
                        :class="getSpotResetBlockClass(row)"
                    >
                        <div class="mb-2 text-lg font-bold text-surface-900 dark:text-surface-0">{{ getBotName(row) }}</div>
                        <div class="space-y-1.5 text-sm">
                            <div class="flex justify-between gap-2">
                                <span class="text-muted-color">Spot ciclo ID</span>
                                <span class="font-bold tabular-nums">{{ getSpotCycleId(row) }}</span>
                            </div>
                            <div class="flex justify-between gap-2">
                                <span class="text-muted-color">Ciclo SPOT PB</span>
                                <span class="font-bold tabular-nums">{{ formatSpotPbCycle(row) }}</span>
                            </div>
                            <div class="flex justify-between gap-2">
                                <span class="text-muted-color">Mano shoe</span>
                                <span class="font-bold tabular-nums">{{ getSpotShoeHand(row) }}</span>
                            </div>
                            <div class="flex justify-between gap-2">
                                <span class="text-muted-color">Livello</span>
                                <span class="font-bold">L{{ getSpotMartingala(row) ?? '—' }}</span>
                            </div>
                            <div class="flex justify-between gap-2 border-t border-surface-200 pt-2 dark:border-surface-700">
                                <span class="text-muted-color">L5 giocate ciclo</span>
                                <span class="font-bold tabular-nums">{{ getSpotL5PlayedCount(row) }}</span>
                            </div>
                            <div class="flex justify-between gap-2">
                                <span class="font-semibold text-surface-900 dark:text-surface-0">L5 perse ciclo</span>
                                <span class="text-base font-bold tabular-nums">
                                    {{ getSpotL5LossCount(row) }}/{{ isSpotThresholdConfigured() ? getSpotL6Threshold() : '—' }}
                                </span>
                            </div>
                            <div class="flex justify-between gap-2">
                                <span class="text-muted-color">L6 concessi ciclo</span>
                                <span class="font-bold tabular-nums">{{ getSpotL6GrantedCount(row) }}</span>
                            </div>
                            <div class="flex flex-col gap-1 border-t border-surface-200 pt-2 dark:border-surface-700">
                                <span class="font-semibold text-muted-color">Prossima L5 persa</span>
                                <span
                                    class="text-xs font-bold leading-snug"
                                    :class="
                                        getNextL5LossPreviewLabel(row).includes('AUTORIZZA L6') && !getNextL5LossPreviewLabel(row).includes('NON')
                                            ? 'text-emerald-700 dark:text-emerald-300'
                                            : 'text-surface-800 dark:text-surface-100'
                                    "
                                >
                                    {{ getNextL5LossPreviewLabel(row) }}
                                </span>
                            </div>
                            <div class="flex flex-wrap items-center justify-between gap-2 pt-1">
                                <span class="font-semibold text-muted-color">Stato</span>
                                <span class="rounded-full border px-2.5 py-0.5 text-[11px] font-bold uppercase tracking-wide" :class="getSpotResetBadgeClass(row)">
                                    {{ getSpotResetStatusLabel(row) }}
                                </span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div class="col-span-12">
        <div class="card">
            <div class="mb-4 grid grid-cols-1 gap-3 md:grid-cols-2 xl:grid-cols-3 2xl:grid-cols-6">
                <div class="rounded-xl border border-slate-300 bg-slate-50/50 p-4 dark:border-slate-700 dark:bg-slate-950/30">
                    <div class="text-lg font-bold">Security Filter</div>
                    <div class="mt-1 text-sm text-muted-color">
                        Protezione runtime del Decisore basata su score e velocità.
                    </div>
                    <div class="mt-2 text-sm font-semibold" :class="isSecurityFilterEnabled() ? 'text-green-600' : 'text-red-500'">
                        {{ isSecurityFilterEnabled() ? 'ATTIVO' : 'SPENTO' }}
                    </div>
                    <div v-if="securityFilterStatus" class="mt-1 text-xs text-primary">{{ securityFilterStatus }}</div>
                    <div class="mt-3 flex flex-wrap gap-2">
                        <button
                            type="button"
                            class="rounded-lg bg-emerald-600 px-3 py-2 text-sm font-bold text-white hover:bg-emerald-700 disabled:opacity-60"
                            :disabled="securityFilterLoading || isSecurityFilterEnabled()"
                            @click="setSecurityFilterModule(true)"
                        >
                            ATTIVA
                        </button>
                        <button
                            type="button"
                            class="rounded-lg bg-red-600 px-3 py-2 text-sm font-bold text-white hover:bg-red-700 disabled:opacity-60"
                            :disabled="securityFilterLoading || !isSecurityFilterEnabled()"
                            @click="setSecurityFilterModule(false)"
                        >
                            SPEGNI
                        </button>
                    </div>
                    <div class="mt-4 text-sm font-semibold text-muted-color">Configurazione:</div>
                    <div class="mt-2 space-y-2 text-sm">
                        <label class="flex flex-wrap items-center gap-2 font-semibold">
                            <span>Max Avg Seconds</span>
                            <input
                                v-model.number="securityFilterMaxAvgDraft"
                                type="number"
                                min="0.01"
                                max="120"
                                step="0.01"
                                class="w-20 rounded-lg border border-slate-400 bg-white px-2 py-1 text-center font-bold dark:border-slate-600 dark:bg-surface-900"
                            />
                        </label>
                        <label class="flex flex-wrap items-center gap-2 font-semibold">
                            <span>Very Fast Seconds</span>
                            <input
                                v-model.number="securityFilterVeryFastDraft"
                                type="number"
                                min="0.01"
                                max="120"
                                step="0.01"
                                class="w-20 rounded-lg border border-slate-400 bg-white px-2 py-1 text-center font-bold dark:border-slate-600 dark:bg-surface-900"
                            />
                        </label>
                        <label class="flex flex-wrap items-center gap-2 font-semibold">
                            <span>Min Score</span>
                            <input
                                v-model.number="securityFilterMinScoreDraft"
                                type="number"
                                min="1"
                                max="4"
                                step="1"
                                class="w-14 rounded-lg border border-slate-400 bg-white px-2 py-1 text-center font-bold dark:border-slate-600 dark:bg-surface-900"
                            />
                        </label>
                    </div>
                    <div class="mt-3">
                        <button
                            type="button"
                            class="rounded-lg bg-slate-700 px-3 py-2 text-sm font-bold uppercase tracking-wide text-white hover:bg-slate-800 disabled:opacity-60"
                            :disabled="securityFilterLoading"
                            @click="saveSecurityFilterParameters"
                        >
                            SALVA
                        </button>
                    </div>
                </div>
                <div class="rounded-xl border border-amber-200 bg-amber-50/40 p-4 dark:border-amber-900 dark:bg-amber-950/20">
                    <div class="text-lg font-bold">Filtro 5</div>
                    <div class="mt-1 text-sm text-muted-color">Avviso Player Race a 5 PLAYER consecutivi.</div>
                    <div class="mt-2 text-sm font-semibold" :class="isPlayerRace5FilterEnabled() ? 'text-green-600' : 'text-red-500'">
                        {{ isPlayerRace5FilterEnabled() ? 'ATTIVO' : 'SPENTO' }}
                    </div>
                    <div v-if="playerRace5FilterStatus" class="mt-1 text-xs text-primary">{{ playerRace5FilterStatus }}</div>
                    <div class="mt-3 flex flex-wrap gap-2">
                        <button type="button" class="rounded-lg bg-emerald-600 px-3 py-2 text-sm font-bold text-white hover:bg-emerald-700 disabled:opacity-60" :disabled="playerRace5FilterLoading || isPlayerRace5FilterEnabled()" @click="setPlayerRace5Filter(true)">ATTIVA</button>
                        <button type="button" class="rounded-lg bg-red-600 px-3 py-2 text-sm font-bold text-white hover:bg-red-700 disabled:opacity-60" :disabled="playerRace5FilterLoading || !isPlayerRace5FilterEnabled()" @click="setPlayerRace5Filter(false)">SPEGNI</button>
                    </div>
                </div>
                <div class="rounded-xl border border-orange-200 bg-orange-50/40 p-4 dark:border-orange-900 dark:bg-orange-950/20">
                    <div class="text-lg font-bold">AC3 Filtro 5</div>
                    <div class="mt-1 text-sm text-muted-color">Intervento AC3 a 5 PLAYER (indipendente dall'avviso).</div>
                    <div class="mt-2 text-sm font-semibold" :class="isPlayerRace5Ac3Enabled() ? 'text-green-600' : 'text-red-500'">
                        {{ isPlayerRace5Ac3Enabled() ? 'ATTIVO' : 'SPENTO' }}
                    </div>
                    <div v-if="playerRace5Ac3Status" class="mt-1 text-xs text-primary">{{ playerRace5Ac3Status }}</div>
                    <div class="mt-3 flex flex-wrap gap-2">
                        <button type="button" class="rounded-lg bg-emerald-600 px-3 py-2 text-sm font-bold text-white hover:bg-emerald-700 disabled:opacity-60" :disabled="playerRace5Ac3Loading || isPlayerRace5Ac3Enabled()" @click="setPlayerRace5Ac3(true)">ATTIVA</button>
                        <button type="button" class="rounded-lg bg-red-600 px-3 py-2 text-sm font-bold text-white hover:bg-red-700 disabled:opacity-60" :disabled="playerRace5Ac3Loading || !isPlayerRace5Ac3Enabled()" @click="setPlayerRace5Ac3(false)">SPEGNI</button>
                    </div>
                </div>
                <div class="rounded-xl border border-blue-200 bg-blue-50/40 p-4 dark:border-blue-900 dark:bg-blue-950/20">
                    <div class="text-lg font-bold">Filtro 8</div>
                    <div class="mt-1 text-sm text-muted-color">Avviso Player Race a 8 PLAYER consecutivi.</div>
                    <div class="mt-2 text-sm font-semibold" :class="isPlayerRace8FilterEnabled() ? 'text-green-600' : 'text-red-500'">
                        {{ isPlayerRace8FilterEnabled() ? 'ATTIVO' : 'SPENTO' }}
                    </div>
                    <div v-if="playerRace8FilterStatus" class="mt-1 text-xs text-primary">{{ playerRace8FilterStatus }}</div>
                    <div class="mt-3 flex flex-wrap gap-2">
                        <button type="button" class="rounded-lg bg-emerald-600 px-3 py-2 text-sm font-bold text-white hover:bg-emerald-700 disabled:opacity-60" :disabled="playerRace8FilterLoading || isPlayerRace8FilterEnabled()" @click="setPlayerRace8Filter(true)">ATTIVA</button>
                        <button type="button" class="rounded-lg bg-red-600 px-3 py-2 text-sm font-bold text-white hover:bg-red-700 disabled:opacity-60" :disabled="playerRace8FilterLoading || !isPlayerRace8FilterEnabled()" @click="setPlayerRace8Filter(false)">SPEGNI</button>
                    </div>
                </div>
                <div class="rounded-xl border border-violet-200 bg-violet-50/40 p-4 dark:border-violet-900 dark:bg-violet-950/20">
                    <div class="text-lg font-bold">AC3 Filtro 8</div>
                    <div class="mt-1 text-sm text-muted-color">Intervento AC3 a 8 PLAYER (indipendente dall'avviso).</div>
                    <div class="mt-2 text-sm font-semibold" :class="isPlayerRace8Ac3Enabled() ? 'text-green-600' : 'text-red-500'">
                        {{ isPlayerRace8Ac3Enabled() ? 'ATTIVO' : 'SPENTO' }}
                    </div>
                    <div v-if="playerRace8Ac3Status" class="mt-1 text-xs text-primary">{{ playerRace8Ac3Status }}</div>
                    <div class="mt-3 flex flex-wrap gap-2">
                        <button type="button" class="rounded-lg bg-emerald-600 px-3 py-2 text-sm font-bold text-white hover:bg-emerald-700 disabled:opacity-60" :disabled="playerRace8Ac3Loading || isPlayerRace8Ac3Enabled()" @click="setPlayerRace8Ac3(true)">ATTIVA</button>
                        <button type="button" class="rounded-lg bg-red-600 px-3 py-2 text-sm font-bold text-white hover:bg-red-700 disabled:opacity-60" :disabled="playerRace8Ac3Loading || !isPlayerRace8Ac3Enabled()" @click="setPlayerRace8Ac3(false)">SPEGNI</button>
                    </div>
                </div>
                <div class="rounded-xl border border-cyan-200 bg-cyan-50/40 p-4 dark:border-cyan-900 dark:bg-cyan-950/20">
                    <div class="text-lg font-bold">SPOT L6 per bot</div>
                    <div class="mt-1 text-sm text-muted-color">
                        Concede L6 al singolo bot dopo N L5 perse nel suo ciclo SPOT.
                    </div>
                    <div class="mt-2 text-sm font-semibold" :class="isSpotL6PerBotEnabled() ? 'text-green-600' : 'text-red-500'">
                        {{ isSpotL6PerBotEnabled() ? 'ATTIVO' : 'SPENTO' }}
                    </div>
                    <div v-if="spotL6PerBotStatus" class="mt-1 text-xs text-primary">{{ spotL6PerBotStatus }}</div>
                    <div class="mt-3 flex flex-wrap gap-2">
                        <button
                            type="button"
                            class="rounded-lg bg-emerald-600 px-3 py-2 text-sm font-bold text-white hover:bg-emerald-700 disabled:opacity-60"
                            :disabled="spotL6PerBotLoading || isSpotL6PerBotEnabled()"
                            @click="setSpotL6PerBot(true)"
                        >
                            ATTIVA
                        </button>
                        <button
                            type="button"
                            class="rounded-lg bg-red-600 px-3 py-2 text-sm font-bold text-white hover:bg-red-700 disabled:opacity-60"
                            :disabled="spotL6PerBotLoading || !isSpotL6PerBotEnabled()"
                            @click="setSpotL6PerBot(false)"
                        >
                            SPEGNI
                        </button>
                    </div>
                    <div class="mt-4 flex flex-wrap items-center gap-2 text-sm font-semibold">
                        <span>Soglia L6 per bot:</span>
                        <input
                            v-model.number="spotResetThresholdDraft"
                            type="number"
                            min="1"
                            max="99"
                            class="w-14 rounded-lg border border-cyan-400 bg-white px-2 py-1 text-center font-bold dark:border-cyan-700 dark:bg-surface-900"
                        />
                    </div>
                    <div class="mt-2">
                        <button
                            type="button"
                            class="rounded-lg bg-cyan-700 px-3 py-2 text-sm font-bold uppercase tracking-wide text-white hover:bg-cyan-800 disabled:opacity-60"
                            :disabled="spotL6PerBotLoading"
                            @click="saveSpotL6ThresholdOnly"
                        >
                            Salva soglia
                        </button>
                    </div>
                </div>
            </div>

            <template v-if="securityFilterRows.length">
            <div class="mb-4 flex flex-col gap-3 lg:flex-row lg:items-end lg:justify-between">
                <div>
                    <span class="block text-muted-color font-medium">Control Room — sequenza PLAYER</span>
                    <div class="mt-1 text-sm text-muted-color">Sempre P1–P8. Filtro 5/8 = avviso; AC3 Filtro 5/8 = intervento.</div>
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

                    <div class="mb-3">
                        <div class="mb-1 text-[10px] font-semibold uppercase tracking-wide text-blue-600 dark:text-blue-300">P1 – P8</div>
                        <div class="flex flex-wrap items-center gap-1">
                            <span
                                v-for="step in getPlayerStreakSteps(row)"
                                :key="`${getBotName(row)}-${step.label}`"
                                class="inline-flex h-7 w-7 items-center justify-center rounded-md text-[10px] font-bold transition-transform"
                                :class="getPlayerStepClassCompact(step, row)"
                            >
                                {{ step.label }}
                            </span>
                        </div>
                        <div v-if="getPlayerRaceProgressLines(row).length" class="mt-1 space-y-0.5">
                            <div
                                v-for="line in getPlayerRaceProgressLines(row)"
                                :key="`${getBotName(row)}-${line.key}`"
                                :class="line.class"
                            >
                                {{ line.label }}
                            </div>
                        </div>
                        <div class="mt-1 space-y-0.5">
                            <div class="text-[10px] font-bold uppercase tracking-wide text-muted-color">
                                STATO BOT:
                                <span :class="getRealBotStateClass(row)">{{ getRealBotStateLabel(row) }}</span>
                            </div>
                            <div class="text-[10px] font-bold uppercase tracking-wide text-muted-color">
                                STATO FILTRI:
                                <span :class="getFilterStateClass(row)">{{ getFilterStateLabel(row) }}</span>
                            </div>
                        </div>
                    </div>

                    <div class="flex items-center justify-between gap-3">
                        <span class="text-xs font-bold" :class="getTriggerClass(row)">{{ getTriggerLabel(row) }}</span>
                        <span class="rounded-full border px-2.5 py-1 text-xs font-bold" :class="getRiskStatusClass(row)">{{ getRiskLabel(row) }}</span>
                    </div>
                </button>
            </div>
            </template>

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

                    <div class="mb-4 rounded-xl border border-surface-200 bg-surface-0 p-4 dark:border-surface-700 dark:bg-surface-900">
                        <div class="mb-2 text-xs font-bold uppercase tracking-wide text-muted-color">Stato bot e filtri</div>
                        <div class="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
                            <div>
                                <div class="text-xl font-bold" :class="getRealBotStateClass(selectedSecurityFilterRow)">
                                    STATO BOT: {{ getRealBotStateLabel(selectedSecurityFilterRow) }}
                                </div>
                                <div class="mt-1 text-lg font-bold" :class="getFilterStateClass(selectedSecurityFilterRow)">
                                    STATO FILTRI: {{ getFilterStateLabel(selectedSecurityFilterRow) }}
                                </div>
                                <div class="mt-1 text-sm text-muted-color">
                                    Martingala L{{ selectedSecurityFilterRow.Martingala ?? '-' }}
                                    <span v-if="selectedSecurityFilterRow.LastReason || selectedSecurityFilterRow.lastReason">
                                        · {{ selectedSecurityFilterRow.LastReason || selectedSecurityFilterRow.lastReason }}
                                    </span>
                                </div>
                            </div>
                            <div class="flex flex-col items-start gap-2 md:items-end">
                                <div class="flex flex-wrap gap-2">
                                    <button
                                        type="button"
                                        class="rounded-lg bg-emerald-700 px-4 py-2 text-sm font-bold uppercase tracking-wide text-white hover:bg-emerald-800 disabled:opacity-60"
                                        :disabled="isOperatorCommandLoading(selectedSecurityFilterRow, 'continue')"
                                        @click="continueBot(selectedSecurityFilterRow)"
                                    >
                                        CONTINUA
                                    </button>
                                    <button
                                        type="button"
                                        class="rounded-lg bg-slate-800 px-4 py-2 text-sm font-bold uppercase tracking-wide text-white hover:bg-slate-900 disabled:opacity-60 dark:bg-slate-200 dark:text-slate-900 dark:hover:bg-white"
                                        :disabled="isOperatorCommandLoading(selectedSecurityFilterRow, 'reset')"
                                        @click="resetBotMartingale(selectedSecurityFilterRow)"
                                    >
                                        AZZERA MARTINGALA
                                    </button>
                                </div>
                                <div v-if="operatorCommandStatus" class="max-w-md text-xs text-primary">{{ operatorCommandStatus }}</div>
                            </div>
                        </div>
                    </div>

                    <div class="mb-4 rounded-xl border p-4" :class="getPlayerPacePanelClass(selectedSecurityFilterRow)">
                        <div class="mb-3 text-lg font-bold text-surface-900 dark:text-surface-0">Sequenza PLAYER (P1 – P8)</div>

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

                            <div class="mt-4 space-y-2 text-center">
                                <div class="text-sm font-semibold text-muted-color">
                                    Timeline P1–P8 · streak {{ getPlayerPaceVisual(selectedSecurityFilterRow).count }}/{{ PLAYER_BLOCK_COUNT }}
                                </div>
                                <div
                                    v-if="getPlayerRaceProgressLines(selectedSecurityFilterRow).length"
                                    class="space-y-1"
                                >
                                    <div
                                        v-for="line in getPlayerRaceProgressLines(selectedSecurityFilterRow)"
                                        :key="`detail-${line.key}`"
                                        class="text-lg font-bold"
                                        :class="line.class"
                                    >
                                        {{ line.label }}
                                    </div>
                                </div>
                                <div v-else class="text-lg font-bold text-muted-color">
                                    Nessun filtro PLAYER attivo
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
