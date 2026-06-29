import { ref } from 'vue';
import { fetchUserTransactions, type TransactionDto } from '../services/apiService';

export function useTransactions() {
  const transactions = ref<TransactionDto[]>([]);
  const loading = ref(false);
  const error = ref('');

  const loadTransactions = async (token: string) => {
    loading.value = true;
    error.value = '';
    try {
      transactions.value = await fetchUserTransactions(token);
    } catch (err) {
      const e = err as Error;
      // Do not swallow errors silently in new clean code, provide logging and user feedback
      error.value = e.message || 'Failed to load transaction history.';
      console.error(err);
    } finally {
      loading.value = false;
    }
  };

  const formatReason = (reason: string): string => {
    if (reason.startsWith('payment_purchase:')) {
      return 'Compra de Créditos';
    }
    if (reason === 'job_safety_scan') {
      return 'Escaneo de Seguridad';
    }
    if (reason === 'welcome_balance') {
      return 'Cortesía de Registro';
    }
    if (reason === 'proposal_generation') {
      return 'Generación de Propuesta';
    }
    if (reason === 'proposal_generation_refund') {
      return 'Reembolso por Fallo';
    }
    return reason;
  };

  return {
    transactions,
    loading,
    error,
    loadTransactions,
    formatReason
  };
}
