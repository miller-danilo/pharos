<template>
  <div class="w-full max-w-5xl mx-auto space-y-8 relative overflow-hidden">
    <!-- Glow Decorators -->
    <div class="absolute -top-20 -left-20 w-80 h-80 bg-brand-primary/5 rounded-full blur-3xl"></div>
    <div class="absolute top-40 -right-20 w-80 h-80 bg-brand-gradient-end/5 rounded-full blur-3xl"></div>

    <div class="flex items-center justify-between pb-4 border-b border-white/5">
      <div>
        <h2 class="text-3xl font-extrabold text-gradient">Admin Settings & Telemetry</h2>
        <p class="text-xs text-text-secondary">Track margins, infrastructure costs, and config multipliers.</p>
      </div>

      <div v-if="user" class="text-right">
        <p class="text-xs text-text-secondary font-semibold">Logged as administrator</p>
        <p class="text-sm font-bold text-text-primary">{{ user.email }}</p>
      </div>
    </div>

    <!-- Auth Check / Spinner -->
    <div v-if="!isMounted || authLoading" class="flex justify-center py-16">
      <LoadingSpinner message="Checking administrator privileges..." />
    </div>

    <div v-else-if="!user" class="glass-panel p-12 text-center space-y-4">
      <p class="text-text-secondary text-sm">Please authenticate to access the administrator panel.</p>
      <button
        @click="signInWithGoogle"
        class="px-6 h-12 bg-white text-bg-deep font-semibold rounded-lg shadow-md hover:bg-white/90 transition-colors flex items-center justify-center gap-2 mx-auto cursor-pointer"
      >
        <svg class="w-5 h-5" viewBox="0 0 24 24">
          <path fill="#EA4335" d="M12.24 10.285V14.4h6.887c-.648 2.41-2.519 4.09-5.142 4.09-3.218 0-5.832-2.614-5.832-5.832s2.614-5.832 5.832-5.832c1.393 0 2.67.491 3.67 1.304l3.125-3.125C18.884 1.83 15.79 0 12.24 0c-5.922 0-11.24 4.92-11.24 11.24s4.922 11.24 11.24 11.24c6.545 0 11.24-4.7 11.24-11.24 0-.764-.09-1.3-.24-1.85H12.24z"/>
        </svg>
        <span>Sign In with Google</span>
      </button>
      <p v-if="error" class="text-sm text-semaphore-danger text-center font-medium">{{ error }}</p>
    </div>

    <!-- Main Admin Panel -->
    <div v-else class="space-y-8">
      <!-- Error Panel -->
      <p v-if="error" class="text-sm text-semaphore-danger text-center font-medium">{{ error }}</p>

      <MarginAnalytics
        :revenue="totalRevenue"
        :infraCost="totalInfrastructureCost"
        :margin="marginPercentage"
      />

      <div class="grid grid-cols-1 gap-8">
        <CostConfigForm
          v-model:inputRate="inputRate"
          v-model:outputRate="outputRate"
          v-model:readRate="readRate"
          v-model:writeRate="writeRate"
          :loading="actionLoading"
          :saveMessage="saveMessage"
          @save="triggerSaveMultipliers"
        />

        <AdminTransactionTable
          :transactions="transactions"
          :formatReason="formatReason"
          :calculateSingleCost="calculateSingleCost"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { watch, ref, onMounted } from 'vue';
import { useAuth } from '../composables/useAuth';
import { useTransactions } from '../composables/useTransactions';
import { useCostCalculator } from '../composables/useCostCalculator';
import LoadingSpinner from './LoadingSpinner.vue';
import CostConfigForm from './CostConfigForm.vue';
import MarginAnalytics from './MarginAnalytics.vue';
import AdminTransactionTable from './AdminTransactionTable.vue';

const { user, authLoading, signInWithGoogle } = useAuth();
const { transactions, loading: txnLoading, error: txnError, loadTransactions, formatReason } = useTransactions();
const {
  inputRate,
  outputRate,
  readRate,
  writeRate,
  actionLoading,
  saveMessage,
  error: costError,
  loadCostConfig,
  handleSaveMultipliers,
  totalRevenue,
  totalInfrastructureCost,
  marginPercentage,
  calculateSingleCost
} = useCostCalculator(transactions);

const isMounted = ref(false);
const error = ref('');

onMounted(() => {
  isMounted.value = true;
});

watch(user, async (newUser) => {
  error.value = '';
  if (newUser) {
    const token = await newUser.getIdToken();
    await loadCostConfig(token);
    await loadTransactions(token);
  } else {
    transactions.value = [];
  }
}, { immediate: true });

watch([txnError, costError], ([newTxnErr, newCostErr]) => {
  if (newTxnErr) error.value = newTxnErr;
  else if (newCostErr) error.value = newCostErr;
});

const triggerSaveMultipliers = async () => {
  if (!user.value) return;
  const token = await user.value.getIdToken();
  await handleSaveMultipliers(token);
};
</script>
