<script setup>
import { useLayout } from '@/layout/composables/layout';
import { computed, onMounted, ref, watch } from 'vue';

const { getPrimary, getSurface, isDarkTheme } = useLayout();

const margine = computed(() => {
    const bots = props.chartData ?? [];
    if (!Array.isArray(bots) || !bots.length) return null;
    return Number(bots.reduce((sum, bot) => sum + (bot.margine || 0), 0).toFixed(2));
});

const props = defineProps({
    chartData: {
        type: Array,
        default: () => []
    },
    title: {
        type: String,
        default: 'Profits Chart'
    }
});

const sourceChartData = ref([]);

const chartDataRef = ref(null);

const chartOptions = ref(null);

const formattedChartData = computed(() => {
    const documentStyle = getComputedStyle(document.documentElement);

    // Usa i dati originali
    const originalPoints = Array.isArray(props.chartData) ? props.chartData : [];

    // Punto finale: margine attuale e timestamp attuale SOLO se margine valido
    let chartPoints = [...originalPoints];

    // Trova tutte le date uniche ordinate
    const allDates = Array.from(new Set(chartPoints.map((item) => item.dateTime ?? item.timestamp))).sort();
    let selectedDates = allDates;

    const labels = allDates.map((dateStr) => {
        if (selectedDates.includes(dateStr)) {
            const date = new Date(dateStr);
            const localDate = new Date(date.getTime() - date.getTimezoneOffset() * 60000);
            return localDate.toLocaleString('it-IT', {
                day: '2-digit',
                month: '2-digit',
                year: 'numeric',
                hour: '2-digit',
                minute: '2-digit'
            });
        }
        return '';
    });

    const dataByDate = {};

    chartPoints.forEach((item) => {
        dataByDate[item.dateTime ?? item.timestamp] = item.margine;
    });

    const data = allDates.map((date) => dataByDate[date] ?? null);

    const color = documentStyle.getPropertyValue('--p-primary-500') || '#3b82f6';

    const datasets = [
        {
            label: 'Margine',
            data,
            fill: false,
            backgroundColor: color,
            borderColor: color,
            tension: 0.4,
            spanGaps: true
        }
    ];

    return {
        labels,
        datasets
    };
});

function setChartOptions() {
    const documentStyle = getComputedStyle(document.documentElement);
    const textColor = documentStyle.getPropertyValue('--text-color');
    const textColorSecondary = documentStyle.getPropertyValue('--text-color-secondary');
    const surfaceBorder = documentStyle.getPropertyValue('--surface-border');

    return {
        plugins: {
            legend: {
                labels: {
                    fontColor: textColor
                }
            },
            tooltip: {
                callbacks: {
                    label: function (context) {
                        const value = context.parsed.y;
                        return `Margine: ${value}`;
                    }
                }
            }
        },
        scales: {
            x: {
                ticks: {
                    color: textColorSecondary
                },
                grid: {
                    color: surfaceBorder,
                    drawBorder: false
                }
            },
            y: {
                ticks: {
                    color: textColorSecondary
                },
                grid: {
                    color: surfaceBorder,
                    drawBorder: false
                }
            }
        }
    };
}

watch([getPrimary, getSurface, isDarkTheme, sourceChartData], () => {
    chartDataRef.value = formattedChartData.value;
    chartOptions.value = setChartOptions();
});

watch(margine, () => {
    chartDataRef.value = formattedChartData.value;
});

watch(
    () => props.chartData,
    (value) => {
        sourceChartData.value = Array.isArray(value) ? value : [];
        chartDataRef.value = formattedChartData.value;
    },
    { deep: true }
);

onMounted(async () => {
    sourceChartData.value = props.chartData;
    chartDataRef.value = formattedChartData.value;
    chartOptions.value = setChartOptions();
});
</script>

<template>
    <div class="card">
        <div class="font-semibold text-xl mb-4">{{ title }}</div>
        <Chart type="line" :data="chartDataRef" :options="chartOptions" />
    </div>
</template>
