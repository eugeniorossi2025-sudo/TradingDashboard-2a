<script setup>
import { AuthService } from '@/service/AuthService';
import { DashboardService } from '@/service/DashboardService';
import { FinancialReportService } from '@/service/FinancialReportService';
import { formatRomeTime, toRomeIsoDate } from '@/utils/romeTime';
import { computed, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';

const router = useRouter();
const loading = ref(true);
const tableRows = ref([]);
const chartRows = ref([]);
const runtimeMode = ref({ runtimeMode: 'Production', isDemoMode: false });
const productionReport = ref(null);
const lastSync = ref('');
const error = ref('');

const today = new Date();
const [romeYear, romeMonth] = toRomeIsoDate(today).split('-');
const from = `${romeYear}-${romeMonth}-01`;
const to = toRomeIsoDate(today);

const activeTables = computed(() => tableRows.value.length);
const periodResult = computed(() => Number(productionReport.value?.totals?.periodResultEuro ?? 0));
const liveMargin = computed(() => tableRows.value.reduce((sum, row) => sum + getNumber(row, 'margine', 'Margine'), 0));
const target = computed(() => Number(productionReport.value?.totals?.globalTargetEuro || 0));
const progress = computed(() => {
    const value = Number(productionReport.value?.totals?.progressPct);
    return Number.isFinite(value) ? value : 0;
});
const strategy = computed(() => {
    const row = tableRows.value.find((item) => item.valutazione || item.Valutazione || item.reason || item.Reason);
    return row?.valutazione || row?.Valutazione || row?.reason || row?.Reason || '--';
});
const chartSamples = computed(() => (chartRows.value.length ? chartRows.value : tableRows.value));
const chartPath = computed(() => buildPath(chartSamples.value, (row) => getNumber(row, 'margine', 'Margine', 'margin')));

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

function formatMoney(value) {
    const number = Number(value || 0);
    const abs = new Intl.NumberFormat('it-IT', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(Math.abs(number));
    const sign = number > 0 ? '+' : number < 0 ? '-' : '';
    return `€ ${sign}${abs}`;
}

function formatPercent(value) {
    return `${new Intl.NumberFormat('it-IT', { minimumFractionDigits: 1, maximumFractionDigits: 1 }).format(Number(value || 0))}%`;
}

function buildPath(rows, selector) {
    const values = rows.map(selector).map(Number).filter(Number.isFinite);
    if (!values.length) return '';
    const min = Math.min(0, ...values);
    const max = Math.max(1, ...values);
    const span = Math.max(1, max - min);
    const maxIndex = Math.max(1, values.length - 1);
    return values
        .map((value, index) => {
            const x = 12 + (index / maxIndex) * 676;
            const y = 210 - ((value - min) / span) * 170;
            return `${index === 0 ? 'M' : 'L'}${x.toFixed(1)} ${y.toFixed(1)}`;
        })
        .join(' ');
}

async function loadData() {
    loading.value = true;
    error.value = '';
    try {
        const [dashboard, chart] = await Promise.all([DashboardService.getDashboardData(), DashboardService.getChartData()]);
        tableRows.value = Array.isArray(dashboard) ? dashboard : dashboard?.rows || dashboard?.tables || [];
        chartRows.value = Array.isArray(chart) ? chart : [];
        runtimeMode.value = await FinancialReportService.getRuntimeMode();
        productionReport.value = await FinancialReportService.getRangeReport('Production', from, to);
        lastSync.value = formatRomeTime();
    } catch (err) {
        console.error('Client desktop load error:', err);
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
    <main class="client-desktop">
        <nav class="client-nav">
            <div class="brand">
                <div class="logo-mark"></div>
                <div>
                    <div class="brand-title">
                        EUGENIO TRADING - CLIENT
                        <span class="mode-badge" :class="{ demo: runtimeMode?.isDemoMode }">{{ runtimeMode?.isDemoMode ? 'DEMO' : 'PRODUZIONE' }}</span>
                    </div>
                    <div class="brand-subtitle">Live KPI + Mission Analysis</div>
                </div>
            </div>
            <div class="nav-actions">
                <button type="button" @click="router.push('/client/mobile')">Vista mobile</button>
                <button type="button" @click="loadData">Sync</button>
                <button type="button" @click="logout">Logout</button>
            </div>
        </nav>

        <div v-if="error" class="error-banner">{{ error }}</div>

        <section class="kpi-grid">
            <article class="kpi-card">
                <div class="kpi-value">{{ activeTables }}</div>
                <div class="kpi-label">Tavoli Attivi</div>
            </article>
            <article class="kpi-card">
                <div class="kpi-value" :class="toneClass(periodResult)">{{ formatMoney(periodResult) }}</div>
                <div class="kpi-label">Risultato periodo (mese)</div>
            </article>
            <article class="kpi-card">
                <div class="kpi-value" :class="toneClass(liveMargin)">{{ formatMoney(liveMargin) }}</div>
                <div class="kpi-label">Margine live (PBT)</div>
            </article>
            <article class="kpi-card">
                <div class="kpi-value small">{{ strategy }}</div>
                <div class="kpi-label">Strategy Attiva</div>
            </article>
            <article class="kpi-card">
                <div class="kpi-value">{{ chartSamples.length }}</div>
                <div class="kpi-label">PBT Count</div>
            </article>
            <article class="kpi-card warning">
                <div class="kpi-value">{{ formatPercent(progress) }}</div>
                <div class="kpi-label">% Achievement</div>
            </article>
        </section>

        <section class="chart-panel">
            <div class="panel-header">
                <div>
                    <div class="panel-title">Chart - Mission Analysis</div>
                    <div class="panel-subtitle">Cliente read-only, senza strumenti admin</div>
                </div>
                <div class="last-sync">{{ loading ? 'Caricamento' : `Sync ${lastSync}` }}</div>
            </div>

            <svg class="chart" viewBox="0 0 700 240">
                <line class="axis" x1="12" y1="210" x2="688" y2="210"></line>
                <path class="chart-line" :class="toneClass(liveMargin)" :d="chartPath"></path>
            </svg>

            <div class="chart-stats">
                <div>
                    <span>Risultato periodo</span><strong :class="toneClass(periodResult)">{{ formatMoney(periodResult) }}</strong>
                </div>
                <div>
                    <span>Margine live</span><strong :class="toneClass(liveMargin)">{{ formatMoney(liveMargin) }}</strong>
                </div>
                <div>
                    <span>Target</span><strong>{{ formatMoney(target) }}</strong>
                </div>
                <div>
                    <span>Achievement</span><strong>{{ formatPercent(progress) }}</strong>
                </div>
                <div>
                    <span>Samples</span><strong>{{ chartSamples.length }}</strong>
                </div>
            </div>
        </section>
    </main>
</template>

<style scoped>
.client-desktop {
    min-height: 100vh;
    padding: 22px;
    color: var(--text-color);
    background: linear-gradient(180deg, var(--surface-ground), var(--surface-card));
}
.client-nav {
    display: flex;
    justify-content: space-between;
    align-items: center;
    gap: 18px;
    margin-bottom: 22px;
    padding: 14px 16px;
    border: 1px solid var(--surface-border);
    border-radius: 18px;
    background: var(--surface-card);
}
.brand {
    display: flex;
    align-items: center;
    gap: 14px;
}
.logo-mark {
    width: 34px;
    height: 34px;
    border-radius: 10px;
    background: linear-gradient(135deg, var(--primary-color), #f59e0b);
    clip-path: polygon(50% 0, 100% 100%, 0 100%);
}
.brand-title {
    font-weight: 800;
    letter-spacing: -0.02em;
}
.brand-subtitle,
.kpi-label,
.panel-subtitle,
.last-sync,
.chart-stats span {
    color: var(--text-color-secondary);
    font-size: 12px;
}
.mode-badge {
    display: inline-flex;
    margin-left: 8px;
    padding: 4px 8px;
    border-radius: 999px;
    font-size: 10px;
    color: var(--primary-color);
    background: color-mix(in srgb, var(--primary-color) 12%, transparent);
    border: 1px solid color-mix(in srgb, var(--primary-color) 36%, transparent);
}
.mode-badge.demo {
    color: #f59e0b;
    border-color: rgba(245, 158, 11, 0.38);
    background: rgba(245, 158, 11, 0.12);
}
.nav-actions {
    display: flex;
    gap: 8px;
    flex-wrap: wrap;
    justify-content: flex-end;
}
.nav-actions button {
    min-height: 36px;
    padding: 8px 13px;
    border-radius: 12px;
    border: 1px solid var(--surface-border);
    background: rgba(255, 255, 255, 0.04);
    color: var(--text-color);
    cursor: pointer;
}
.kpi-grid {
    display: grid;
    grid-template-columns: repeat(6, minmax(0, 1fr));
    gap: 14px;
    margin-bottom: 18px;
}
.kpi-card,
.chart-panel {
    border: 1px solid var(--surface-border);
    border-radius: 18px;
    background: var(--surface-card);
    box-shadow: 0 16px 48px rgba(0, 0, 0, 0.16);
}
.kpi-card {
    padding: 16px;
    min-height: 108px;
}
.kpi-value {
    font-size: 28px;
    font-weight: 850;
    letter-spacing: -0.05em;
}
.kpi-value.small {
    font-size: 15px;
    overflow-wrap: anywhere;
    color: var(--primary-color);
}
.warning .kpi-value {
    color: #f59e0b;
}
.chart-panel {
    padding: 18px;
}
.panel-header {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    gap: 14px;
    margin-bottom: 8px;
}
.panel-title {
    font-size: 18px;
    font-weight: 800;
}
.chart {
    width: 100%;
    height: 260px;
}
.axis {
    stroke: var(--surface-border);
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
.chart-stats {
    display: grid;
    grid-template-columns: repeat(4, minmax(0, 1fr));
    gap: 10px;
}
.chart-stats div {
    padding: 12px;
    border-radius: 14px;
    background: rgba(255, 255, 255, 0.04);
}
.chart-stats strong {
    display: block;
    margin-top: 4px;
    font-size: 18px;
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
    margin-bottom: 14px;
    padding: 12px 14px;
    border-radius: 14px;
    color: #fecaca;
    background: rgba(239, 68, 68, 0.12);
    border: 1px solid rgba(239, 68, 68, 0.28);
}
@media (max-width: 900px) {
    .client-nav {
        align-items: flex-start;
        flex-direction: column;
    }
    .kpi-grid,
    .chart-stats {
        grid-template-columns: 1fr 1fr;
    }
}
</style>
