import { ref, computed } from 'vue';
import { fetchCostMultipliers, saveCostMultipliers, type TransactionDto } from '../services/apiService';
import { TOKENS_PER_MILLION, OPS_PER_HUNDRED_K, CREDIT_PRICE_USD, UI_SAVE_FEEDBACK_MS } from '../constants';

export function useCostCalculator(allTransactions: { value: TransactionDto[] }) {
  const inputRate = ref(0.075);  // per 1M tokens
  const outputRate = ref(0.300); // per 1M tokens
  const readRate = ref(0.060);   // per 100k
  const writeRate = ref(0.180);  // per 100k

  const actionLoading = ref(false);
  const saveMessage = ref('');
  const error = ref('');

  const loadCostConfig = async (token: string) => {
    try {
      const multipliers = await fetchCostMultipliers(token);
      inputRate.value = multipliers.geminiInputTokenRate * TOKENS_PER_MILLION;
      outputRate.value = multipliers.geminiOutputTokenRate * TOKENS_PER_MILLION;
      readRate.value = multipliers.firestoreReadRate * OPS_PER_HUNDRED_K;
      writeRate.value = multipliers.firestoreWriteRate * OPS_PER_HUNDRED_K;
    } catch (err) {
      const e = err as Error;
      error.value = e.message || 'Failed to load cost multipliers.';
    }
  };

  const handleSaveMultipliers = async (token: string) => {
    if (actionLoading.value) return;
    saveMessage.value = '';
    error.value = '';
    actionLoading.value = true;

    try {
      const payload = {
        geminiInputTokenRate: inputRate.value / TOKENS_PER_MILLION,
        geminiOutputTokenRate: outputRate.value / TOKENS_PER_MILLION,
        firestoreReadRate: readRate.value / OPS_PER_HUNDRED_K,
        firestoreWriteRate: writeRate.value / OPS_PER_HUNDRED_K
      };
      await saveCostMultipliers(token, payload);
      saveMessage.value = '✓ Parámetros guardados con éxito';
      setTimeout(() => {
        saveMessage.value = '';
      }, UI_SAVE_FEEDBACK_MS);
    } catch (err) {
      const e = err as Error;
      error.value = 'Error al guardar: ' + e.message;
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
    const purchasedCredits = allTransactions.value.reduce((sum, t) => {
      return sum + (t.reason.startsWith('payment_purchase:') ? t.creditsChanged : 0);
    }, 0);
    return purchasedCredits * CREDIT_PRICE_USD;
  });

  const totalInfrastructureCost = computed(() => {
    const inputCost = totalInputTokens.value * (inputRate.value / TOKENS_PER_MILLION);
    const outputCost = totalOutputTokens.value * (outputRate.value / TOKENS_PER_MILLION);
    const readCost = totalReads.value * (readRate.value / OPS_PER_HUNDRED_K);
    const writeCost = totalWrites.value * (writeRate.value / OPS_PER_HUNDRED_K);
    return inputCost + outputCost + readCost + writeCost;
  });

  const marginPercentage = computed(() => {
    if (totalRevenue.value === 0) return '0.00';
    const margin = ((totalRevenue.value - totalInfrastructureCost.value) / totalRevenue.value) * 100;
    return margin.toFixed(2);
  });

  const calculateSingleCost = (t: TransactionDto): number => {
    if (!t.usage) return 0;
    const inputCost = t.usage.promptTokens * (inputRate.value / TOKENS_PER_MILLION);
    const outputCost = t.usage.completionTokens * (outputRate.value / TOKENS_PER_MILLION);
    const readCost = t.usage.dbReads * (readRate.value / OPS_PER_HUNDRED_K);
    const writeCost = t.usage.dbWrites * (writeRate.value / OPS_PER_HUNDRED_K);
    return inputCost + outputCost + readCost + writeCost;
  };

  return {
    inputRate,
    outputRate,
    readRate,
    writeRate,
    actionLoading,
    saveMessage,
    error,
    loadCostConfig,
    handleSaveMultipliers,
    totalInputTokens,
    totalOutputTokens,
    totalReads,
    totalWrites,
    totalRevenue,
    totalInfrastructureCost,
    marginPercentage,
    calculateSingleCost
  };
}
