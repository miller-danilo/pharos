import { ref, onMounted } from 'vue';
import { auth, googleProvider, signInWithPopup } from '../lib/firebase';
import { onAuthStateChanged, type User, signOut } from 'firebase/auth';

const user = ref<User | null>(null);
const authLoading = ref(true);

export function useAuth() {
  if (!auth) {
    authLoading.value = false;
  } else {
    onAuthStateChanged(auth, (firebaseUser) => {
      user.value = firebaseUser;
      authLoading.value = false;
    });
  }

  const signInWithGoogle = async () => {
    if (!auth || !googleProvider) return;
    authLoading.value = true;
    try {
      await signInWithPopup(auth, googleProvider);
    } finally {
      authLoading.value = false;
    }
  };

  const logout = async () => {
    if (!auth) return;
    authLoading.value = true;
    try {
      await signOut(auth);
    } finally {
      authLoading.value = false;
    }
  };

  return {
    user,
    authLoading,
    signInWithGoogle,
    logout
  };
}
