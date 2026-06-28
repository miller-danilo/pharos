import { initializeApp } from 'firebase/app';
import { getAuth, GoogleAuthProvider, signInWithPopup, signOut, connectAuthEmulator } from 'firebase/auth';

const firebaseConfig = {
  apiKey: import.meta.env.PUBLIC_FIREBASE_API_KEY || "AIzaSyFakeKey-ForLocalDevTestingOnly",
  authDomain: import.meta.env.PUBLIC_FIREBASE_AUTH_DOMAIN || "pharos-ai-demo.firebaseapp.com",
  projectId: import.meta.env.PUBLIC_FIREBASE_PROJECT_ID || "pharos-ai-demo",
  storageBucket: import.meta.env.PUBLIC_FIREBASE_STORAGE_BUCKET || "pharos-ai-demo.appspot.com",
  messagingSenderId: import.meta.env.PUBLIC_FIREBASE_MESSAGING_SENDER_ID || "1234567890",
  appId: import.meta.env.PUBLIC_FIREBASE_APP_ID || "1:1234567890:web:1234567890"
};

let app;
let auth: ReturnType<typeof getAuth> | undefined;
let googleProvider: GoogleAuthProvider | undefined;

if (typeof window !== 'undefined') {
  app = initializeApp(firebaseConfig);
  auth = getAuth(app);
  googleProvider = new GoogleAuthProvider();
  if (import.meta.env.DEV) {
    connectAuthEmulator(auth, 'http://localhost:9099');
  }
}

export { auth, googleProvider, signInWithPopup, signOut, GoogleAuthProvider };
