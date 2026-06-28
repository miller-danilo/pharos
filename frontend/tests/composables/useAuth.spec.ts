import { describe, it, expect, vi } from 'vitest';
import { useAuth } from '../../src/composables/useAuth';
import { signInWithPopup } from '../../src/lib/firebase';
import { signOut } from 'firebase/auth';

vi.mock('../../src/lib/firebase', () => ({
  auth: {},
  googleProvider: {},
  signInWithPopup: vi.fn()
}));

vi.mock('firebase/auth', () => ({
  onAuthStateChanged: vi.fn((auth, cb) => {
    cb({ email: 'test@example.com', uid: '123' });
    return vi.fn();
  }),
  signInWithPopup: vi.fn().mockResolvedValue({}),
  signOut: vi.fn().mockResolvedValue({})
}));

describe('useAuth Composable', () => {
  it('should initialize auth listener on mount', () => {
    const { user } = useAuth();
    expect(user.value).not.toBeNull();
    expect(user.value?.email).toBe('test@example.com');
  });

  it('should trigger signInWithPopup on signInWithGoogle', async () => {
    const { signInWithGoogle } = useAuth();
    await signInWithGoogle();
    expect(signInWithPopup).toHaveBeenCalled();
  });

  it('should trigger signOut on logout', async () => {
    const { logout } = useAuth();
    await logout();
    expect(signOut).toHaveBeenCalled();
  });
});
