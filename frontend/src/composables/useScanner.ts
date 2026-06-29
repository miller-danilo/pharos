import { ref, computed } from 'vue';
import { STORAGE_KEYS } from '../config';
import { analyzeScan, type ScanResultDto } from '../services/apiService';

export function useScanner() {
  const jobText = ref('');
  const selectedFile = ref<File | null>(null);
  const loading = ref(false);
  const dragOver = ref(false);
  const error = ref('');
  const scanResult = ref<ScanResultDto | null>(null);

  const loadScanDetails = () => {
    try {
      const rawResult = sessionStorage.getItem(STORAGE_KEYS.SCAN_RESULT) || localStorage.getItem(STORAGE_KEYS.SCAN_RESULT);
      const rawText = sessionStorage.getItem(STORAGE_KEYS.SCANNED_JOB_TEXT) || localStorage.getItem(STORAGE_KEYS.SCANNED_JOB_TEXT);
      if (rawResult) {
        scanResult.value = JSON.parse(rawResult);
      }
      if (rawText) {
        jobText.value = rawText;
      }
    } catch (err) {
      const e = err as Error;
      error.value = 'Failed to load cached scan details: ' + e.message;
    }
  };

  const handleFileChange = (e: Event) => {
    const target = e.target as HTMLInputElement;
    if (target.files && target.files.length > 0) {
      selectedFile.value = target.files[0];
      error.value = '';
    }
  };

  const handleDrop = (e: DragEvent) => {
    dragOver.value = false;
    if (loading.value) return;
    
    if (e.dataTransfer?.files && e.dataTransfer.files.length > 0) {
      selectedFile.value = e.dataTransfer.files[0];
      error.value = '';
    }
  };

  const performScan = async () => {
    if (loading.value) return;
    error.value = '';
    loading.value = true;

    try {
      const formData = new FormData();
      if (selectedFile.value) {
        formData.append('file', selectedFile.value);
      }
      if (jobText.value) {
        formData.append('text', jobText.value);
      }

      const result = await analyzeScan(formData);
      
      scanResult.value = result;
      sessionStorage.setItem(STORAGE_KEYS.SCAN_RESULT, JSON.stringify(result));
      sessionStorage.setItem(STORAGE_KEYS.SCANNED_JOB_TEXT, jobText.value || selectedFile.value?.name || 'Scanned job file');

      return result;
    } catch (err) {
      const e = err as Error;
      error.value = e.message || 'An error occurred during scanning. Please try again.';
      throw e;
    } finally {
      loading.value = false;
    }
  };

  const badgeClasses = computed(() => {
    if (!scanResult.value) return '';
    const level = scanResult.value.riskLevel.toUpperCase();
    
    if (level === 'GREEN') {
      return 'border-semaphore-safe/25 bg-semaphore-safe/5 text-semaphore-safe glow-safe';
    } else if (level === 'YELLOW') {
      return 'border-semaphore-warning/25 bg-semaphore-warning/5 text-semaphore-warning glow-warning';
    } else {
      return 'border-semaphore-danger/25 bg-semaphore-danger/5 text-semaphore-danger glow-danger';
    }
  });

  return {
    jobText,
    selectedFile,
    loading,
    dragOver,
    error,
    scanResult,
    loadScanDetails,
    handleFileChange,
    handleDrop,
    performScan,
    badgeClasses
  };
}
