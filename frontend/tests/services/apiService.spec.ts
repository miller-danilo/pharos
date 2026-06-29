import { describe, it, expect, vi, beforeEach } from 'vitest';
import {
  fetchUserProfile,
  saveUserCv,
  generateProposal,
  fetchUserTransactions,
  fetchCostMultipliers,
  saveCostMultipliers,
  analyzeScan,
  sendMockWebhook
} from '../../src/services/apiService';

describe('apiService.ts', () => {
  beforeEach(() => {
    vi.stubGlobal('fetch', vi.fn());
    vi.clearAllMocks();
  });

  it('fetchUserProfile calls fetch with Authorization header', async () => {
    const mockRes = { id: 'u1', email: 'test@example.com' };
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(mockRes)
    });
    vi.stubGlobal('fetch', fetchMock);

    const res = await fetchUserProfile('mock_token');
    expect(res).toEqual(mockRes);
    expect(fetchMock).toHaveBeenCalledWith(
      expect.stringContaining('/api/user/profile'),
      expect.objectContaining({
        headers: {
          'Authorization': 'Bearer mock_token'
        }
      })
    );
  });

  it('fetchUserProfile throws error if not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false }));
    await expect(fetchUserProfile('token')).rejects.toThrow('Failed to load profile.');
  });

  it('saveUserCv posts cv text', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve({ message: 'Success' })
    });
    vi.stubGlobal('fetch', fetchMock);

    const res = await saveUserCv('token', 'CV Content');
    expect(res.message).toBe('Success');
  });

  it('saveUserCv throws error if not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: false,
      text: () => Promise.resolve('Error saving')
    }));
    await expect(saveUserCv('token', 'cv')).rejects.toThrow('Error saving');
  });

  it('generateProposal generates letter', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve({ proposal: 'letter' })
    });
    vi.stubGlobal('fetch', fetchMock);

    const res = await generateProposal('token', 'job', 'cv');
    expect(res.proposal).toBe('letter');
  });

  it('generateProposal throws payment required error on 402', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      status: 402,
      ok: false
    }));
    await expect(generateProposal('token', 'job', 'cv')).rejects.toThrow('Insufficient credits. Please purchase more credits.');
  });

  it('fetchUserTransactions fetches transactions', async () => {
    const mockTx = [{ id: 'tx1' }];
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(mockTx)
    }));

    const res = await fetchUserTransactions('token');
    expect(res).toEqual(mockTx);
  });

  it('fetchCostMultipliers fetches multipliers', async () => {
    const multipliers = { geminiInputTokenRate: 0.05 };
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(multipliers)
    }));

    const res = await fetchCostMultipliers('token');
    expect(res).toEqual(multipliers);
  });

  it('saveCostMultipliers posts multipliers', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve({ message: 'Success' })
    }));

    const res = await saveCostMultipliers('token', {
      geminiInputTokenRate: 1,
      geminiOutputTokenRate: 2,
      firestoreReadRate: 3,
      firestoreWriteRate: 4
    });
    expect(res.message).toBe('Success');
  });

  it('analyzeScan uploads FormData', async () => {
    const mockScan = { riskLevel: 'GREEN' };
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(mockScan)
    }));

    const formData = new FormData();
    const res = await analyzeScan(formData);
    expect(res.riskLevel).toBe('GREEN');
  });

  it('sendMockWebhook sends payload', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve({ message: 'Success' })
    }));

    const res = await sendMockWebhook({ event: 'order' });
    expect(res.message).toBe('Success');
  });
});
