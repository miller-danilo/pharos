import { describe, it, expect, vi, beforeEach } from 'vitest';
import { useScanner } from '../../src/composables/useScanner';
import * as apiService from '../../src/services/apiService';
import { STORAGE_KEYS } from '../../src/config';

vi.mock('../../src/services/apiService', () => ({
  analyzeScan: vi.fn()
}));

describe('useScanner composable', () => {
  beforeEach(() => {
    sessionStorage.clear();
    vi.clearAllMocks();
  });

  it('initializes with default state', () => {
    const { jobText, selectedFile, loading, dragOver, error, scanResult } = useScanner();
    expect(jobText.value).toBe('');
    expect(selectedFile.value).toBeNull();
    expect(loading.value).toBe(false);
    expect(dragOver.value).toBe(false);
    expect(error.value).toBe('');
    expect(scanResult.value).toBeNull();
  });

  it('loads scan details from sessionStorage', () => {
    const mockResult = { riskLevel: 'GREEN', summary: 'Safe Job', reasoning: 'Ok', factors: [] };
    sessionStorage.setItem(STORAGE_KEYS.SCAN_RESULT, JSON.stringify(mockResult));
    sessionStorage.setItem(STORAGE_KEYS.SCANNED_JOB_TEXT, 'Scanned vacancy content');

    const { loadScanDetails, scanResult, jobText } = useScanner();
    loadScanDetails();

    expect(scanResult.value).toEqual(mockResult);
    expect(jobText.value).toBe('Scanned vacancy content');
  });

  it('handles file change events', () => {
    const { handleFileChange, selectedFile } = useScanner();
    const file = new File(['fake content'], 'test.pdf', { type: 'application/pdf' });
    const mockEvent = {
      target: {
        files: [file]
      }
    } as any;

    handleFileChange(mockEvent);
    expect(selectedFile.value).toBe(file);
  });

  it('handles file drop events', () => {
    const { handleDrop, selectedFile, dragOver } = useScanner();
    const file = new File(['fake content'], 'test.png', { type: 'image/png' });
    const mockEvent = {
      dataTransfer: {
        files: [file]
      }
    } as any;

    handleDrop(mockEvent);
    expect(selectedFile.value).toBe(file);
    expect(dragOver.value).toBe(false);
  });

  it('performs scan and updates state & sessionStorage', async () => {
    const mockResult = { riskLevel: 'YELLOW', summary: 'Suspicious Job', reasoning: 'None', factors: [] };
    const scanSpy = vi.spyOn(apiService, 'analyzeScan').mockResolvedValue(mockResult as any);

    const { performScan, jobText, scanResult, loading } = useScanner();
    jobText.value = 'Job description text';

    const promise = performScan();
    expect(loading.value).toBe(true);
    await promise;

    expect(loading.value).toBe(false);
    expect(scanResult.value).toEqual(mockResult);
    expect(scanSpy).toHaveBeenCalled();

    expect(JSON.parse(sessionStorage.getItem(STORAGE_KEYS.SCAN_RESULT) || '{}')).toEqual(mockResult);
    expect(sessionStorage.getItem(STORAGE_KEYS.SCANNED_JOB_TEXT)).toBe('Job description text');
  });

  it('sets error on scan failure', async () => {
    vi.spyOn(apiService, 'analyzeScan').mockRejectedValue(new Error('Rate limit exceeded'));

    const { performScan, error } = useScanner();
    await expect(performScan()).rejects.toThrow('Rate limit exceeded');
    expect(error.value).toBe('Rate limit exceeded');
  });

  it('computes badge classes based on riskLevel', () => {
    const { badgeClasses, scanResult } = useScanner();
    
    expect(badgeClasses.value).toBe('');

    scanResult.value = { riskLevel: 'GREEN' } as any;
    expect(badgeClasses.value).toContain('text-semaphore-safe');

    scanResult.value = { riskLevel: 'YELLOW' } as any;
    expect(badgeClasses.value).toContain('text-semaphore-warning');

    scanResult.value = { riskLevel: 'RED' } as any;
    expect(badgeClasses.value).toContain('text-semaphore-danger');
  });
});
