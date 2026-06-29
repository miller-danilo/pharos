export const API_BASE_URL = import.meta.env.PUBLIC_API_URL || 'http://localhost:5295';

export const LEMON_SQUEEZY_CHECKOUT_URL = import.meta.env.PUBLIC_LEMON_SQUEEZY_CHECKOUT_URL || 'https://pharos-ai.lemonsqueezy.com/buy/variant-id';

export const STORAGE_KEYS = {
  SCAN_RESULT: 'scanResult',
  SCANNED_JOB_TEXT: 'scannedJobText'
} as const;
