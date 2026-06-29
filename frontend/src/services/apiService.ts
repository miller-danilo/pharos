import { API_BASE_URL } from '../config';

export interface CostMultipliersDto {
  geminiInputTokenRate: number;
  geminiOutputTokenRate: number;
  firestoreReadRate: number;
  firestoreWriteRate: number;
}

export interface UsageTelemetryDto {
  promptTokens: number;
  completionTokens: number;
  dbReads: number;
  dbWrites: number;
}

export interface TransactionDto {
  id: string;
  userId: string;
  creditsChanged: number;
  reason: string;
  createdAt: string;
  currency: string;
  usage?: UsageTelemetryDto;
}

export interface UserProfileDto {
  id: string;
  email: string;
  credits: number;
  cvText: string;
  createdAt: string;
}

export interface AnalysisFactorDto {
  name: string;
  description: string;
  isRisk: boolean;
}

export interface ScanResultDto {
  riskLevel: string;
  summary: string;
  reasoning: string;
  factors: AnalysisFactorDto[];
  promptTokens: number;
  completionTokens: number;
}

export async function fetchUserProfile(token: string): Promise<UserProfileDto> {
  const response = await fetch(`${API_BASE_URL}/api/user/profile`, {
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });
  if (!response.ok) {
    throw new Error('Failed to load profile.');
  }
  return response.json();
}

export async function saveUserCv(token: string, cvText: string): Promise<{ message: string }> {
  const response = await fetch(`${API_BASE_URL}/api/user/cv`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify({ cvText })
  });
  if (!response.ok) {
    const msg = await response.text();
    throw new Error(msg || 'Failed to save CV.');
  }
  return response.json();
}

export async function generateProposal(token: string, jobText: string, cvText: string): Promise<{ proposal: string }> {
  const response = await fetch(`${API_BASE_URL}/api/proposal/generate`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify({ jobText, cvText })
  });

  if (response.status === 402) {
    throw new Error('Insufficient credits. Please purchase more credits.');
  }

  if (!response.ok) {
    const errMsg = await response.text();
    throw new Error(errMsg || 'Failed to generate proposal.');
  }

  return response.json();
}

export async function fetchUserTransactions(token: string): Promise<TransactionDto[]> {
  const response = await fetch(`${API_BASE_URL}/api/user/transactions`, {
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });
  if (!response.ok) {
    throw new Error('Failed to load transactions.');
  }
  return response.json();
}

export async function fetchCostMultipliers(token: string): Promise<CostMultipliersDto> {
  const response = await fetch(`${API_BASE_URL}/api/admin/multipliers`, {
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });
  if (!response.ok) {
    throw new Error('Failed to load cost multipliers.');
  }
  return response.json();
}

export async function saveCostMultipliers(token: string, multipliers: CostMultipliersDto): Promise<{ message: string }> {
  const response = await fetch(`${API_BASE_URL}/api/admin/multipliers`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify(multipliers)
  });
  if (!response.ok) {
    throw new Error('Failed to save cost multipliers.');
  }
  return response.json();
}

export async function analyzeScan(formData: FormData): Promise<ScanResultDto> {
  const response = await fetch(`${API_BASE_URL}/api/shield/analyze`, {
    method: 'POST',
    body: formData
  });
  if (!response.ok) {
    const errMsg = await response.text();
    throw new Error(errMsg || 'Failed to analyze job vacancy.');
  }
  return response.json();
}

export async function sendMockWebhook(payload: object): Promise<{ message: string }> {
  const response = await fetch(`${API_BASE_URL}/api/payment/webhook`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(payload)
  });
  if (!response.ok) {
    const errMsg = await response.text();
    throw new Error(errMsg || 'Failed to send mock payment webhook.');
  }
  return response.json();
}
