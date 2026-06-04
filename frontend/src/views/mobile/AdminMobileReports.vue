<script setup lang="ts">
import { FinancialReportService } from '@/service/FinancialReportService';
import { REPORT_PERIOD_CHIPS, getPeriodRange, type ReportPeriodChip } from '@/composables/useReportPeriod';
import { formatRomeTime } from '@/utils/romeTime';
import MobileAdminQuickNav from '@/components/mobile/MobileAdminQuickNav.vue';
import { computed, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';

const router = useRouter();
const from = ref('');
const to = ref('');
const periodChip = ref<ReportPeriodChip>('month');
const periodChips = REPORT_PERIOD_CHIPS;
const runtimeMode = ref('Production');
const report = ref(null);
const loading = ref(false);
const downloading = ref('');
const error = ref('');
const lastSync = ref('');

const periodLabel = computed(() => {
    const [, fm, fd] = from.value.split('-');
    const [, tm, td] = to.value.split('-');
    return `${fd}/${fm} - ${td}/${tm}`;
});

const isRangeValid = computed(() => Boolean(from.value && to.value && from.value <= to.value));

const periodResultEuro = computed(() => {
    const totals = report.value?.totals;
    if (!totals) return 0;
    return Number(totals.periodResultEuro ?? totals.totalMarginEuro ?? 0);
});

const isDemoEmpty = computed(() => runtimeMode.value === 'Demo' && !loading.value && !error.value && (report.value?.totals?.sampleCount ?? 0) === 0);

function formatMoney(value) {
    const number = Number(value || 0);
    const abs = new Intl.NumberFormat('it-IT', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(Math.abs(number));
    const sign = number > 0 ? '+' : number < 0 ? '-' : '';
    return `€ ${sign}${abs}`;
}

function formatPercent(value) {
    return `${new Intl.NumberFormat('it-IT', { minimumFractionDigits: 1, maximumFractionDigits: 1 }).format(Number(value || 0))}%`;
}

function toneClass(value) {
    const number = Number(value || 0);
    if (number > 0.01) return 'pos';
    if (number < -0.01) return 'neg';
    return 'neutral';
}

function applyPeriodChip(chip: ReportPeriodChip) {
    periodChip.value = chip;
    const range = getPeriodRange(chip, new Date());
    from.value = range.from;
    to.value = range.to;
    loadReport();
}

function onRuntimeModeChange(mode: 'Production' | 'Demo') {
    runtimeMode.value = mode;
    applyPeriodChip(periodChip.value);
}

function goBack() {
    router.push('/admin/mobile-live');
}

function openDesktopReports() {
    router.push('/pages/log');
}

async function loadReport() {
    if (!isRangeValid.value) {
        error.value = 'Seleziona un periodo valido: la data Da deve essere prima o uguale alla data A.';
        return;
    }

    loading.value = true;
    error.value = '';
    try {
        report.value = await FinancialReportService.getRangeReport(runtimeMode.value, from.value, to.value);
        lastSync.value = formatRomeTime();
    } catch (err) {
        console.error('Admin mobile reports load error:', err);
        const status = err?.response?.status;
        if (runtimeMode.value === 'Demo' && status === 200) {
            error.value = '';
        } else {
            error.value = err?.response?.data?.message || 'Report non disponibile per il periodo selezionato.';
        }
    } finally {
        loading.value = false;
    }
}

async function download(format) {
    if (!isRangeValid.value) {
        error.value = 'Seleziona un periodo valido prima di scaricare.';
        return;
    }

    downloading.value = format;
    error.value = '';
    try {
        if (format === 'csv') {
            await FinancialReportService.downloadCsv(runtimeMode.value, from.value, to.value);
        } else if (format === 'html') {
            await FinancialReportService.openHtmlReport(runtimeMode.value, from.value, to.value);
        } else {
            await FinancialReportService.downloadJson(runtimeMode.value, from.value, to.value);
        }
    } catch (err) {
        console.error('Admin mobile reports download error:', err);
        error.value = 'Download non riuscito per il periodo selezionato.';
    } finally {
        downloading.value = '';
    }
}

onMounted(() => applyPeriodChip('month'));
</script>

<template>
    <main class="mobile-page">
        <section class="shell">
            <section class="intro">
                <div class="intro-head">
                    <div>
                        <div class="intro-kicker">Admin mobile</div>
                        <div class="intro-title">Financial reports</div>
                        <div class="intro-copy">Pick period and mode, then download live reports.</div>
                    </div>
                    <button type="button" class="link-btn" @click="goBack">Live</button>
                </div>
                <MobileAdminQuickNav />
            </section>

            <div v-if="error" class="error-banner">{{ error }}</div>
            <div v-else-if="isDemoEmpty" class="error-banner demo-empty">No Demo missions in this period.</div>

            <section class="panel section">
                <div class="section-head">
                    <div>
                        <div class="section-title">Report options</div>
                        <div class="section-copy">Selected period: {{ periodLabel }}</div>
                    </div>
                    <div class="mini-label">{{ runtimeMode }}</div>
                </div>

                <div class="mode-tabs">
                    <button type="button" :class="{ active: runtimeMode === 'Production' }" @click="onRuntimeModeChange('Production')">Production</button>
                    <button type="button" :class="{ active: runtimeMode === 'Demo' }" @click="onRuntimeModeChange('Demo')">Demo</button>
                </div>

                <div class="period-chips">
                    <button
                        v-for="chip in periodChips"
                        :key="chip.id"
                        type="button"
                        class="period-chip"
                        :class="{ active: periodChip === chip.id }"
                        @click="applyPeriodChip(chip.id)"
                    >
                        {{ chip.label }}
                    </button>
                </div>

                <div class="date-grid">
                    <label>
                        From
                        <input v-model="from" type="date" @change="loadReport" />
                    </label>
                    <label>
                        To
                        <input v-model="to" type="date" @change="loadReport" />
                    </label>
                </div>

                <div class="quick-actions">
                    <button type="button" class="ghost-btn" @click="loadReport">Refresh</button>
                </div>
            </section>

            <section class="panel hero">
                <div class="hero-top">
                    <div>
                        <div class="eyebrow">Period result</div>
                        <div class="mega-value" :class="toneClass(periodResultEuro)">{{ loading ? '...' : formatMoney(periodResultEuro) }}</div>
                        <div class="hero-note">{{ report?.totals?.sampleCount || 0 }} live samples · Upd. {{ lastSync || '--' }}</div>
                    </div>
                </div>

                <div class="summary-grid">
                    <div class="summary-card">
                        <div class="mini-label">Target</div>
                        <div class="summary-value">{{ formatMoney(report?.totals?.globalTargetEuro) }}</div>
                    </div>
                    <div class="summary-card">
                        <div class="mini-label">Progress</div>
                        <div class="summary-value">{{ formatPercent(report?.totals?.progressPct) }}</div>
                    </div>
                    <div class="summary-card">
                        <div class="mini-label">Min / Max</div>
                        <div class="summary-value small">{{ formatMoney(report?.totals?.margineMin) }} / {{ formatMoney(report?.totals?.margineMax) }}</div>
                    </div>
                    <div class="summary-card">
                        <div class="mini-label">Daily avg</div>
                        <div class="summary-value small">{{ formatMoney(report?.totals?.averageDailyPnl) }}</div>
                    </div>
                </div>
            </section>

            <section class="panel section">
                <div class="section-head">
                    <div>
                        <div class="section-title">Download</div>
                        <div class="section-copy">Scarica in base alle scelte sopra</div>
                    </div>
                </div>
                <div class="download-grid">
                    <button type="button" class="download-btn primary" :disabled="loading || !!downloading" @click="download('csv')">
                        {{ downloading === 'csv' ? 'Scarico...' : 'Scarica CSV' }}
                    </button>
                    <button type="button" class="download-btn" :disabled="loading || !!downloading" @click="download('html')">
                        {{ downloading === 'html' ? 'Apro...' : 'Apri HTML' }}
                    </button>
                    <button type="button" class="download-btn" :disabled="loading || !!downloading" @click="download('json')">
                        {{ downloading === 'json' ? 'Scarico...' : 'Scarica JSON' }}
                    </button>
                </div>
                <button type="button" class="desktop-link" @click="openDesktopReports">Apri Report e Log completo</button>
            </section>

            <div class="foot-copy">Route mobile admin. Usa solo endpoint finanziari esistenti.</div>
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
    padding: 8px 4px 2px;
}
.intro-head,
.section-head,
.hero-top {
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    gap: 14px;
}
.intro-kicker,
.eyebrow,
.mini-label {
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
.hero-note,
.foot-copy {
    font-size: 12px;
    color: var(--text-color-secondary);
    line-height: 1.45;
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
.link-btn,
.ghost-btn,
.download-btn,
.desktop-link,
.mode-tabs button {
    min-height: 36px;
    padding: 8px 11px;
    border-radius: 999px;
    border: 1px solid var(--surface-border);
    background: color-mix(in srgb, var(--surface-card) 80%, transparent);
    color: var(--text-color);
    font: inherit;
    font-size: 12px;
    cursor: pointer;
}
.mode-tabs {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 8px;
    margin-top: 14px;
}
.mode-tabs button.active,
.download-btn.primary {
    border-color: color-mix(in srgb, var(--primary-color) 38%, transparent);
    background: color-mix(in srgb, var(--primary-color) 16%, transparent);
    color: var(--primary-color);
    font-weight: 800;
}
.period-chips {
    display: flex;
    flex-wrap: wrap;
    gap: 8px;
    margin-top: 14px;
}
.period-chip {
    min-height: 36px;
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
    border-color: color-mix(in srgb, var(--primary-color) 38%, transparent);
    background: color-mix(in srgb, var(--primary-color) 16%, transparent);
    color: var(--primary-color);
    font-weight: 800;
}
.date-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 10px;
    margin-top: 14px;
}
.date-grid label {
    display: flex;
    flex-direction: column;
    gap: 6px;
    font-size: 11px;
    color: var(--text-color-secondary);
    letter-spacing: 0.1em;
    text-transform: uppercase;
}
.date-grid input {
    width: 100%;
    min-height: 42px;
    border-radius: 14px;
    border: 1px solid var(--surface-border);
    background: rgba(255, 255, 255, 0.04);
    color: var(--text-color);
    padding: 0 10px;
    font: inherit;
    font-size: 13px;
}
.quick-actions,
.download-grid {
    display: grid;
    grid-template-columns: repeat(3, minmax(0, 1fr));
    gap: 8px;
    margin-top: 14px;
}
.mega-value {
    margin-top: 8px;
    font-size: 42px;
    line-height: 0.94;
    font-weight: 800;
    letter-spacing: -0.06em;
}
.hero-note {
    margin-top: 10px;
}
.summary-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 10px;
    margin-top: 18px;
}
.summary-card {
    padding: 12px;
    border-radius: 18px;
    background: rgba(255, 255, 255, 0.04);
    border: 1px solid rgba(255, 255, 255, 0.06);
}
.summary-value {
    margin-top: 7px;
    font-size: 18px;
    font-weight: 800;
    letter-spacing: -0.04em;
}
.summary-value.small {
    font-size: 12px;
    line-height: 1.35;
}
.download-btn,
.desktop-link {
    min-height: 42px;
    font-weight: 800;
}
.download-btn:disabled {
    opacity: 0.55;
    cursor: not-allowed;
}
.desktop-link {
    width: 100%;
    margin-top: 10px;
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
