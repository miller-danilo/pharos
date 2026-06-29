import { describe, it, expect, vi, beforeEach } from 'vitest';
import { useTransactions } from '../../src/composables/useTransactions';
import * as apiService from '../../src/services/apiService';

vi.mock('../../src/services/apiService', () => ({
  fetchUserTransactions: vi.fn()
}));

describe('useTransactions composable', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('initializes with empty state', () => {
    const { transactions, loading, error } = useTransactions();
    expect(transactions.value).toEqual([]);
    expect(loading.value).toBe(false);
    expect(error.value).toBe('');
  });

  it('loads transactions successfully', async () => {
    const mockTx = [{ id: 'tx_1', reason: 'welcome_balance', creditsChanged: 3 }];
    vi.spyOn(apiService, 'fetchUserTransactions').mockResolvedValue(mockTx as any);

    const { transactions, loading, loadTransactions } = useTransactions();
    
    const promise = loadTransactions('token');
    expect(loading.value).toBe(true);
    await promise;
    
    expect(loading.value).toBe(false);
    expect(transactions.value).toEqual(mockTx);
  });

  it('handles loading errors', async () => {
    vi.spyOn(apiService, 'fetchUserTransactions').mockRejectedValue(new Error('Network offline'));

    const { transactions, error, loadTransactions } = useTransactions();
    await loadTransactions('token');
    
    expect(transactions.value).toEqual([]);
    expect(error.value).toBe('Network offline');
  });

  it('formats transaction reasons correctly', () => {
    const { formatReason } = useTransactions();
    expect(formatReason('payment_purchase:123')).toBe('Compra de Créditos');
    expect(formatReason('job_safety_scan')).toBe('Escaneo de Seguridad');
    expect(formatReason('welcome_balance')).toBe('Cortesía de Registro');
    expect(formatReason('proposal_generation')).toBe('Generación de Propuesta');
    expect(formatReason('proposal_generation_refund')).toBe('Reembolso por Fallo');
    expect(formatReason('some_other_reason')).toBe('some_other_reason');
  });
});
