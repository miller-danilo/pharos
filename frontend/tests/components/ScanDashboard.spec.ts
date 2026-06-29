import { describe, it, expect, vi, beforeEach } from 'vitest';
import { mount } from '@vue/test-utils';
import ScanDashboard from '../../src/components/ScanDashboard.vue';
import { STORAGE_KEYS } from '../../src/config';

// Mock the child ProposalWidget to avoid rendering auth dependencies
vi.mock('../../src/components/ProposalWidget.vue', () => ({
  default: {
    name: 'ProposalWidget',
    template: '<div>Mocked Proposal Widget</div>',
    props: ['jobText']
  }
}));

describe('ScanDashboard.vue', () => {
  beforeEach(() => {
    sessionStorage.clear();
  });

  it('renders no active scan screen if no scan result is in storage', () => {
    const wrapper = mount(ScanDashboard);
    expect(wrapper.text()).toContain('No Active Scan Found');
    expect(wrapper.text()).toContain('Go to Scanner');
  });

  it('renders scan verdict, reasoning, and factors when result exists in storage', async () => {
    const mockResult = {
      riskLevel: 'YELLOW',
      summary: 'Potential job listing warning',
      reasoning: 'Hiring company lacks registered domain name.',
      factors: [
        { name: 'Domain name missing', description: 'Domain missing description', isRisk: true },
        { name: 'Valid Salary Range', description: 'Salary is standard', isRisk: false }
      ]
    };
    sessionStorage.setItem(STORAGE_KEYS.SCAN_RESULT, JSON.stringify(mockResult));
    sessionStorage.setItem(STORAGE_KEYS.SCANNED_JOB_TEXT, 'Job description text');

    const wrapper = mount(ScanDashboard);
    await wrapper.vm.$nextTick();

    expect(wrapper.text()).toContain('Scan Verdict');
    expect(wrapper.text()).toContain('Potential job listing warning');
    expect(wrapper.text()).toContain('YELLOW');
    expect(wrapper.text()).toContain('Hiring company lacks registered domain name.');
    
    // Check factors
    expect(wrapper.text()).toContain('Domain name missing');
    expect(wrapper.text()).toContain('Valid Salary Range');
  });
});
