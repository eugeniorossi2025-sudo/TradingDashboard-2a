<script setup>
import MobilePushPanel from '@/components/mobile/MobilePushPanel.vue';
import { AuthService } from '@/service/AuthService';
import { DashboardService } from '@/service/DashboardService';
import { FinancialReportService } from '@/service/FinancialReportService';
import { useMobileLiveRefresh } from '@/composables/useMobileLiveRefresh';
import { useOpenMissionHero } from '@/composables/useOpenMissionHero';
import { REPORT_PERIOD_CHIPS, useReportPeriod } from '@/composables/useReportPeriod';
import { formatRomeTime } from '@/utils/romeTime';
import { computed, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';

const router = useRouter();
const loading = ref(true);
const error = ref('');
const tableRows = ref([]);
const chartRows = ref([]);
const runtimeMode = ref({ runtimeMode: 'Production', isDemoMode: false });
const productionReport = ref(null);
const demoReport = ref(null);
const lastSync = ref('');

const { periodChip, from, to, demoFrom, demoTo, applyPeriodChip, formatPeriod, formatDemoPeriod } = useReportPeriod('month');
const periodChips = REPORT_PERIOD_CHIPS;

const demoPeriodResult = computed(() => Number(demoReport.value?.totals?.periodResultEuro ?? demoReport.value?.totals?.totalMarginEuro ?? 0));
const productionPeriodResult = computed(() => Number(productionReport.value?.totals?.periodResultEuro ?? productionReport.value?.totals?.totalMarginEuro ?? 0));

const liveMargin = computed(() => tableRows.value.reduce((sum, row) => sum + getNumber(row, 'margine', 'Margine'), 0));
const {
    loadCurrentMission,
    hasOpenMission,
    heroSessionId,
    heroMargin,
    stopWinEuro,
    missionTarget,
    heroProgress,
    heroProgressClamped,
    chartTarget,
    heroNote,
    heroProgressLabel
} = useOpenMissionHero(liveMargin);

function formatPeriodMeta(report) {
    if (!report?.totals) return '—';
    const sessions = report.totals.sessionCount ?? 0;
    const samples = report.totals.sampleCount ?? 0;
    const days = report.totals.workingDays;
    const daysPart = days != null && days > 0 ? ` · ${days} gg lavorativi` : '';
    return `${sessions} missioni · ${samples} campioni${daysPart}`;
}
const activeTables = computed(() => tableRows.value.length);
const chartSamples = computed(() => {
    if (chartRows.value.length) return chartRows.value;
    return tableRows.value.map((row, index) => ({
        timestamp: row.timestamp || row.dateTime || new Date(Date.now() - (tableRows.value.length - index) * 60000).toISOString(),
        margine: getNumber(row, 'margine', 'Margine')
    }));
});
const chartPath = computed(() => buildPath(chartSamples.value, (row) => getNumber(row, 'margine', 'Margine', 'margin')));
const targetPath = computed(() => buildPath(chartSamples.value, () => chartTarget.value));

function getNumber(row, ...keys) {
    for (const key of keys) {
        const value = row?.[key];
        const number = Number(value);
        if (Number.isFinite(number)) return number;
    }
    return 0;
}

function toneClass(value) {
    const number = Number(value || 0);
    if (number > 0.01) return 'pos';
    if (number < -0.01) return 'neg';
    return 'neutral';
}

function formatMoney(value) {
    const number = Number(value || 0);
    const abs = new Intl.NumberFormat('it-IT', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(Math.abs(number));
    const sign = number > 0 ? '+' : number < 0 ? '-' : '';
    return `€ ${sign}${abs}`;
}

function formatPercent(value) {
    return `${new Intl.NumberFormat('it-IT', { minimumFractionDigits: 1, maximumFractionDigits: 1 }).format(Number(value || 0))}%`;
}

function formatPeriodRange() {
    return formatPeriod();
}

async function setReportPeriod(chip) {
    applyPeriodChip(chip);
    await loadReports();
}

async function loadReports() {
    productionReport.value = await FinancialReportService.getRangeReport('Production', from.value, to.value);
    demoReport.value = await FinancialReportService.getRangeReport('Demo', demoFrom.value, demoTo.value);
}

/** Live margin: dashboard + chart + open mission (no full-page loading). */
async function refreshLive() {
    const [dashboard, chart] = await Promise.all([DashboardService.getDashboardData(), DashboardService.getChartData()]);
    tableRows.value = Array.isArray(dashboard) ? dashboard : dashboard?.rows || dashboard?.tables || [];
    chartRows.value = Array.isArray(chart) ? chart : [];
    await loadCurrentMission();
    lastSync.value = formatRomeTime();
}

useMobileLiveRefresh({
    onRefreshLive: refreshLive,
    onRefreshReports: loadReports,
    liveIntervalMs: 5000,
    reportIntervalMs: 60_000
});

function buildPath(rows, selector) {
    const values = rows.map(selector).map(Number).filter(Number.isFinite);
    if (!values.length) return '';
    const min = Math.min(0, ...values);
    const max = Math.max(1, ...values);
    const span = Math.max(1, max - min);
    const left = 10;
    const right = 350;
    const top = 18;
    const bottom = 130;
    const maxIndex = Math.max(1, values.length - 1);
    return values
        .map((value, index) => {
            const x = left + (index / maxIndex) * (right - left);
            const y = bottom - ((value - min) / span) * (bottom - top);
            return `${index === 0 ? 'M' : 'L'}${x.toFixed(1)} ${y.toFixed(1)}`;
        })
        .join(' ');
}

async function loadData() {
    loading.value = true;
    error.value = '';
    try {
        runtimeMode.value = await FinancialReportService.getRuntimeMode();
        await refreshLive();
        await loadReports();
    } catch (err) {
        console.error('Client mobile load error:', err);
        error.value = 'Dati reali non disponibili in questo momento.';
    } finally {
        loading.value = false;
    }
}

async function logout() {
    await AuthService.logout();
    router.push('/auth/login');
}

onMounted(loadData);
</script>

<template>
    <main class="mobile-page">
        <section class="shell">
            <header class="topbar">
                <div>
                    <div class="eyebrow">Client mobile</div>
                    <h1>EuGenio Live</h1>
                    <div class="mode-badge" :class="{ demo: runtimeMode?.isDemoMode }">
                        {{ runtimeMode?.isDemoMode ? 'DEMO' : 'PRODUZIONE' }}
                    </div>
                </div>
                <div class="actions">
                    <button type="button" class="link-btn" @click="router.push('/client/desktop')">Vista desktop</button>
                    <button type="button" class="link-btn" @click="loadData">Sync</button>
                    <button type="button" class="logout-btn" @click="logout">Logout</button>
                </div>
            </header>

            <div v-if="error" class="error-banner">{{ error }}</div>

            <section class="panel hero-client">
                <div class="eyebrow">Operatività live</div>
                <div class="hero-kpi">
                    <div class="hero-kpi-item">
                        <div class="hero-kpi-label">Margine live</div>
                        <div class="hero-value numeric-stable" :class="toneClass(heroMargin)" dir="ltr">
                            {{ formatMoney(heroMargin) }}
                        </div>
                    </div>
                    <div class="hero-kpi-item">
                        <div class="hero-kpi-label">Stop Win attuale</div>
                        <div class="hero-value numeric-stable" dir="ltr">{{ formatMoney(stopWinEuro) }}</div>
                    </div>
                    <div class="hero-kpi-item">
                        <div class="hero-kpi-label">Progresso missione</div>
                        <div class="hero-value numeric-stable" dir="ltr">{{ hasOpenMission && missionTarget > 0 ? formatPercent(heroProgress) : '—' }}</div>
                    </div>
                </div>
                <div class="subline numeric-stable" dir="ltr">
                    <template v-if="hasOpenMission">Missione #{{ heroSessionId }} · {{ heroProgressLabel }}</template>
                    <template v-else>{{ heroNote }}</template>
                </div>
                <div v-if="hasOpenMission && missionTarget > 0" class="progress-wrap">
                    <div class="progress-bar" :style="{ width: `${heroProgressClamped}%` }"></div>
                </div>
            </section>

            <section class="grid">
                <div class="panel stat">
                    <div class="eyebrow">Sessione</div>
                    <div class="stat-value">{{ hasOpenMission ? `#${heroSessionId}` : '—' }}</div>
                    <div class="subline">{{ activeTables }} tavoli live</div>
                </div>
                <div class="panel stat">
                    <div class="eyebrow">Campioni curva</div>
                    <div class="stat-value">{{ chartSamples.length }}</div>
                    <div class="subline">Mission chart live</div>
                </div>
            </section>

            <section class="panel">
                <div class="eyebrow">Mission chart</div>
                <svg class="chart" viewBox="0 0 360 150" role="img" aria-label="Client mission margin chart">
                    <line class="axis" x1="10" y1="130" x2="350" y2="130"></line>
                    <path class="target-line" :d="targetPath"></path>
                    <path class="chart-line" :class="toneClass(heroMargin)" :d="chartPath"></path>
                </svg>
                <div class="subline">{{ chartSamples.length > 0 ? `${chartSamples.length} campioni missione` : 'Nessun campione disponibile' }}</div>
            </section>

            <section class="panel">
                <div class="month-report-head">
                    <div>
                        <div class="eyebrow">Report periodo</div>
                        <div class="subline">Contabilità storica · Production {{ formatPeriodRange() }} · Demo {{ formatDemoPeriod() }}</div>
                    </div>
                </div>
                <div class="period-chips">
                    <button
                        v-for="chip in periodChips"
                        :key="chip.id"
                        type="button"
                        class="period-chip"
                        :class="{ active: periodChip === chip.id }"
                        @click="setReportPeriod(chip.id)"
                    >
                        {{ chip.label }}
                    </button>
                </div>
                <div class="month-report-grid">
                    <article class="month-report-card">
                        <div class="month-report-label">Production</div>
                        <div class="month-report-value" :class="toneClass(productionPeriodResult)">
                            {{ formatMoney(productionPeriodResult) }}
                        </div>
                        <div class="month-report-meta">
                            {{ formatPeriodMeta(productionReport) }}<br />
                            esclude missione aperta
                        </div>
                    </article>
                    <article class="month-report-card">
                        <div class="month-report-label">Demo</div>
                        <div class="month-report-value" :class="toneClass(demoPeriodResult)">
                            {{ formatMoney(demoPeriodResult) }}
                        </div>
                        <div class="month-report-meta">
                            {{ formatPeriodMeta(demoReport) }}<br />
                            demo · esclude missione aperta
                        </div>
                    </article>
                </div>
                <div class="month-report-foot">Updated {{ lastSync || '--' }} · live data only</div>
            </section>

            <section class="panel">
                <div class="month-report-head">
                    <div>
                        <div class="eyebrow">Bot / Tavoli</div>
                        <div class="subline">Snapshot live cliente</div>
                    </div>
                    <div class="month-report-period">{{ activeTables }} bot</div>
                </div>
                <div class="watchlist">
                    <div v-if="loading" class="empty">Caricamento dati reali...</div>
                    <div v-else-if="!tableRows.length" class="empty">Nessun tavolo attivo.</div>
                    <article v-else v-for="row in tableRows.slice(0, 8)" :key="`${row.account || row.Account}-${row.tavolo || row.Tavolo}`" class="watch-item">
                        <div class="watch-top">
                            <div>
                                <div class="watch-name">{{ row.account || row.Account || 'Account' }} · Tavolo {{ row.tavolo || row.Tavolo || '-' }}</div>
                                <div class="watch-detail">{{ row.stato || row.Stato || 'Stato non disponibile' }}</div>
                            </div>
                            <div class="watch-level">{{ formatMoney(row.margine ?? row.Margine) }}</div>
                        </div>
                    </article>
                </div>
            </section>

            <MobilePushPanel
                return-path="/client/mobile"
                title="Notifiche push"
                description="Ricevi avvisi missione anche da mobile cliente. Consigliato Chrome Android e notifica di prova."
            />

            <div class="status">Sincronizzazione {{ lastSync || 'in corso' }}</div>
        </section>
    </main>
</template>

<style scoped>
.mobile-page {
    min-height: 100vh;
    padding: 16px 14px 28px;
    color: var(--text-color);
    background:
        radial-gradient(circle at 10% 0%, color-mix(in srgb, var(--primary-color) 24%, transparent), transparent 24%), radial-gradient(circle at 90% 0%, rgba(245, 158, 11, 0.14), transparent 26%),
        linear-gradient(180deg, var(--surface-ground) 0%, var(--surface-card) 46%, var(--surface-ground) 100%);
}
.shell {
    max-width: 460px;
    margin: 0 auto;
    display: flex;
    flex-direction: column;
    gap: 14px;
}
.topbar,
.month-report-head,
.watch-top {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    gap: 12px;
}
.eyebrow,
.month-report-label,
.month-report-period {
    color: var(--text-color-secondary);
    font-size: 11px;
    letter-spacing: 0.16em;
    text-transform: uppercase;
}
h1 {
    margin: 4px 0 0;
    font-size: 25px;
    letter-spacing: -0.04em;
}
.mode-badge {
    display: inline-flex;
    margin-top: 8px;
    padding: 6px 10px;
    border-radius: 999px;
    border: 1px solid color-mix(in srgb, var(--primary-color) 40%, transparent);
    background: color-mix(in srgb, var(--primary-color) 14%, transparent);
    color: var(--primary-color);
    font-size: 10px;
    font-weight: 800;
    letter-spacing: 0.14em;
}
.mode-badge.demo {
    color: #f59e0b;
    border-color: rgba(245, 158, 11, 0.42);
    background: rgba(245, 158, 11, 0.11);
}
.actions {
    display: flex;
    gap: 8px;
    flex-wrap: wrap;
    justify-content: flex-end;
}
.link-btn,
.logout-btn {
    min-height: 34px;
    padding: 8px 11px;
    border-radius: 999px;
    border: 1px solid var(--surface-border);
    background: color-mix(in srgb, var(--surface-card) 80%, transparent);
    color: var(--text-color);
    font: inherit;
    font-size: 12px;
    cursor: pointer;
}
.panel {
    border: 1px solid var(--surface-border);
    border-radius: 24px;
    background: linear-gradient(180deg, rgba(255, 255, 255, 0.05), rgba(255, 255, 255, 0.015)), var(--surface-card);
    box-shadow: 0 24px 70px rgba(0, 0, 0, 0.22);
    padding: 18px;
    overflow: hidden;
}
.hero-kpi {
    display: grid;
    grid-template-columns: 1fr;
    gap: 0.85rem;
    margin: 0.75rem 0 0.35rem;
}

.hero-kpi-label {
    color: var(--text-color-secondary);
    font-size: 11px;
    letter-spacing: 0.14em;
    text-transform: uppercase;
}

.hero-kpi-item .hero-value {
    margin-top: 4px;
    font-size: clamp(28px, 8vw, 40px);
}

.hero-value {
    margin-top: 8px;
    font-size: clamp(38px, 11vw, 54px);
    font-weight: 800;
    letter-spacing: -0.08em;
}
.pos {
    color: #22c55e;
}
.neg {
    color: #ef4444;
}
.neutral {
    color: var(--text-color);
}
.numeric-stable {
    direction: ltr;
    unicode-bidi: isolate;
    font-variant-numeric: tabular-nums;
}
.subline,
.status,
.month-report-meta,
.month-report-foot,
.watch-detail,
.empty {
    color: var(--text-color-secondary);
    font-size: 12px;
    line-height: 1.4;
}
.progress-wrap {
    margin-top: 16px;
    height: 10px;
    border-radius: 999px;
    background: rgba(255, 255, 255, 0.08);
    overflow: hidden;
}
.progress-bar {
    height: 100%;
    background: linear-gradient(90deg, var(--primary-color), #f59e0b);
    border-radius: inherit;
    transition: width 0.25s ease;
}
.grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 12px;
}
.stat {
    min-height: 96px;
}
.stat-value {
    margin-top: 8px;
    font-size: 24px;
    font-weight: 800;
    letter-spacing: -0.05em;
}
.chart {
    height: 150px;
    width: 100%;
}
.chart-line {
    fill: none;
    stroke: var(--primary-color);
    stroke-width: 3;
    stroke-linecap: round;
    stroke-linejoin: round;
}
.chart-line.neg {
    stroke: #ef4444;
}
.chart-line.neutral {
    stroke: var(--text-color-secondary);
}
.target-line {
    fill: none;
    stroke: #f59e0b;
    stroke-width: 2;
    stroke-dasharray: 7 7;
    opacity: 0.75;
}
.axis {
    stroke: var(--surface-border);
    stroke-width: 1;
}
.month-report-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 10px;
    margin-top: 12px;
}
.period-chips {
    display: flex;
    flex-wrap: wrap;
    gap: 8px;
}
.period-chip {
    min-height: 34px;
    padding: 8px 12px;
    border-radius: 999px;
    border: 1px solid var(--surface-border);
    background: rgba(255, 255, 255, 0.04);
    color: var(--text-color-secondary);
    font: inherit;
    font-size: 12px;
    font-weight: 700;
    letter-spacing: 0.04em;
    cursor: pointer;
}
.period-chip.active {
    color: var(--primary-color);
    border-color: color-mix(in srgb, var(--primary-color) 45%, transparent);
    background: color-mix(in srgb, var(--primary-color) 14%, transparent);
}
.month-report-card,
.watch-item,
.empty {
    padding: 12px;
    border-radius: 18px;
    background: rgba(255, 255, 255, 0.04);
    border: 1px solid rgba(255, 255, 255, 0.06);
}
.month-report-value {
    margin-top: 7px;
    font-size: 22px;
    font-weight: 800;
    letter-spacing: -0.05em;
}
.watchlist {
    display: flex;
    flex-direction: column;
    gap: 10px;
}
.watch-name {
    font-size: 14px;
    font-weight: 700;
}
.watch-level {
    min-width: 74px;
    padding: 7px 10px;
    border-radius: 999px;
    text-align: center;
    background: color-mix(in srgb, var(--primary-color) 12%, transparent);
    color: var(--primary-color);
    font-size: 11px;
    font-weight: 700;
}
.error-banner {
    padding: 12px 14px;
    border-radius: 18px;
    color: #fecaca;
    background: rgba(239, 68, 68, 0.12);
    border: 1px solid rgba(239, 68, 68, 0.28);
}
.status {
    text-align: center;
}
</style>
