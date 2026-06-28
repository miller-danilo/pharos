export const API_BASE_URL = import.meta.env.PUBLIC_API_URL || 'http://localhost:5295';

export const STORAGE_KEYS = {
  SCAN_RESULT: 'scanResult',
  SCANNED_JOB_TEXT: 'scannedJobText'
} as const;
