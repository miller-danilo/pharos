<template>
  <div class="glass-panel p-6 space-y-6">
    <h3 class="text-lg font-bold text-gradient">Cost Multipliers Configuration</h3>
    
    <form @submit.prevent="$emit('save')" class="space-y-4">
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div class="space-y-1.5">
          <label class="block text-xs font-semibold text-text-secondary">Gemini Input Token Rate (USD per 1M)</label>
          <input
            :value="inputRate"
            @input="emitUpdate('inputRate', $event)"
            type="number"
            step="0.001"
            class="w-full h-10 bg-bg-deep/50 border border-white/10 rounded-lg px-3 text-xs text-text-primary focus:outline-none focus:border-brand-primary/50"
            :disabled="loading"
          />
        </div>
        <div class="space-y-1.5">
          <label class="block text-xs font-semibold text-text-secondary">Gemini Output Token Rate (USD per 1M)</label>
          <input
            :value="outputRate"
            @input="emitUpdate('outputRate', $event)"
            type="number"
            step="0.001"
            class="w-full h-10 bg-bg-deep/50 border border-white/10 rounded-lg px-3 text-xs text-text-primary focus:outline-none focus:border-brand-primary/50"
            :disabled="loading"
          />
        </div>
        <div class="space-y-1.5">
          <label class="block text-xs font-semibold text-text-secondary">Firestore Read Rate (USD per 100k)</label>
          <input
            :value="readRate"
            @input="emitUpdate('readRate', $event)"
            type="number"
            step="0.001"
            class="w-full h-10 bg-bg-deep/50 border border-white/10 rounded-lg px-3 text-xs text-text-primary focus:outline-none focus:border-brand-primary/50"
            :disabled="loading"
          />
        </div>
        <div class="space-y-1.5">
          <label class="block text-xs font-semibold text-text-secondary">Firestore Write Rate (USD per 100k)</label>
          <input
            :value="writeRate"
            @input="emitUpdate('writeRate', $event)"
            type="number"
            step="0.001"
            class="w-full h-10 bg-bg-deep/50 border border-white/10 rounded-lg px-3 text-xs text-text-primary focus:outline-none focus:border-brand-primary/50"
            :disabled="loading"
          />
        </div>
      </div>

      <div class="flex items-center gap-4">
        <button
          type="submit"
          :disabled="loading"
          class="px-5 h-10 bg-brand-primary hover:bg-brand-primary/95 text-white font-semibold rounded-lg text-xs shadow-md transition-all cursor-pointer disabled:opacity-50"
        >
          <LoadingSpinner v-if="loading" />
          <span v-else>Guardar Parámetros</span>
        </button>

        <span v-if="saveMessage" class="text-xs text-semaphore-safe font-medium">{{ saveMessage }}</span>
      </div>
    </form>
  </div>
</template>

<script setup lang="ts">
import LoadingSpinner from './LoadingSpinner.vue';

defineProps<{
  inputRate: number;
  outputRate: number;
  readRate: number;
  writeRate: number;
  loading: boolean;
  saveMessage: string;
}>();

const emit = defineEmits<{
  (e: 'update:inputRate', value: number): void;
  (e: 'update:outputRate', value: number): void;
  (e: 'update:readRate', value: number): void;
  (e: 'update:writeRate', value: number): void;
  (e: 'save'): void;
}>();

const emitUpdate = (key: 'inputRate' | 'outputRate' | 'readRate' | 'writeRate', e: Event) => {
  const value = parseFloat((e.target as HTMLInputElement).value) || 0;
  emit(`update:${key}` as any, value);
};
</script>
