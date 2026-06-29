<template>
  <div class="mt-8 pt-6 border-t border-white/5 space-y-4">
    <h4 class="text-sm font-semibold text-text-primary">Historial de Transacciones / Uso</h4>
    <div class="glass-panel p-4 overflow-x-auto">
      <table class="w-full text-left text-xs text-text-secondary border-collapse">
        <thead>
          <tr class="border-b border-white/10">
            <th class="py-2 pr-4 text-text-primary">Fecha</th>
            <th class="py-2 pr-4 text-text-primary">Detalle</th>
            <th class="py-2 pr-4 text-text-primary">Créditos</th>
            <th class="py-2 text-text-primary">Uso Detallado</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="t in transactions" :key="t.id" class="border-b border-white/5 hover:bg-white/5">
            <td class="py-2 pr-4 font-mono">{{ new Date(t.createdAt).toLocaleDateString() }}</td>
            <td class="py-2 pr-4 capitalize">
              {{ formatReason(t.reason) }}
            </td>
            <td :class="['py-2 pr-4 font-bold', t.creditsChanged > 0 ? 'text-semaphore-safe' : t.creditsChanged < 0 ? 'text-semaphore-danger' : 'text-text-secondary']">
              {{ t.creditsChanged > 0 ? '+' : '' }}{{ t.creditsChanged }}
            </td>
            <td class="py-2 font-mono text-[10px] text-text-secondary/70">
              <span v-if="t.usage">
                Tokens: {{ t.usage.promptTokens + t.usage.completionTokens }} (In: {{ t.usage.promptTokens }} | Out: {{ t.usage.completionTokens }}) | DB: R{{ t.usage.dbReads }} W{{ t.usage.dbWrites }}
              </span>
              <span v-else>-</span>
            </td>
          </tr>
          <tr v-if="transactions.length === 0">
            <td colspan="4" class="py-4 text-center text-text-secondary/60">No se encontraron registros de transacciones.</td>
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
}>();
</script>
