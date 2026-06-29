import { describe, it, expect, vi, beforeEach } from 'vitest';
import { mount } from '@vue/test-utils';
import ProposalWidget from '../../src/components/ProposalWidget.vue';
import { ref } from 'vue';
import * as apiService from '../../src/services/apiService';

const mockUser = ref<any>(null);
const mockAuthLoading = ref(false);
const mockSignInWithGoogle = vi.fn();
let mockApiUrl = 'https://api.pharos-production.com';

vi.mock('../../src/config', () => ({
  get API_BASE_URL() { return mockApiUrl; },
  LEMON_SQUEEZY_CHECKOUT_URL: 'https://pharos-ai.lemonsqueezy.com/buy/variant-id',
  STORAGE_KEYS: {
    SCAN_RESULT: 'scanResult',
    SCANNED_JOB_TEXT: 'scannedJobText'
  }
}));

vi.mock('../../src/composables/useAuth', () => ({
  useAuth: () => ({
    user: mockUser,
    authLoading: mockAuthLoading,
    signInWithGoogle: mockSignInWithGoogle,
    logout: vi.fn()
  })
}));

vi.mock('../../src/services/apiService', () => ({
  fetchUserProfile: vi.fn().mockResolvedValue({ credits: 5, cvText: 'My CV' }),
  saveUserCv: vi.fn().mockResolvedValue({ message: 'CV saved' }),
  generateProposal: vi.fn().mockResolvedValue({ proposal: 'Cover Letter text' }),
  sendMockWebhook: vi.fn().mockResolvedValue({ message: 'Success' }),
  fetchUserTransactions: vi.fn().mockResolvedValue([
    {
      id: 'tx_1',
      createdAt: '2026-06-29T12:00:00Z',
      reason: 'welcome_balance',
      creditsChanged: 3,
      usage: null
    },
    {
      id: 'tx_2',
      createdAt: '2026-06-29T12:10:00Z',
      reason: 'proposal_generation',
      creditsChanged: -1,
      usage: {
        promptTokens: 100,
        completionTokens: 50,
        dbReads: 2,
        dbWrites: 2
      }
    }
  ])
}));

describe('ProposalWidget.vue', () => {
  beforeEach(() => {
    mockUser.value = null;
    mockAuthLoading.value = false;
    mockApiUrl = 'https://api.pharos-production.com';
    vi.clearAllMocks();
  });

  it('renders sign in button when logged out and triggers sign in', async () => {
    const wrapper = mount(ProposalWidget, {
      props: { jobText: 'Software Engineer Job vacancy details' }
    });
    await new Promise((resolve) => setTimeout(resolve, 50));
    await wrapper.vm.$nextTick();

    expect(wrapper.text()).toContain('Sign In with Google');
    
    const signInBtn = wrapper.find('button');
    await signInBtn.trigger('click');
    expect(mockSignInWithGoogle).toHaveBeenCalled();
  });

  it('renders credit balance and handles CV save & proposal generation', async () => {
    mockUser.value = {
      email: 'test@example.com',
      getIdToken: () => Promise.resolve('mock_token'),
      uid: 'uid123'
    };

    const wrapper = mount(ProposalWidget, {
      props: { jobText: 'Software Engineer Job vacancy details' }
    });

    // Wait for watchers to run and profile load
    await new Promise((resolve) => setTimeout(resolve, 50));
    await wrapper.vm.$nextTick();

    expect(wrapper.text()).toContain('test@example.com');
    expect(wrapper.text()).toContain('5 left');

    // Save CV
    const buttons = wrapper.findAll('button');
    const saveButton = buttons.find(b => b.text().includes('Save CV'));
    expect(saveButton).toBeDefined();
    await saveButton?.trigger('click');
    expect(apiService.saveUserCv).toHaveBeenCalled();

    // Generate Proposal
    const generateButton = buttons.find(b => b.text().includes('Generate Cover Letter'));
    expect(generateButton).toBeDefined();
    await generateButton?.trigger('click');
    expect(apiService.generateProposal).toHaveBeenCalled();
  });

  it('handles copying to clipboard', async () => {
    mockUser.value = {
      email: 'test@example.com',
      getIdToken: () => Promise.resolve('mock_token'),
      uid: 'uid123'
    };

    const wrapper = mount(ProposalWidget, {
      props: { jobText: 'Software Engineer Job vacancy details' }
    });

    await new Promise((resolve) => setTimeout(resolve, 50));
    await wrapper.vm.$nextTick();

    // Trigger generate
    const buttons = wrapper.findAll('button');
    const generateButton = buttons.find(b => b.text().includes('Generate Cover Letter'));
    await generateButton?.trigger('click');
    await wrapper.vm.$nextTick();

    // Mock clipboard API
    const writeTextMock = vi.fn().mockResolvedValue({});
    vi.stubGlobal('navigator', { clipboard: { writeText: writeTextMock } });

    const copyButton = wrapper.findAll('button').find(b => b.text().includes('Copy'));
    expect(copyButton).toBeDefined();
    await copyButton?.trigger('click');
    expect(writeTextMock).toHaveBeenCalledWith('Cover Letter text');
  });

  it('handles buying credits via window.open if LemonSqueezy SDK is absent', async () => {
    mockUser.value = {
      email: 'test@example.com',
      getIdToken: () => Promise.resolve('mock_token'),
      uid: 'uid123'
    };

    const wrapper = mount(ProposalWidget, {
      props: { jobText: 'Software Engineer Job vacancy details' }
    });

    await new Promise((resolve) => setTimeout(resolve, 50));
    await wrapper.vm.$nextTick();

    // Ensure LemonSqueezy is undefined
    vi.stubGlobal('LemonSqueezy', undefined);
    const openMock = vi.fn();
    vi.stubGlobal('open', openMock);

    const buttons = wrapper.findAll('button');
    const buyButton = buttons.find(b => b.text().includes('Buy Credits'));
    expect(buyButton).toBeDefined();
    await buyButton?.trigger('click');
    await new Promise((resolve) => setTimeout(resolve, 50));
    expect(openMock).toHaveBeenCalled();
  });

  it('handles buying credits via LemonSqueezy SDK if available', async () => {
    mockUser.value = {
      email: 'test@example.com',
      getIdToken: () => Promise.resolve('mock_token'),
      uid: 'uid123'
    };

    const wrapper = mount(ProposalWidget, {
      props: { jobText: 'Software Engineer Job vacancy details' }
    });

    await new Promise((resolve) => setTimeout(resolve, 50));
    await wrapper.vm.$nextTick();

    const openMock = vi.fn();
    vi.stubGlobal('LemonSqueezy', { Url: { Open: openMock } });

    const buttons = wrapper.findAll('button');
    const buyButton = buttons.find(b => b.text().includes('Buy Credits'));
    expect(buyButton).toBeDefined();
    await buyButton?.trigger('click');
    await new Promise((resolve) => setTimeout(resolve, 50));
    expect(openMock).toHaveBeenCalled();
  });

  it('handles sign in failures gracefully', async () => {
    mockSignInWithGoogle.mockRejectedValue(new Error('Google sign in canceled'));
    const wrapper = mount(ProposalWidget, {
      props: { jobText: 'Job details' }
    });
    await new Promise((resolve) => setTimeout(resolve, 50));
    await wrapper.vm.$nextTick();

    const signInBtn = wrapper.find('button');
    await signInBtn.trigger('click');
    await wrapper.vm.$nextTick();

    expect(wrapper.text()).toContain('Sign in failed');
  });

  it('handles CV save failures gracefully', async () => {
    mockUser.value = {
      email: 'test@example.com',
      getIdToken: () => Promise.resolve('mock_token'),
      uid: 'uid123'
    };

    vi.spyOn(apiService, 'saveUserCv').mockRejectedValue(new Error('Database offline'));

    const wrapper = mount(ProposalWidget, {
      props: { jobText: 'Job details' }
    });

    await new Promise((resolve) => setTimeout(resolve, 50));
    await wrapper.vm.$nextTick();

    const buttons = wrapper.findAll('button');
    const saveButton = buttons.find(b => b.text().includes('Save CV'));
    await saveButton?.trigger('click');
    await wrapper.vm.$nextTick();

    expect(wrapper.text()).toContain('Database offline');
  });

  it('handles generate proposal failures gracefully', async () => {
    mockUser.value = {
      email: 'test@example.com',
      getIdToken: () => Promise.resolve('mock_token'),
      uid: 'uid123'
    };

    vi.spyOn(apiService, 'generateProposal').mockRejectedValue(new Error('AI generation timed out'));

    const wrapper = mount(ProposalWidget, {
      props: { jobText: 'Job details' }
    });

    await new Promise((resolve) => setTimeout(resolve, 50));
    await wrapper.vm.$nextTick();

    const buttons = wrapper.findAll('button');
    const generateButton = buttons.find(b => b.text().includes('Generate Cover Letter'));
    await generateButton?.trigger('click');
    await wrapper.vm.$nextTick();

    expect(wrapper.text()).toContain('AI generation timed out');
  });

  it('simulates buying credits in development mode', async () => {
    mockUser.value = {
      email: 'test@example.com',
      getIdToken: () => Promise.resolve('mock_token'),
      uid: 'uid123'
    };

    const wrapper = mount(ProposalWidget, {
      props: { jobText: 'Software Engineer Job vacancy details' }
    });

    await new Promise((resolve) => setTimeout(resolve, 50));
    await wrapper.vm.$nextTick();

    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve({ message: 'Success' })
    });
    vi.stubGlobal('fetch', fetchMock);

    // Mock API URL to target localhost
    mockApiUrl = 'http://localhost:5295';

    const buyButton = wrapper.findAll('button').find(b => b.text().includes('Buy Credits'));
    expect(buyButton).toBeDefined();
    await buyButton?.trigger('click');
    await new Promise((resolve) => setTimeout(resolve, 50));
    expect(apiService.sendMockWebhook).toHaveBeenCalled();
  });
});
