<script setup>
import { ConfigurationService } from '@/service/ConfigurationService';
import { computed, onMounted, ref } from 'vue';

const props = defineProps({
    tableData: {
        type: Array,
        default: null
    },
    chartData: {
        type: Array,
        default: () => []
    }
});

const target = ref(0);
const missionTime = ref(0);

onMounted(async () => {
    const targetResponse = await ConfigurationService.getConfigurationById('STOP_WIN');
    const timeMissionResponse = await ConfigurationService.getConfigurationById('STOP_TIME');
    target.value = targetResponse?.value ?? 0;
    missionTime.value = timeMissionResponse?.value ?? 0;
});

// 1. Valorizza timestamp delle statistiche con data di adesso quando è nulla
function enrichStatistiche(statistiche) {
    const now = new Date().toISOString();
    return statistiche.map((row) => ({
        ...row,
        timestamp: row.timestamp || now
    }));
}

// Usa tutte le entry della tabella Statistiche per il grafico e i calcoli
const statistiche = computed(() => {
    // props.chartData rappresenta la tabella Statistiche
    return enrichStatistiche(props.chartData || []);
});

// 3. Margine Corrente: (riga Statistiche più recente).MARGINE_TOT
const margineCorrente = computed(() => {
    if (!statistiche.value.length) return 0;
    const latest = statistiche.value.reduce((a, b) => (new Date(a.timestamp) > new Date(b.timestamp) ? a : b));
    return Number(latest.margine || 0).toFixed(2);
});

// 4. Margine Min: (riga Statistiche più recente).MARGINE_MIN
const margineMin = computed(() => {
    if (!statistiche.value.length) return 0;
    const latest = statistiche.value.reduce((a, b) => (new Date(a.timestamp) > new Date(b.timestamp) ? a : b));
    return Number(latest.margineMin || 0).toFixed(2);
});

// 5. Margine MAX: (riga Statistiche più recente).MARGINE_MAX
const margineMax = computed(() => {
    if (!statistiche.value.length) return 0;
    const latest = statistiche.value.reduce((a, b) => (new Date(a.timestamp) > new Date(b.timestamp) ? a : b));
    return Number(latest.margineMax || 0).toFixed(2);
});

const formatCurrency = (value) => {
    if (value === null || value === undefined) return '€0,00';

    let num = value;
    if (typeof num === 'string') {
        num = num.replace(',', '.');
    }

    num = Number(num);
    if (isNaN(num)) return '€0,00';

    return new Intl.NumberFormat('it-IT', {
        style: 'currency',
        currency: 'EUR'
    }).format(num);
};

// Per compatibilità con il template esistente
const margineAttuale = margineCorrente;

const speed = computed(() => {
    const margineNum = Number(margineAttuale.value);
    return isNaN(margineNum) ? formatCurrency(0) : formatCurrency(margineNum / 60);
});

const achievement = computed(() => {
    return Math.round((margineAttuale.value / target.value) * 100) || 0;
    // return latestBot ? JSON.parse(latestBot.missionSnapshot).AchievementPercent ?? 0 : 0;
});

// Tempo trascorso: numero di ore più alto in tabella
const tempoTrascorso = computed(() => {
    if (!statistiche.value.length) return 0;
    const latest = statistiche.value.reduce((a, b) => (new Date(a.timestamp) > new Date(b.timestamp) ? a : b));
    return Math.floor(Number(latest.elapsed || 0));
});
</script>

<template>
    <div class="col-span-12 lg:col-span-6 xl:col-span-3">
        <div class="card mb-0">
            <div class="flex justify-between mb-4">
                <div>
                    <span class="block text-muted-color font-medium mb-4">Tempo Trascorso</span>
                    <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">{{ tempoTrascorso }} min</div>
                </div>
                <div class="flex items-center justify-center bg-blue-100 dark:bg-blue-400/10 rounded-border" style="width: 2.5rem; height: 2.5rem">
                    <i class="pi pi-clock text-blue-500 !text-xl"></i>
                </div>
            </div>
        </div>
    </div>
    <div class="col-span-12 lg:col-span-6 xl:col-span-3">
        <div class="card mb-0">
            <div class="flex justify-between mb-4">
                <div>
                    <span class="block text-muted-color font-medium mb-4">Margine Minimo</span>
                    <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">{{ margineMin }}</div>
                </div>
                <div class="flex items-center justify-center bg-orange-100 dark:bg-orange-400/10 rounded-border" style="width: 2.5rem; height: 2.5rem">
                    <i class="pi pi-sort-amount-down text-orange-500 !text-xl"></i>
                </div>
            </div>
        </div>
    </div>
    <div class="col-span-12 lg:col-span-6 xl:col-span-3">
        <div class="card mb-0">
            <div class="flex justify-between mb-4">
                <div>
                    <span class="block text-muted-color font-medium mb-4">Margine Massimo</span>
                    <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">{{ margineMax }}</div>
                </div>
                <div class="flex items-center justify-center bg-cyan-100 dark:bg-cyan-400/10 rounded-border" style="width: 2.5rem; height: 2.5rem">
                    <i class="pi pi-sort-amount-up text-cyan-500 !text-xl"></i>
                </div>
            </div>
        </div>
    </div>
    <div class="col-span-12 lg:col-span-6 xl:col-span-3">
        <div class="card mb-0">
            <div class="flex justify-between mb-4">
                <div>
                    <span class="block text-muted-color font-medium mb-4">Margine Attuale</span>
                    <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">{{ margineAttuale }}</div>
                </div>
                <div class="flex items-center justify-center bg-purple-100 dark:bg-purple-400/10 rounded-border" style="width: 2.5rem; height: 2.5rem">
                    <i class="pi pi-gauge text-purple-500 !text-xl"></i>
                </div>
            </div>
        </div>
    </div>
    <div class="col-span-12 lg:col-span-6 xl:col-span-3">
        <div class="card mb-0">
            <div class="flex justify-between mb-4">
                <div>
                    <span class="block text-muted-color font-medium mb-4">Target</span>
                    <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">{{ target }}</div>
                </div>
                <div class="flex items-center justify-center bg-purple-100 dark:bg-purple-400/10 rounded-border" style="width: 2.5rem; height: 2.5rem">
                    <i class="pi pi-bullseye text-purple-500 !text-xl"></i>
                </div>
            </div>
        </div>
    </div>
    <div class="col-span-12 lg:col-span-6 xl:col-span-3">
        <div class="card mb-0">
            <div class="flex justify-between mb-4">
                <div>
                    <span class="block text-muted-color font-medium mb-4">Mission Time</span>
                    <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">
                        {{ Math.round(missionTime) }}
                        min
                    </div>
                </div>
                <div class="flex items-center justify-center bg-purple-100 dark:bg-purple-400/10 rounded-border" style="width: 2.5rem; height: 2.5rem">
                    <i class="pi pi-stopwatch text-purple-500 !text-xl"></i>
                </div>
            </div>
        </div>
    </div>
    <div class="col-span-12 lg:col-span-6 xl:col-span-3">
        <div class="card mb-0">
            <div class="flex justify-between mb-4">
                <div>
                    <span class="block text-muted-color font-medium mb-4">Speed</span>
                    <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">{{ speed }} / min</div>
                </div>
                <div class="flex items-center justify-center bg-purple-100 dark:bg-purple-400/10 rounded-border" style="width: 2.5rem; height: 2.5rem">
                    <i class="pi pi-send text-purple-500 !text-xl"></i>
                </div>
            </div>
        </div>
    </div>
    <div class="col-span-12 lg:col-span-6 xl:col-span-3">
        <div class="card mb-0">
            <div class="flex justify-between mb-4">
                <div>
                    <span class="block text-muted-color font-medium mb-4">Achievement</span>
                    <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">{{ achievement }}%</div>
                </div>
                <div class="flex items-center justify-center bg-purple-100 dark:bg-purple-400/10 rounded-border" style="width: 2.5rem; height: 2.5rem">
                    <i class="pi pi-percentage text-purple-500 !text-xl"></i>
                </div>
            </div>
        </div>
    </div>
</template>
