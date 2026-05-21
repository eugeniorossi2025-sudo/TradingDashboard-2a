<template>
    <div :style="{ color: colorMap[resultValutation.Color] || 'inherit' }">
        <span>{{ resultValutation.Message }}</span>
        <span v-if="resultValutation.VmValue !== undefined">
            &nbsp;| VmValue: {{ resultValutation.VmValue.toFixed(2) }}
        </span>
    </div>
</template>

<script setup>
const props = defineProps({
    resultValutation: {
        type: Object,
        required: true
    }
});

// Map color names to CSS colors
const colorMap = {
    yellow: '#facc15', // Tailwind yellow-400
    red: '#ef4444',    // Tailwind red-500
    green: '#22c55e',  // Tailwind green-500
    blue: '#3b82f6',   // Tailwind blue-500
    orange: '#fb923c', // Tailwind orange-400
    // Add more as needed
};
</script>
