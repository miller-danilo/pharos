<template>
  <div v-if="noCreditsMode" class="p-6 rounded-lg border border-semaphore-danger/20 bg-semaphore-danger/5 text-center space-y-3">
    <p class="text-sm text-semaphore-danger font-semibold">No credits remaining</p>
    <p class="text-xs text-text-secondary">Please purchase a credit package to unlock AI cover letter generation aligned with your resume.</p>
    <button
      @click="$emit('buy')"
      :disabled="loading"
      class="px-6 h-10 bg-brand-primary text-white font-semibold rounded-lg shadow-md hover:bg-brand-primary/95 transition-all text-xs cursor-pointer glow-primary disabled:opacity-50 disabled:cursor-not-allowed"
    >
      <LoadingSpinner v-if="loading" />
      <span v-else>Get Credits Now</span>
    </button>
  </div>
  <div v-else class="flex items-center gap-4">
    <div class="text-right">
      <p class="text-xs text-text-secondary font-medium">Credits</p>
      <p class="text-lg font-bold text-gradient">{{ credits }} left</p>
    </div>
    <button
      @click="$emit('buy')"
      :disabled="loading"
      class="px-4 h-9 bg-brand-primary/20 hover:bg-brand-primary/30 text-brand-gradient-start text-xs font-semibold rounded-md border border-brand-primary/30 transition-all cursor-pointer disabled:opacity-50"
    >
      <LoadingSpinner v-if="loading" />
      <span v-else>Buy Credits</span>
    </button>
  </div>
</template>

<script setup lang="ts">
import LoadingSpinner from './LoadingSpinner.vue';

defineProps<{
  credits: number;
  loading: boolean;
  noCreditsMode?: boolean;
}>();

defineEmits<{
  (e: 'buy'): void;
}>();
</script>
