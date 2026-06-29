import { describe, it, expect, vi, beforeEach } from 'vitest';
import { ref } from 'vue';
import { useCostCalculator } from '../../src/composables/useCostCalculator';
import * as apiService from '../../src/services/apiService';

vi.mock('../../src/services/apiService', () => ({
  fetchCostMultipliers: vi.fn(),
  saveCostMultipliers: vi.fn()
}));

describe('useCostCalculator composable', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('calculates total infra cost, revenue, and margins correctly', () => {
    const transactions = ref<any[]>([
      {
        reason: 'payment_purchase:mock',
        creditsChanged: 10,
        usage: null
      },
      {
        reason: 'job_safety_scan',
        creditsChanged: 0,
        usage: {
          promptTokens: 1000000, // 1M
          completionTokens: 2000000, // 2M
          dbReads: 100000, // 100k
          dbWrites: 200000 // 200k
        }
      }
    ]);

    const {
      inputRate,
      outputRate,
      readRate,
      writeRate,
      totalRevenue,
      totalInfrastructureCost,
      marginPercentage,
      calculateSingleCost
    } = useCostCalculator(transactions);

    // Initial rates: inputRate = 0.075 per 1M, outputRate = 0.300 per 1M, readRate = 0.060 per 100k, writeRate = 0.180 per 100k
    expect(inputRate.value).toBe(0.075);
    expect(outputRate.value).toBe(0.300);
    expect(readRate.value).toBe(0.060);
    expect(writeRate.value).toBe(0.180);

    // Revenue: 10 credits * 0.599 credit price = $5.99
    expect(totalRevenue.value).toBeCloseTo(5.99, 2);

    // Infrastructure cost for job_safety_scan:
    // Input tokens cost: 1M * 0.075 / 1M = 0.075
    // Output tokens cost: 2M * 0.300 / 1M = 0.600
    // Reads cost: 100k * 0.060 / 100k = 0.060
    // Writes cost: 200k * 0.180 / 100k = 0.360
    // Total = 0.075 + 0.600 + 0.060 + 0.360 = 1.095
    expect(totalInfrastructureCost.value).toBeCloseTo(1.095, 4);

    // Margin: ((5.99 - 1.095) / 5.99) * 100 = 81.719% -> 81.72%
    expect(marginPercentage.value).toBe('81.72');

    // Single cost check
    const cost = calculateSingleCost(transactions.value[1]);
    expect(cost).toBeCloseTo(1.095, 4);
  });

  it('loads config from service', async () => {
    const mockMultipliers = {
      geminiInputTokenRate: 0.00000005, // 0.05 per 1M
      geminiOutputTokenRate: 0.0000002, // 0.20 per 1M
      firestoreReadRate: 0.0000005, // 0.05 per 100k
      firestoreWriteRate: 0.0000015 // 0.15 per 100k
    };
    vi.spyOn(apiService, 'fetchCostMultipliers').mockResolvedValue(mockMultipliers as any);

    const transactions = ref([]);
    const { loadCostConfig, inputRate, outputRate, readRate, writeRate } = useCostCalculator(transactions);
    
    await loadCostConfig('token');
    
    expect(inputRate.value).toBeCloseTo(0.05, 4);
    expect(outputRate.value).toBeCloseTo(0.20, 4);
    expect(readRate.value).toBeCloseTo(0.05, 4);
    expect(writeRate.value).toBeCloseTo(0.15, 4);
  });

  it('saves multipliers to service', async () => {
    const saveMock = vi.spyOn(apiService, 'saveCostMultipliers').mockResolvedValue({ message: 'Saved' });
    vi.useFakeTimers();

    const transactions = ref([]);
    const { handleSaveMultipliers, saveMessage, inputRate } = useCostCalculator(transactions);
    
    inputRate.value = 0.10; // per 1M tokens
    
    const promise = handleSaveMultipliers('token');
    await promise;
    
    expect(saveMock).toHaveBeenCalledWith('token', expect.objectContaining({
      geminiInputTokenRate: 0.10 / 1000000
    }));
    expect(saveMessage.value).toBe('✓ Parámetros guardados con éxito');

    vi.runAllTimers();
    expect(saveMessage.value).toBe('');
    vi.useRealTimers();
  });
});
