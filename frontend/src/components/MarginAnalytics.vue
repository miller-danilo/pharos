<template>
  <div class="grid grid-cols-1 sm:grid-cols-3 gap-6">
    <div class="glass-panel p-6 space-y-2">
      <p class="text-xs font-semibold text-text-secondary">Ingresos Totales (Est.)</p>
      <p class="text-3xl font-extrabold text-gradient font-mono">${{ revenue.toFixed(2) }}</p>
      <p class="text-[10px] text-text-secondary">Venta de Créditos en Pasarelas</p>
    </div>

    <div class="glass-panel p-6 space-y-2">
      <p class="text-xs font-semibold text-text-secondary">Costos de Infraestructura (Est.)</p>
      <p class="text-3xl font-extrabold text-gradient-orange font-mono">${{ infraCost.toFixed(4) }}</p>
      <p class="text-[10px] text-text-secondary">AI Tokens + Firestore Operaciones</p>
    </div>

    <div class="glass-panel p-6 space-y-2">
      <p class="text-xs font-semibold text-text-secondary">Margen Operativo</p>
      <p :class="['text-3xl font-extrabold font-mono', marginValue >= 0 ? 'text-semaphore-safe' : 'text-semaphore-danger']">
        {{ marginValue >= 0 ? '+' : '' }}{{ margin }}%
      </p>
      <p class="text-[10px] text-text-secondary">Rentabilidad de uso general</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';

const props = defineProps<{
  revenue: number;
  infraCost: number;
  margin: string;
}>();

const marginValue = computed(() => parseFloat(props.margin) || 0);
</script>
