import { describe, it, expect, vi } from 'vitest';
import { mount } from '@vue/test-utils';
import CostConfigForm from '../../src/components/CostConfigForm.vue';
import MarginAnalytics from '../../src/components/MarginAnalytics.vue';
import AdminTransactionTable from '../../src/components/AdminTransactionTable.vue';

describe('CostConfigForm.vue', () => {
  it('renders input rates and triggers emits', async () => {
    const wrapper = mount(CostConfigForm, {
      props: {
        inputRate: 0.05,
        outputRate: 0.15,
        readRate: 0.04,
        writeRate: 0.12,
        loading: false,
        saveMessage: 'Saved successfully'
      }
    });

    expect(wrapper.find('input[type="number"]').element.value).toBe('0.05');
    expect(wrapper.text()).toContain('Saved successfully');

    // Trigger input change
    const inputs = wrapper.findAll('input[type="number"]');
    await inputs[0].setValue(0.08);
    expect(wrapper.emitted('update:inputRate')?.[0]).toEqual([0.08]);

    // Trigger save form submit
    await wrapper.find('form').trigger('submit');
    expect(wrapper.emitted('save')).toBeDefined();
  });
});

describe('MarginAnalytics.vue', () => {
  it('renders revenue, costs, and margins correctly', () => {
    const wrapper = mount(MarginAnalytics, {
      props: {
        revenue: 100.50,
        infraCost: 40.25,
        margin: '59.95'
      }
    });

    expect(wrapper.text()).toContain('$100.50');
    expect(wrapper.text()).toContain('$40.2500');
    expect(wrapper.text()).toContain('+59.95%');
  });

  it('renders negative margin with red text class', () => {
    const wrapper = mount(MarginAnalytics, {
      props: {
        revenue: 10.00,
        infraCost: 25.00,
        margin: '-150.00'
      }
    });

    expect(wrapper.text()).toContain('-150.00%');
    const marginText = wrapper.find('.text-semaphore-danger');
    expect(marginText.exists()).toBe(true);
  });
});

describe('AdminTransactionTable.vue', () => {
  it('renders transaction details correctly', () => {
    const transactions = [
      {
        id: 'tx_admin_1',
        userId: 'user_A',
        reason: 'welcome_balance',
        creditsChanged: 3,
        createdAt: '2026-06-29T12:00:00Z',
        usage: null
      },
      {
        id: 'tx_admin_2',
        userId: 'user_B',
        reason: 'job_safety_scan',
        creditsChanged: -1,
        createdAt: '2026-06-29T12:05:00Z',
        usage: {
          promptTokens: 100,
          completionTokens: 200,
          dbReads: 5,
          dbWrites: 5
        }
      }
    ];

    const formatReasonMock = vi.fn().mockImplementation((r) => 'Formatted: ' + r);
    const calculateSingleCostMock = vi.fn().mockReturnValue(0.0045);

    const wrapper = mount(AdminTransactionTable, {
      props: {
        transactions,
        formatReason: formatReasonMock,
        calculateSingleCost: calculateSingleCostMock
      }
    });

    expect(wrapper.text()).toContain('user_A');
    expect(wrapper.text()).toContain('user_B');
    expect(formatReasonMock).toHaveBeenCalledWith('welcome_balance');
    expect(calculateSingleCostMock).toHaveBeenCalledWith(transactions[1]);
    expect(wrapper.text()).toContain('$0.0045');
  });

  it('renders empty list message when no logs available', () => {
    const wrapper = mount(AdminTransactionTable, {
      props: {
        transactions: [],
        formatReason: vi.fn(),
        calculateSingleCost: vi.fn()
      }
    });

    expect(wrapper.text()).toContain('No transaction logs available.');
  });
});
