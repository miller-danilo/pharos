<template>
  <div class="space-y-8">
    <div class="flex items-center justify-between">
      <h2 class="text-3xl font-extrabold tracking-tight text-gradient">Panel de Control de Costos</h2>
      <a href="/" class="text-xs text-brand-gradient-start hover:underline">← Volver al Escáner</a>
    </div>

    <div v-if="authLoading" class="flex justify-center py-12">
      <svg class="animate-spin h-8 w-8 text-brand-primary" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
        <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
        <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
      </svg>
    </div>

    <div v-else-if="!user" class="glass-panel p-8 text-center space-y-4 max-w-md mx-auto">
      <p class="text-text-secondary text-sm">Debes iniciar sesión con una cuenta autorizada para acceder a este panel.</p>
      <button
        @click="signInWithGoogle"
        class="px-6 h-12 bg-white text-bg-deep font-semibold rounded-lg shadow-md hover:bg-white/90 transition-colors flex items-center justify-center gap-2 mx-auto cursor-pointer"
      >
        <span>Sign In with Google</span>
      </button>
    </div>

    <div v-else class="grid grid-cols-1 lg:grid-cols-3 gap-8">
      <!-- Settings Column -->
      <div class="glass-panel p-6 space-y-6 lg:col-span-1">
        <h3 class="text-lg font-bold text-text-primary border-b border-white/5 pb-3">Tuning de Costos (Unit Rates)</h3>
        
        <div class="space-y-4">
          <div class="space-y-2">
            <label class="text-xs text-text-secondary font-medium block">Costo Gemini Input (por 1M tokens)</label>
            <div class="flex gap-2 items-center">
              <span class="text-xs text-text-secondary">$</span>
              <input v-model.number="inputRate" type="number" step="0.001" class="w-full bg-bg-deep/50 border border-white/10 rounded p-2 text-xs text-text-primary" />
            </div>
          </div>

          <div class="space-y-2">
            <label class="text-xs text-text-secondary font-medium block">Costo Gemini Output (por 1M tokens)</label>
            <div class="flex gap-2 items-center">
              <span class="text-xs text-text-secondary">$</span>
              <input v-model.number="outputRate" type="number" step="0.001" class="w-full bg-bg-deep/50 border border-white/10 rounded p-2 text-xs text-text-primary" />
            </div>
          </div>

          <div class="space-y-2">
            <label class="text-xs text-text-secondary font-medium block">Costo Firestore Reads (por 100K)</label>
            <div class="flex gap-2 items-center">
              <span class="text-xs text-text-secondary">$</span>
              <input v-model.number="readRate" type="number" step="0.001" class="w-full bg-bg-deep/50 border border-white/10 rounded p-2 text-xs text-text-primary" />
            </div>
          </div>

          <div class="space-y-2">
            <label class="text-xs text-text-secondary font-medium block">Costo Firestore Writes (por 100K)</label>
            <div class="flex gap-2 items-center">
              <span class="text-xs text-text-secondary">$</span>
              <input v-model.number="writeRate" type="number" step="0.001" class="w-full bg-bg-deep/50 border border-white/10 rounded p-2 text-xs text-text-primary" />
            </div>
          </div>

          <button
            @click="handleSaveMultipliers"
            :disabled="actionLoading"
            class="w-full h-10 bg-brand-primary hover:bg-brand-primary/90 text-white text-xs font-semibold rounded transition-all cursor-pointer glow-primary mt-2"
          >
            {{ actionLoading ? 'Guardando...' : 'Guardar Parámetros' }}
          </button>
          
          <p v-if="saveMessage" class="text-xs text-semaphore-safe text-center font-medium">{{ saveMessage }}</p>
        </div>
      </div>

      <!-- Margin Metrics Column -->
      <div class="lg:col-span-2 space-y-8">
        <div class="grid grid-cols-1 sm:grid-cols-3 gap-6">
          <div class="glass-panel p-5 space-y-1">
            <p class="text-xs text-text-secondary">Ingresos Totales (Créditos)</p>
            <p class="text-2xl font-bold text-gradient">${{ totalRevenue.toFixed(2) }} USD</p>
          </div>
          <div class="glass-panel p-5 space-y-1">
            <p class="text-xs text-text-secondary">Costo Estimado Infra</p>
            <p class="text-2xl font-bold text-semaphore-danger">${{ totalInfrastructureCost.toFixed(4) }} USD</p>
          </div>
          <div class="glass-panel p-5 space-y-1">
            <p class="text-xs text-text-secondary">Margen Neto Operativo</p>
            <p class="text-2xl font-bold text-semaphore-safe">{{ marginPercentage }}%</p>
          </div>
        </div>

        <!-- Resource Breakdown -->
        <div class="glass-panel p-6 space-y-4">
          <h3 class="text-lg font-bold text-text-primary border-b border-white/5 pb-3">Consumo Total de Recursos</h3>
          <div class="grid grid-cols-2 sm:grid-cols-4 gap-6 text-center">
            <div class="p-3 bg-bg-deep/20 rounded">
              <p class="text-xs text-text-secondary">Gemini Input Tokens</p>
              <p class="text-lg font-semibold text-text-primary mt-1">{{ totalInputTokens.toLocaleString() }}</p>
            </div>
            <div class="p-3 bg-bg-deep/20 rounded">
              <p class="text-xs text-text-secondary">Gemini Output Tokens</p>
              <p class="text-lg font-semibold text-text-primary mt-1">{{ totalOutputTokens.toLocaleString() }}</p>
            </div>
            <div class="p-3 bg-bg-deep/20 rounded">
              <p class="text-xs text-text-secondary">Firestore Reads</p>
              <p class="text-lg font-semibold text-text-primary mt-1">{{ totalReads.toLocaleString() }}</p>
            </div>
            <div class="p-3 bg-bg-deep/20 rounded">
              <p class="text-xs text-text-secondary">Firestore Writes</p>
              <p class="text-lg font-semibold text-text-primary mt-1">{{ totalWrites.toLocaleString() }}</p>
            </div>
          </div>
        </div>

        <!-- System Audit Log -->
        <div class="glass-panel p-6 space-y-4">
          <h3 class="text-lg font-bold text-text-primary border-b border-white/5 pb-3">Logs Globales de Transacciones</h3>
          <div class="overflow-x-auto max-h-60 overflow-y-auto">
            <table class="w-full text-left text-xs text-text-secondary border-collapse">
              <thead>
                <tr class="border-b border-white/10">
                  <th class="py-2 pr-4 text-text-primary">Usuario</th>
                  <th class="py-2 pr-4 text-text-primary">Acción</th>
                  <th class="py-2 pr-4 text-text-primary">Tokens</th>
                  <th class="py-2 text-text-primary">Costo Est.</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="t in allTransactions" :key="t.id" class="border-b border-white/5 hover:bg-white/5">
                  <td class="py-2 pr-4 font-mono text-[10px]">{{ t.userId.substring(0, 8) }}...</td>
                  <td class="py-2 pr-4 capitalize">{{ t.reason.replace('payment_purchase:', 'Compra ').replace('job_safety_scan', 'Safety Scan').replace('proposal_generation', 'Cover Letter') }}</td>
                  <td class="py-2 pr-4 font-mono text-[10px]">
                    <span v-if="t.usage">In: {{ t.usage.promptTokens }} | Out: {{ t.usage.completionTokens }}</span>
                    <span v-else>-</span>
                  </td>
                  <td class="py-2 font-mono text-[10px] text-text-primary">
                    ${{ calculateSingleCost(t).toFixed(5) }}
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed, watch } from 'vue';
import { useAuth } from '../composables/useAuth';
import { fetchCostMultipliers, saveCostMultipliers, fetchUserTransactions } from '../services/apiService';

const { user, authLoading, signInWithGoogle } = useAuth();

const inputRate = ref(0.075);  // per 1M tokens
const outputRate = ref(0.300); // per 1M tokens
const readRate = ref(0.060);   // per 100k
const writeRate = ref(0.180);  // per 100k

const allTransactions = ref([]);
const actionLoading = ref(false);
const saveMessage = ref('');

const loadCostConfig = async () => {
  if (!user.value) return;
  try {
    const token = await user.value.getIdToken();
    const multipliers = await fetchCostMultipliers(token);
    inputRate.value = multipliers.geminiInputTokenRate * 1000000;
    outputRate.value = multipliers.geminiOutputTokenRate * 1000000;
    readRate.value = multipliers.firestoreReadRate * 100000;
    writeRate.value = multipliers.firestoreWriteRate * 100000;
  } catch (err) {
    // Fail silently, fallbacks are active
  }
};

const loadAllTransactions = async () => {
  if (!user.value) return;
  try {
    const token = await user.value.getIdToken();
    allTransactions.value = await fetchUserTransactions(token);
  } catch (err) {
    // Fail silently
  }
};

watch(user, async (newUser) => {
  if (newUser) {
    await loadCostConfig();
    await loadAllTransactions();
  }
}, { immediate: true });

const handleSaveMultipliers = async () => {
  if (!user.value || actionLoading.value) return;
  saveMessage.value = '';
  actionLoading.value = true;

  try {
    const token = await user.value.getIdToken();
    const payload = {
      geminiInputTokenRate: inputRate.value / 1000000,
      geminiOutputTokenRate: outputRate.value / 1000000,
      firestoreReadRate: readRate.value / 100000,
      firestoreWriteRate: writeRate.value / 100000
    };
    await saveCostMultipliers(token, payload);
    saveMessage.value = '✓ Parámetros guardados con éxito';
    setTimeout(() => {
      saveMessage.value = '';
    }, 3000);
  } catch (err) {
    saveMessage.value = 'Error al guardar: ' + err.message;
  } finally {
    actionLoading.value = false;
  }
};

// Computes
const totalInputTokens = computed(() => {
  return allTransactions.value.reduce((sum, t) => sum + (t.usage?.promptTokens || 0), 0);
});

const totalOutputTokens = computed(() => {
  return allTransactions.value.reduce((sum, t) => sum + (t.usage?.completionTokens || 0), 0);
});

const totalReads = computed(() => {
  return allTransactions.value.reduce((sum, t) => sum + (t.usage?.dbReads || 0), 0);
});

const totalWrites = computed(() => {
  return allTransactions.value.reduce((sum, t) => sum + (t.usage?.dbWrites || 0), 0);
});

const totalRevenue = computed(() => {
  // Aggregate credits purchased in payment logs.
  // Assuming $0.599 USD per credit.
  const purchasedCredits = allTransactions.value.reduce((sum, t) => {
    return sum + (t.reason.startsWith('payment_purchase:') ? t.creditsChanged : 0);
  }, 0);
  return purchasedCredits * 0.599;
});

const totalInfrastructureCost = computed(() => {
  const inputCost = totalInputTokens.value * (inputRate.value / 1000000);
  const outputCost = totalOutputTokens.value * (outputRate.value / 1000000);
  const readCost = totalReads.value * (readRate.value / 100000);
  const writeCost = totalWrites.value * (writeRate.value / 100000);
  return inputCost + outputCost + readCost + writeCost;
});

const marginPercentage = computed(() => {
  if (totalRevenue.value === 0) return '0.00';
  const margin = ((totalRevenue.value - totalInfrastructureCost.value) / totalRevenue.value) * 100;
  return margin.toFixed(2);
});

const calculateSingleCost = (t) => {
  if (!t.usage) return 0;
  const inputCost = t.usage.promptTokens * (inputRate.value / 1000000);
  const outputCost = t.usage.completionTokens * (outputRate.value / 1000000);
  const readCost = t.usage.dbReads * (readRate.value / 100000);
  const writeCost = t.usage.dbWrites * (writeRate.value / 100000);
  return inputCost + outputCost + readCost + writeCost;
};
</script>
