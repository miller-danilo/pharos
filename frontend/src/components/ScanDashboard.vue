<template>
  <div class="w-full max-w-4xl mx-auto space-y-8">
    <div v-if="!scanResult" class="glass-panel p-12 text-center space-y-4">
      <svg xmlns="http://www.w3.org/2000/svg" class="w-12 h-12 text-text-secondary mx-auto" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <circle cx="12" cy="12" r="10"/>
        <line x1="12" y1="8" x2="12" y2="12"/>
        <line x1="12" y1="16" x2="12.01" y2="16"/>
      </svg>
      <h2 class="text-xl font-bold">No Active Scan Found</h2>
      <p class="text-sm text-text-secondary">You haven't scanned a job listing yet. Please go back to the scanner page.</p>
      <a href="/" class="inline-flex items-center gap-2 px-6 h-11 bg-brand-primary text-white font-semibold rounded-lg shadow-md hover:bg-brand-primary/95 transition-all text-sm cursor-pointer glow-primary">
        Go to Scanner
      </a>
      <p v-if="error" class="text-sm text-semaphore-danger text-center mt-2 font-medium">{{ error }}</p>
    </div>

    <div v-else class="space-y-8">
      <a href="/" class="inline-flex items-center gap-2 text-sm text-text-secondary hover:text-text-primary transition-colors">
        <svg xmlns="http://www.w3.org/2000/svg" class="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <line x1="19" y1="12" x2="5" y2="12"/>
          <polyline points="12 19 5 12 12 5"/>
        </svg>
        Scan Another Job
      </a>

      <div class="glass-panel p-8 relative overflow-hidden">
        <div class="flex flex-col md:flex-row items-start md:items-center justify-between gap-6">
          <div class="space-y-2">
            <h2 class="text-2xl font-bold text-gradient">Scan Verdict</h2>
            <p class="text-sm text-text-secondary leading-relaxed max-w-xl">
              {{ scanResult.summary }}
            </p>
          </div>

          <div :class="['px-6 py-4 rounded-xl font-extrabold text-base tracking-widest uppercase flex items-center gap-2.5 transition-all duration-300 select-none border', badgeClasses]">
            <span class="w-3.5 h-3.5 rounded-full bg-current animate-pulse"></span>
            {{ scanResult.riskLevel }}
          </div>
        </div>

        <div class="mt-6 pt-6 border-t border-white/5 space-y-2">
          <h4 class="text-sm font-semibold text-text-primary">Reasoning</h4>
          <p class="text-sm text-text-secondary leading-relaxed">
            {{ scanResult.reasoning }}
          </p>
        </div>
      </div>

      <div class="space-y-4">
        <h3 class="text-lg font-semibold text-text-primary">Analysis Factors</h3>
        
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div
            v-for="(factor, idx) in scanResult.factors"
            :key="idx"
            class="glass-panel p-5 flex items-start gap-4"
          >
            <div :class="['p-2 rounded-lg shrink-0', factor.isRisk ? 'bg-semaphore-danger/10 text-semaphore-danger' : 'bg-semaphore-safe/10 text-semaphore-safe']">
              <svg v-if="factor.isRisk" xmlns="http://www.w3.org/2000/svg" class="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <circle cx="12" cy="12" r="10"/>
                <line x1="15" y1="9" x2="9" y2="15"/>
                <line x1="9" y1="9" x2="15" y2="15"/>
              </svg>
              <svg v-else xmlns="http://www.w3.org/2000/svg" class="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <polyline points="20 6 9 17 4 12"/>
              </svg>
            </div>
            
            <div class="space-y-1">
              <h4 class="font-bold text-sm text-text-primary">{{ factor.name }}</h4>
              <p class="text-xs text-text-secondary leading-relaxed">{{ factor.description }}</p>
            </div>
          </div>
        </div>
      </div>

      <ProposalWidget :jobText="jobText" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted } from 'vue';
import ProposalWidget from './ProposalWidget.vue';
import { useScanner } from '../composables/useScanner';

const { scanResult, jobText, error, loadScanDetails, badgeClasses } = useScanner();

onMounted(() => {
  loadScanDetails();
});
</script>
