import { API_BASE_URL } from '../config';

export async function fetchUserProfile(token: string) {
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

export async function saveUserCv(token: string, cvText: string) {
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

export async function generateProposal(token: string, jobText: string, cvText: string) {
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

export async function fetchUserTransactions(token: string) {
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

export async function fetchCostMultipliers(token: string) {
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

export async function saveCostMultipliers(token: string, multipliers: any) {
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
