import { describe, it, expect, vi, beforeEach } from 'vitest';
import { usePayment } from '../../src/composables/usePayment';
import * as apiService from '../../src/services/apiService';
import * as config from '../../src/config';

vi.mock('../../src/services/apiService', () => ({
  sendMockWebhook: vi.fn()
}));

vi.mock('../../src/config', () => ({
  API_BASE_URL: 'https://api.pharos-production.com',
  LEMON_SQUEEZY_CHECKOUT_URL: 'https://checkout.url'
}));

describe('usePayment composable', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('sets error if no user ID provided', async () => {
    const { error, handleBuyCredits } = usePayment();
    await handleBuyCredits('', vi.fn());
    expect(error.value).toBe('Please sign in first before purchasing credits.');
  });

  it('opens LemonSqueezy checkout URL if not localhost', async () => {
    // Force production API URL
    vi.spyOn(config, 'API_BASE_URL', 'get').mockReturnValue('https://api.pharos-production.com');
    const openMock = vi.fn();
    vi.stubGlobal('open', openMock);
    vi.stubGlobal('LemonSqueezy', undefined);

    const { handleBuyCredits } = usePayment();
    await handleBuyCredits('uid123', vi.fn());

    expect(openMock).toHaveBeenCalledWith('https://checkout.url?checkout[custom][user_id]=uid123&embed=1', '_blank');
  });

  it('calls LemonSqueezy SDK if available', async () => {
    vi.spyOn(config, 'API_BASE_URL', 'get').mockReturnValue('https://api.pharos-production.com');
    const openSdkMock = vi.fn();
    vi.stubGlobal('LemonSqueezy', { Url: { Open: openSdkMock } });

    const { handleBuyCredits } = usePayment();
    await handleBuyCredits('uid123', vi.fn());

    expect(openSdkMock).toHaveBeenCalledWith('https://checkout.url?checkout[custom][user_id]=uid123&embed=1');
  });

  it('calls mock webhook in development mode (localhost)', async () => {
    vi.spyOn(config, 'API_BASE_URL', 'get').mockReturnValue('http://localhost:5295');
    const webhookSpy = vi.spyOn(apiService, 'sendMockWebhook').mockResolvedValue({ message: 'Success' });
    const refreshCallback = vi.fn();

    const { handleBuyCredits, loading } = usePayment();
    const promise = handleBuyCredits('uid123', refreshCallback);
    
    expect(loading.value).toBe(true);
    await promise;

    expect(loading.value).toBe(false);
    expect(webhookSpy).toHaveBeenCalledWith(expect.objectContaining({
      meta: expect.objectContaining({
        custom_data: { user_id: 'uid123' }
      })
    }));
    expect(refreshCallback).toHaveBeenCalled();
  });
});
