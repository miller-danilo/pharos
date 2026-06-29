<template>
  <div class="glass-panel p-6 space-y-4">
    <div class="flex items-center justify-between">
      <h3 class="text-sm font-bold text-text-primary">Global Transaction Usage & Infrastructure Cost Details</h3>
      <p class="text-xs text-text-secondary font-semibold">Total Records: {{ transactions.length }}</p>
    </div>

    <div class="overflow-x-auto">
      <table class="w-full text-left text-xs text-text-secondary border-collapse">
        <thead>
          <tr class="border-b border-white/10">
            <th class="py-2 pr-4 text-text-primary">User ID</th>
            <th class="py-2 pr-4 text-text-primary">Detalle</th>
            <th class="py-2 pr-4 text-text-primary">Fecha</th>
            <th class="py-2 pr-4 text-text-primary">Credits</th>
            <th class="py-2 pr-4 text-text-primary">Tokens / DB Reads / DB Writes</th>
            <th class="py-2 text-text-primary">Infra Cost</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="t in transactions" :key="t.id" class="border-b border-white/5 hover:bg-white/5 font-mono text-[11px]">
            <td class="py-2 pr-4 text-brand-gradient-start font-bold max-w-[120px] truncate" :title="t.userId">{{ t.userId }}</td>
            <td class="py-2 pr-4 capitalize text-text-primary font-sans">{{ formatReason(t.reason) }}</td>
            <td class="py-2 pr-4">{{ new Date(t.createdAt).toLocaleDateString() }}</td>
            <td :class="['py-2 pr-4 font-bold text-center', t.creditsChanged > 0 ? 'text-semaphore-safe' : t.creditsChanged < 0 ? 'text-semaphore-danger' : 'text-text-secondary']">
              {{ t.creditsChanged > 0 ? '+' : '' }}{{ t.creditsChanged }}
            </td>
            <td class="py-2 pr-4 text-text-secondary/80">
              <span v-if="t.usage">
                Tokens: {{ t.usage.promptTokens + t.usage.completionTokens }} ({{ t.usage.promptTokens }} | {{ t.usage.completionTokens }}) | DB: R{{ t.usage.dbReads }} W{{ t.usage.dbWrites }}
              </span>
              <span v-else class="text-text-secondary/40">No Telemetry</span>
            </td>
            <td class="py-2 font-bold text-text-primary">${{ calculateSingleCost(t).toFixed(4) }}</td>
          </tr>
          <tr v-if="transactions.length === 0">
            <td colspan="6" class="py-4 text-center text-text-secondary/60">No transaction logs available.</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { TransactionDto } from '../services/apiService';

defineProps<{
  transactions: TransactionDto[];
  formatReason: (reason: string) => string;
  calculateSingleCost: (t: TransactionDto) => number;
}>();
</script>
