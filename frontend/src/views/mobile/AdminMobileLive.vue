<script setup>
import { AuthService } from '@/service/AuthService';
import { DashboardService } from '@/service/DashboardService';
import { FinancialReportService } from '@/service/FinancialReportService';
import { PushNotificationService } from '@/service/PushNotificationService';
import { REPORT_PERIOD_CHIPS, useReportPeriod } from '@/composables/useReportPeriod';
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
const pushLoading = ref(false);
const pushStatus = ref({
    supported: false,
    permission: 'unsupported',
    configured: false,
    subscribed: false,
    message: 'Verifica notifiche in corso.'
});

const { periodChip, from, to, demoFrom, demoTo, applyPeriodChip, formatPeriod, formatDemoPeriod } = useReportPeriod('month');
const periodChips = REPORT_PERIOD_CHIPS;

const demoPeriodResult = computed(() => Number(demoReport.value?.totals?.periodResultEuro ?? demoReport.value?.totals?.totalMarginEuro ?? 0));
const productionPeriodResult = computed(() => Number(productionReport.value?.totals?.periodResultEuro ?? productionReport.value?.totals?.totalMarginEuro ?? 0));

const globalMargin = computed(() => tableRows.value.reduce((sum, row) => sum + getNumber(row, 'margine', 'Margine'), 0));
const target = computed(() => Number(productionReport.value?.totals?.globalTargetEuro || 0));
const remaining = computed(() => Math.max(0, target.value - globalMargin.value));
const progress = computed(() => (target.value > 0 ? (globalMargin.value / target.value) * 100 : Number(productionReport.value?.totals?.progressPct || 0)));
const progressClamped = computed(() => Math.max(0, Math.min(100, progress.value)));
const activeTables = computed(() => tableRows.value.length);
const pausedTables = computed(
    () =>
        tableRows.value.filter((row) =>
            String(row.stato ?? row.Stato ?? '')
                .toLowerCase()
                .includes('pause')
        ).length
);
const highRiskRows = computed(() => [...tableRows.value].sort((a, b) => getNumber(b, 'colpoMartingala', 'ColpoMartingala', 'levelIndex') - getNumber(a, 'colpoMartingala', 'ColpoMartingala', 'levelIndex')).slice(0, 8));
const chartSamples = computed(() =>
    chartRows.value.length ? chartRows.value : tableRows.value.map((row, index) => ({ timestamp: row.timestamp || new Date(Date.now() - (tableRows.value.length - index) * 60000).toISOString(), margine: getNumber(row, 'margine', 'Margine') }))
);
const chartPath = computed(() => buildPath(chartSamples.value, (row) => getNumber(row, 'margine', 'Margine', 'margin')));
const targetPath = computed(() => buildPath(chartSamples.value, () => target.value));
const strategyLabel = computed(() => {
    const row = tableRows.value.find((item) => buildStrategyLabel(item) !== '--');
    return row ? buildStrategyLabel(row) : '--';
});
const sessionPulse = computed(() => (activeTables.value > 0 ? 'LIVE' : 'IDLE'));
const copilotEvents = computed(() => tableRows.value.filter((row) => buildStrategyLabel(row) !== '--').slice(0, 6));
const pushStateLabel = computed(() => {
    if (pushStatus.value.subscribed) return 'Push attive';
    if (pushStatus.value.permission === 'granted' && !pushStatus.value.configured) return 'Permesso OK, backend non pronto';
    if (pushStatus.value.permission === 'granted') return 'Permesso OK, subscription mancante';
    if (pushStatus.value.configured) return 'Backend pronto';
    return 'Push non attive';
});

function getNumber(row, ...keys) {
    for (const key of keys) {
        const number = Number(row?.[key]);
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

function getField(row, ...keys) {
    for (const key of keys) {
        const value = row?.[key];
        if (value !== undefined && value !== null) return value;
    }
    return null;
}

function getText(row, ...keys) {
    const value = getField(row, ...keys);
    if (typeof value === 'string') {
        const text = value.trim();
        if (!text || text === '{}' || text === '[]' || text.toLowerCase() === 'null') return '';
        return text;
    }
    if (typeof value === 'number' || typeof value === 'boolean') return String(value);
    if (value && typeof value === 'object' && Object.keys(value).length > 0) return JSON.stringify(value);
    return '';
}

function buildStrategyLabel(row) {
    const directText = getText(row, 'valutazione', 'Valutazione', 'reason', 'Reason', 'lastInfo', 'LastInfo', 'prediction', 'Prediction');
    if (directText) return directText;

    const actionCode = getField(row, 'adviceActionCode', 'AdviceActionCode');
    const martingala = getField(row, 'adviceMartingala', 'AdviceMartingala', 'colpoMartingala', 'ColpoMartingala', 'levelIndex', 'LevelIndex');
    const hotZoneLabel = getText(row, 'adviceHotZoneLabel', 'AdviceHotZoneLabel');
    const state = getText(row, 'stato', 'Stato');

    const parts = [];
    if (actionCode !== null) parts.push(`Action ${actionCode}`);
    if (martingala !== null) parts.push(`L${martingala}`);
    if (hotZoneLabel) parts.push(hotZoneLabel);
    if (state) parts.push(state);

    return parts.length ? parts.join(' · ') : '--';
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
    lastSync.value = new Date().toLocaleTimeString('it-IT', { hour: '2-digit', minute: '2-digit' });
}

function buildPath(rows, selector) {
    const values = rows.map(selector).map(Number).filter(Number.isFinite);
    if (!values.length) return '';
    const min = Math.min(0, ...values);
    const max = Math.max(1, ...values);
    const span = Math.max(1, max - min);
    const left = 10;
    const right = 350;
    const top = 18;
    const bottom = 160;
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
        const [dashboard, chart] = await Promise.all([DashboardService.getDashboardData(), DashboardService.getMarginiChart()]);
        tableRows.value = Array.isArray(dashboard) ? dashboard : dashboard?.rows || dashboard?.tables || [];
        chartRows.value = Array.isArray(chart) ? chart : [];

        runtimeMode.value = await FinancialReportService.getRuntimeMode();
        await loadReports();
    } catch (err) {
        console.error('Admin mobile load error:', err);
        error.value = 'Dati reali non disponibili in questo momento.';
    } finally {
        loading.value = false;
    }
}

async function loadPushStatus() {
    pushLoading.value = true;
    try {
        pushStatus.value = await PushNotificationService.getStatus();
    } finally {
        pushLoading.value = false;
    }
}

async function enablePush() {
    pushLoading.value = true;
    try {
        pushStatus.value = await PushNotificationService.subscribe();
    } catch (err) {
        console.warn('Push subscription failed:', err);
        pushStatus.value = {
            supported: pushStatus.value.supported,
            permission: typeof Notification !== 'undefined' ? Notification.permission : 'unsupported',
            configured: false,
            subscribed: false,
            message: 'Notifiche push non attivabili: endpoint backend non disponibile.'
        };
    } finally {
        pushLoading.value = false;
    }
}

function downloadReport(runtimeMode) {
    const rangeFrom = runtimeMode === 'Demo' ? demoFrom.value : from.value;
    const rangeTo = runtimeMode === 'Demo' ? demoTo.value : to.value;
    return FinancialReportService.openHtmlReport(runtimeMode, rangeFrom, rangeTo);
}

function openFinancialReports() {
    router.push('/admin/mobile-reports');
}

async function logout() {
    await AuthService.logout();
    router.push('/auth/login');
}

onMounted(() => {
    loadData();
    loadPushStatus();
});
</script>

<template>
    <main class="mobile-page">
        <section class="shell">
            <section class="intro">
                <div class="intro-head">
                    <div class="intro-copywrap">
                        <div class="intro-kicker">Admin mobile live</div>
                        <div class="intro-title">Mission control on phone</div>
                        <div class="intro-copy">Superficie mobile read-only su dashboard, margini, report e stream bot reali.</div>
                    </div>
                    <div class="status-pill">{{ loading ? 'Sync booting' : 'Sync live' }}</div>
                </div>
                <div class="brand-signature">
                    <div class="brand-signature-mark">D2A</div>
                    <div class="brand-signature-copy">EuGenio Lab<br />Trading Dashboard 2A</div>
                </div>
                <div class="actions">
                    <button type="button" class="link-btn" @click="loadData">Sync</button>
                    <button type="button" class="link-btn" @click="openFinancialReports">Report finanziari</button>
                    <button type="button" class="logout-btn" @click="logout">Logout</button>
                </div>
            </section>

            <div v-if="error" class="error-banner">{{ error }}</div>

            <section class="panel hero">
                <div class="hero-top">
                    <div>
                        <div class="eyebrow">Mission objective</div>
                        <div class="mega-label">Live target</div>
                        <div class="mega-value" :class="toneClass(globalMargin)">{{ formatMoney(globalMargin) }}</div>
                        <div class="hero-note">{{ activeTables > 0 ? 'Dashboard live data available' : 'No live bots available yet' }}</div>
                    </div>
                    <div class="goal-orb">
                        <div class="mini-label">Remaining</div>
                        <div class="goal-orb-v">{{ formatMoney(remaining) }}</div>
                        <div class="bar"><span :style="{ width: `${progressClamped}%` }"></span></div>
                    </div>
                </div>

                <div class="hero-grid">
                    <div class="stat-box">
                        <div class="mini-label">Global margin</div>
                        <div class="stat-v" :class="toneClass(globalMargin)">{{ formatMoney(globalMargin) }}</div>
                        <div class="stat-sub">{{ activeTables }} tavoli live</div>
                    </div>
                    <div class="stat-box">
                        <div class="mini-label">Live strategy</div>
                        <div class="stat-v small">{{ strategyLabel }}</div>
                        <div class="stat-sub">Da payload reale</div>
                    </div>
                    <div class="stat-box">
                        <div class="mini-label">Session pulse</div>
                        <div class="stat-v">{{ sessionPulse }}</div>
                        <div class="stat-sub">{{ pausedTables }} in pausa</div>
                    </div>
                </div>
            </section>

            <section class="panel section">
                <div class="section-head">
                    <div>
                        <div class="section-title">Mission curve</div>
                        <div class="section-copy">Storico margine missione contro target</div>
                    </div>
                    <div class="mini-label">{{ chartSamples.length }} samples</div>
                </div>
                <div class="chart-wrap">
                    <svg class="mission-chart" viewBox="0 0 360 180" preserveAspectRatio="none">
                        <line class="axis" x1="10" y1="160" x2="350" y2="160"></line>
                        <path class="target-line" :d="targetPath"></path>
                        <path class="curve-line" :class="toneClass(globalMargin)" :d="chartPath"></path>
                    </svg>
                    <div class="chart-meta">
                        <span class="chart-current">Current {{ formatMoney(globalMargin) }}</span>
                        <span class="chart-target">Target {{ formatMoney(target) }}</span>
                    </div>
                </div>
            </section>

            <section class="panel section">
                <div class="section-head">
                    <div>
                        <div class="section-title">Level watchlist</div>
                        <div class="section-copy">Top risk tables, martingala visibility</div>
                    </div>
                    <div class="mini-label">{{ highRiskRows.length }} bots</div>
                </div>
                <div class="watchlist">
                    <div v-if="loading" class="empty">Loading live bots...</div>
                    <div v-else-if="!highRiskRows.length" class="empty">No live bots available yet.</div>
                    <article v-else v-for="row in highRiskRows" :key="`${row.account || row.Account}-${row.tavolo || row.Tavolo}`" class="watch-item" :class="{ critical: getNumber(row, 'colpoMartingala', 'ColpoMartingala', 'levelIndex') >= 5 }">
                        <div class="watch-top">
                            <div>
                                <div class="watch-name">{{ row.account || row.Account || 'Account' }} · Table {{ row.tavolo || row.Tavolo || '-' }}</div>
                                <div class="watch-detail">{{ buildStrategyLabel(row) }}</div>
                            </div>
                            <div class="watch-level">L{{ getNumber(row, 'colpoMartingala', 'ColpoMartingala', 'levelIndex') }}</div>
                        </div>
                        <div class="watch-meta">
                            <span class="chip" :class="toneClass(row.margine ?? row.Margine)">{{ formatMoney(row.margine ?? row.Margine) }}</span>
                            <span class="chip">Deck {{ row.mazzo || row.Mazzo || '-' }}</span>
                            <span class="chip">{{ row.stato || row.Stato || 'state n/a' }}</span>
                        </div>
                    </article>
                </div>
            </section>

            <section class="panel section">
                <div class="section-head">
                    <div>
                        <div class="section-title">Mission copilot</div>
                        <div class="section-copy">Decisioni server e notifiche live disponibili</div>
                    </div>
                    <div class="mini-label">{{ copilotEvents.length }} events</div>
                </div>
                <div class="assistant-feed">
                    <div v-if="!copilotEvents.length" class="empty">No server decision has been captured yet.</div>
                    <article v-else v-for="row in copilotEvents" :key="`${row.account || row.Account}-${row.tavolo || row.Tavolo}-copilot`" class="assistant-item">
                        <div class="assistant-top">
                            <div class="assistant-title">{{ row.account || row.Account || 'Bot' }} · {{ buildStrategyLabel(row) }}</div>
                            <div class="assistant-stamp">{{ row.tavolo || row.Tavolo || '-' }}</div>
                        </div>
                        <div class="assistant-body">{{ buildStrategyLabel(row) }}</div>
                    </article>
                </div>
            </section>

            <section id="mobile-financial-reports" class="panel section">
                <div class="section-head">
                    <div>
                        <div class="section-title">Financial reports</div>
                        <div class="section-copy">Production {{ formatPeriodRange() }} · Demo {{ formatDemoPeriod() }}</div>
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
                <div class="report-actions">
                    <button type="button" class="month-report-link compact" @click="openFinancialReports">Pick period & download</button>
                </div>
                <div class="month-report-grid">
                    <article class="month-report-card">
                        <div class="month-report-label">Production</div>
                        <div class="month-report-value" :class="toneClass(productionPeriodResult)">{{ formatMoney(productionPeriodResult) }}</div>
                        <div class="month-report-meta">
                            Target {{ formatMoney(productionReport?.totals?.globalTargetEuro) }}<br />Progress {{ formatPercent(productionReport?.totals?.progressPct) }} · {{ productionReport?.totals?.sampleCount || 0 }} samples<br />official
                            accounting
                        </div>
                        <button type="button" class="month-report-link" @click="downloadReport('Production')">Open HTML</button>
                    </article>
                    <article class="month-report-card">
                        <div class="month-report-label">Demo</div>
                        <div class="month-report-value" :class="toneClass(demoPeriodResult)">{{ formatMoney(demoPeriodResult) }}</div>
                        <div class="month-report-meta">
                            Target {{ formatMoney(demoReport?.totals?.globalTargetEuro) }}<br />Progress {{ formatPercent(demoReport?.totals?.progressPct) }} · {{ demoReport?.totals?.sampleCount || 0 }} samples<br />non-accountable
                        </div>
                        <button type="button" class="month-report-link" @click="downloadReport('Demo')">Open HTML</button>
                    </article>
                </div>
                <div class="month-report-foot">Updated {{ lastSync || '--' }} · live data only</div>
            </section>

            <section class="panel section">
                <div class="section-head">
                    <div>
                        <div class="section-title">Notifiche push</div>
                        <div class="section-copy">Avvisi missione su mobile admin, se backend e browser li supportano</div>
                    </div>
                    <div class="mini-label">{{ pushStatus.subscribed ? 'ON' : 'OFF' }}</div>
                </div>
                <div class="push-box">
                    <div class="push-state" :class="{ active: pushStatus.subscribed, warn: !pushStatus.configured || pushStatus.permission === 'granted' }">
                        {{ pushStateLabel }}
                    </div>
                    <div class="push-message">{{ pushStatus.message }}</div>
                    <div class="push-meta">Browser: {{ pushStatus.supported ? 'supportato' : 'non supportato' }} · Permesso: {{ pushStatus.permission }} · Backend: {{ pushStatus.configured ? 'configurato' : 'non configurato' }}</div>
                    <div class="push-platform-note">
                        Android: abilita dal popup del browser. iOS: apri da app aggiunta alla schermata Home e abilita le notifiche da li.
                    </div>
                    <button type="button" class="push-button" :disabled="pushLoading || !pushStatus.supported" @click="enablePush">
                        {{ pushLoading ? 'Verifica...' : 'Consenti notifiche' }}
                    </button>
                </div>
            </section>

            <div class="foot-copy">Read-only mobile route. Desktop admin flow remains untouched.</div>
        </section>
    </main>
</template>

<style scoped>
.mobile-page {
    min-height: 100vh;
    display: flex;
    justify-content: center;
    padding: 18px 14px 34px;
    color: var(--text-color);
    background:
        radial-gradient(circle at 10% 0%, color-mix(in srgb, var(--primary-color) 20%, transparent), transparent 24%), radial-gradient(circle at 90% 0%, rgba(245, 158, 11, 0.14), transparent 26%),
        linear-gradient(180deg, var(--surface-ground) 0%, var(--surface-card) 42%, var(--surface-ground) 100%);
}
.shell {
    width: 100%;
    max-width: 440px;
    display: flex;
    flex-direction: column;
    gap: 16px;
}
.intro {
    display: flex;
    flex-direction: column;
    gap: 12px;
    padding: 8px 4px 2px;
}
.intro-head,
.hero-top,
.section-head,
.watch-top,
.assistant-top {
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    gap: 14px;
}
.intro-kicker,
.eyebrow,
.mini-label,
.month-report-label {
    font-size: 11px;
    letter-spacing: 0.16em;
    text-transform: uppercase;
    color: var(--text-color-secondary);
}
.intro-title {
    margin-top: 4px;
    font-size: 24px;
    line-height: 1;
    font-weight: 800;
    letter-spacing: -0.05em;
}
.intro-copy,
.section-copy,
.stat-sub,
.watch-detail,
.assistant-body,
.foot-copy,
.month-report-meta,
.month-report-foot,
.empty {
    font-size: 12px;
    color: var(--text-color-secondary);
    line-height: 1.45;
}
.brand-signature {
    width: fit-content;
    padding: 10px 14px 12px;
    border-radius: 18px;
    border: 1px solid var(--surface-border);
    background: rgba(255, 255, 255, 0.05);
    box-shadow: 0 14px 34px rgba(0, 0, 0, 0.18);
}
.brand-signature-mark {
    font-size: 21px;
    color: var(--primary-color);
    font-weight: 900;
}
.brand-signature-copy {
    margin-top: 2px;
    font-size: 10px;
    line-height: 1.2;
}
.status-pill {
    display: inline-flex;
    align-items: center;
    gap: 8px;
    padding: 9px 12px;
    border-radius: 999px;
    background: rgba(255, 255, 255, 0.05);
    border: 1px solid var(--surface-border);
    font-size: 11px;
    color: var(--text-color);
    white-space: nowrap;
}
.status-pill::before {
    content: '';
    width: 8px;
    height: 8px;
    border-radius: 50%;
    background: #22c55e;
    box-shadow: 0 0 0 6px rgba(34, 197, 94, 0.12);
}
.actions {
    display: flex;
    gap: 8px;
    flex-wrap: wrap;
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
    position: relative;
    overflow: hidden;
    border-radius: 30px;
    background: linear-gradient(180deg, rgba(255, 255, 255, 0.05), rgba(255, 255, 255, 0.015)), var(--surface-card);
    border: 1px solid var(--surface-border);
    box-shadow: 0 30px 80px rgba(0, 0, 0, 0.22);
}
.hero,
.section {
    padding: 18px;
}
.mega-label {
    margin-top: 10px;
    font-size: 12px;
    letter-spacing: 0.15em;
    text-transform: uppercase;
    color: var(--text-color-secondary);
}
.mega-value {
    margin-top: 8px;
    font-size: 44px;
    line-height: 0.94;
    font-weight: 800;
    letter-spacing: -0.06em;
}
.hero-note {
    margin-top: 10px;
    display: inline-flex;
    padding: 10px 12px;
    border-radius: 999px;
    background: rgba(34, 197, 94, 0.12);
    color: #22c55e;
    font-size: 13px;
    font-weight: 700;
}
.goal-orb {
    min-width: 122px;
    padding: 14px;
    border-radius: 24px;
    background: rgba(255, 255, 255, 0.04);
    border: 1px solid var(--surface-border);
}
.goal-orb-v {
    margin-top: 8px;
    font-size: 22px;
    font-weight: 800;
    color: #f59e0b;
}
.bar {
    margin-top: 12px;
    height: 8px;
    border-radius: 999px;
    background: rgba(255, 255, 255, 0.08);
    overflow: hidden;
}
.bar > span {
    display: block;
    height: 100%;
    border-radius: inherit;
    background: linear-gradient(90deg, #f59e0b, var(--primary-color));
}
.hero-grid {
    margin-top: 18px;
    display: grid;
    grid-template-columns: repeat(3, minmax(0, 1fr));
    gap: 10px;
}
.stat-box,
.chart-wrap,
.watch-item,
.assistant-item,
.empty,
.month-report-card {
    padding: 12px;
    border-radius: 18px;
    background: rgba(255, 255, 255, 0.04);
    border: 1px solid rgba(255, 255, 255, 0.06);
}
.stat-v {
    margin-top: 7px;
    font-size: 19px;
    font-weight: 800;
    letter-spacing: -0.04em;
    overflow-wrap: anywhere;
}
.stat-v.small {
    font-size: 13px;
}
.mission-chart {
    width: 100%;
    height: 180px;
}
.axis {
    stroke: var(--surface-border);
    stroke-width: 1;
}
.curve-line {
    fill: none;
    stroke: var(--primary-color);
    stroke-width: 3;
    stroke-linecap: round;
    stroke-linejoin: round;
}
.target-line {
    fill: none;
    stroke: #f59e0b;
    stroke-width: 2;
    stroke-dasharray: 7 7;
    opacity: 0.75;
}
.chart-meta,
.watch-meta {
    display: flex;
    justify-content: space-between;
    flex-wrap: wrap;
    gap: 8px;
    margin-top: 10px;
    font-size: 12px;
    color: var(--text-color-secondary);
}
.watchlist,
.assistant-feed {
    display: flex;
    flex-direction: column;
    gap: 10px;
}
.watch-item.critical {
    border-color: rgba(239, 68, 68, 0.45);
    box-shadow: 0 0 28px rgba(239, 68, 68, 0.12);
}
.watch-name,
.assistant-title {
    font-size: 14px;
    font-weight: 700;
}
.watch-level {
    min-width: 54px;
    padding: 7px 10px;
    border-radius: 999px;
    text-align: center;
    background: color-mix(in srgb, var(--primary-color) 12%, transparent);
    color: var(--primary-color);
    font-size: 11px;
    font-weight: 700;
}
.chip {
    padding: 6px 9px;
    border-radius: 999px;
    background: rgba(255, 255, 255, 0.05);
    color: var(--text-color-secondary);
    font-size: 10px;
}
.period-chips {
    display: flex;
    flex-wrap: wrap;
    gap: 8px;
    margin-top: 12px;
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
.month-report-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 10px;
    margin-top: 12px;
}
.report-actions {
    display: flex;
    justify-content: flex-end;
    margin: 12px 0 0;
}
.month-report-value {
    margin-top: 7px;
    font-size: 22px;
    font-weight: 800;
    letter-spacing: -0.05em;
}
.month-report-link,
.push-button {
    margin-top: 12px;
    min-height: 34px;
    padding: 8px 11px;
    border-radius: 999px;
    border: 1px solid color-mix(in srgb, var(--primary-color) 34%, transparent);
    background: color-mix(in srgb, var(--primary-color) 14%, transparent);
    color: var(--primary-color);
    font: inherit;
    font-size: 11px;
    font-weight: 800;
    letter-spacing: 0.06em;
    text-transform: uppercase;
    cursor: pointer;
}
.month-report-link.compact {
    margin-top: 0;
}
.push-button:disabled {
    opacity: 0.55;
    cursor: not-allowed;
}
.push-box {
    padding: 13px;
    border-radius: 18px;
    background: rgba(255, 255, 255, 0.04);
    border: 1px solid rgba(255, 255, 255, 0.06);
}
.push-state {
    display: inline-flex;
    padding: 7px 10px;
    border-radius: 999px;
    background: rgba(239, 68, 68, 0.12);
    color: #ef4444;
    font-size: 11px;
    font-weight: 800;
    letter-spacing: 0.08em;
    text-transform: uppercase;
}
.push-state.active {
    background: rgba(34, 197, 94, 0.12);
    color: #22c55e;
}
.push-state.warn {
    background: rgba(245, 158, 11, 0.12);
    color: #f59e0b;
}
.push-message {
    margin-top: 10px;
    font-size: 13px;
    font-weight: 700;
}
.push-meta {
    margin-top: 6px;
    color: var(--text-color-secondary);
    font-size: 11px;
    line-height: 1.4;
}
.push-platform-note {
    margin-top: 10px;
    padding: 10px 11px;
    border-radius: 14px;
    background: rgba(245, 158, 11, 0.1);
    border: 1px solid rgba(245, 158, 11, 0.18);
    color: var(--text-color-secondary);
    font-size: 11px;
    line-height: 1.4;
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
.error-banner {
    padding: 12px 14px;
    border-radius: 18px;
    color: #fecaca;
    background: rgba(239, 68, 68, 0.12);
    border: 1px solid rgba(239, 68, 68, 0.28);
}
</style>
