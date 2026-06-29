<template>
  <div class="glass-panel p-8 relative overflow-hidden mt-8">
    <div class="absolute -top-10 -left-10 w-40 h-40 bg-brand-primary/10 rounded-full blur-2xl"></div>

    <h3 class="text-xl font-bold mb-6 text-gradient">AI Cover Letter & Pitch Generator</h3>

    <div v-if="!isMounted || authLoading" class="flex justify-center py-8">
      <LoadingSpinner />
    </div>

    <div v-else-if="!user" class="text-center py-6 space-y-4">
      <p class="text-text-secondary text-sm">Sign in to sync your CV and generate customized cover letters/pitches.</p>
      <button
        @click="handleSignIn"
        class="px-6 h-12 bg-white text-bg-deep font-semibold rounded-lg shadow-md hover:bg-white/90 transition-colors flex items-center justify-center gap-2 mx-auto cursor-pointer"
      >
        <svg class="w-5 h-5" viewBox="0 0 24 24">
          <path fill="#EA4335" d="M12.24 10.285V14.4h6.887c-.648 2.41-2.519 4.09-5.142 4.09-3.218 0-5.832-2.614-5.832-5.832s2.614-5.832 5.832-5.832c1.393 0 2.67.491 3.67 1.304l3.125-3.125C18.884 1.83 15.79 0 12.24 0c-5.922 0-11.24 4.92-11.24 11.24s4.922 11.24 11.24 11.24c6.545 0 11.24-4.7 11.24-11.24 0-.764-.09-1.3-.24-1.85H12.24z"/>
        </svg>
        <span>Sign In with Google</span>
      </button>
      <p v-if="error" class="text-sm text-semaphore-danger text-center mt-2 font-medium">{{ error }}</p>
    </div>

    <div v-else class="space-y-6">
      <div class="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 p-4 rounded-lg bg-bg-deep/40 border border-white/5">
        <div>
          <p class="text-xs text-text-secondary font-medium">Logged in as</p>
          <p class="text-sm font-semibold text-text-primary">{{ user.email }}</p>
        </div>
        <CreditPurchase
          :credits="credits"
          :loading="paymentLoading"
          @buy="triggerBuyCredits"
        />
      </div>

      <CvEditor
        v-model="cvText"
        :loading="actionLoading"
        :cvSaved="cvSaved"
        @save="handleSaveCv"
      />

      <div class="pt-4 border-t border-white/5 space-y-4">
        <CreditPurchase
          v-if="credits <= 0"
          :credits="credits"
          :loading="paymentLoading"
          :noCreditsMode="true"
          @buy="triggerBuyCredits"
        />

        <ProposalGenerator
          v-else
          :proposal="proposal"
          :loading="actionLoading"
          :disabled="!cvText.trim()"
          @generate="handleGenerate"
        />

        <p v-if="error" class="text-sm text-semaphore-danger text-center font-medium">{{ error }}</p>
      </div>

      <TransactionHistory
        :transactions="transactions"
        :formatReason="formatReason"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useAuth } from '../composables/useAuth';
import { usePayment } from '../composables/usePayment';
import { useTransactions } from '../composables/useTransactions';
import { fetchUserProfile, saveUserCv, generateProposal } from '../services/apiService';
import { UI_SAVE_FEEDBACK_MS } from '../constants';
import LoadingSpinner from './LoadingSpinner.vue';
import CvEditor from './CvEditor.vue';
import CreditPurchase from './CreditPurchase.vue';
import ProposalGenerator from './ProposalGenerator.vue';
import TransactionHistory from './TransactionHistory.vue';

const props = defineProps<{
  jobText: string;
}>();

const { user, authLoading, signInWithGoogle } = useAuth();
const { loading: paymentLoading, error: paymentError, handleBuyCredits } = usePayment();
const { transactions, loading: txnLoading, error: txnError, loadTransactions, formatReason } = useTransactions();

const isMounted = ref(false);
const credits = ref(0);
const cvText = ref('');
const proposal = ref('');
const actionLoading = ref(false);
const cvSaved = ref(false);
const error = ref('');

onMounted(() => {
  isMounted.value = true;
  if (!document.getElementById('lemonsqueezy-script')) {
    const script = document.createElement('script');
    script.id = 'lemonsqueezy-script';
    script.src = 'https://app.lemonsqueezy.com/js/checkout.js';
    script.defer = true;
    document.head.appendChild(script);
  }
});

const loadProfile = async () => {
  if (!user.value) return;
  try {
    const token = await user.value.getIdToken();
    const profile = await fetchUserProfile(token);
    credits.value = profile.credits;
    cvText.value = profile.cvText || '';
    await loadTransactions(token);
  } catch (err) {
    const e = err as Error;
    error.value = 'Failed to fetch user profile: ' + e.message;
  }
};

watch(user, async (newUser) => {
  error.value = '';
  if (newUser) {
    await loadProfile();
  } else {
    credits.value = 0;
    cvText.value = '';
    proposal.value = '';
    transactions.value = [];
  }
}, { immediate: true });

// Combine child errors into our local error display
watch([paymentError, txnError], ([newPayErr, newTxnErr]) => {
  if (newPayErr) error.value = newPayErr;
  else if (newTxnErr) error.value = newTxnErr;
});

const handleSignIn = async () => {
  error.value = '';
  try {
    await signInWithGoogle();
  } catch (err) {
    error.value = 'Sign in failed. Please try again.';
  }
};

const handleSaveCv = async () => {
  if (actionLoading.value || !user.value) return;
  error.value = '';
  cvSaved.value = false;
  actionLoading.value = true;

  try {
    const token = await user.value.getIdToken();
    await saveUserCv(token, cvText.value);
    cvSaved.value = true;
    setTimeout(() => {
      cvSaved.value = false;
    }, UI_SAVE_FEEDBACK_MS);
  } catch (err) {
    const e = err as Error;
    error.value = e.message || 'Error updating CV.';
  } finally {
    actionLoading.value = false;
  }
};

const handleGenerate = async () => {
  if (actionLoading.value || !user.value) return;
  error.value = '';
  proposal.value = '';
  actionLoading.value = true;

  try {
    const token = await user.value.getIdToken();
    const data = await generateProposal(token, props.jobText, cvText.value);
    proposal.value = data.proposal;
    credits.value = Math.max(0, credits.value - 1);
    await loadTransactions(token);
  } catch (err) {
    const e = err as Error;
    error.value = e.message || 'An error occurred. Please try again.';
  } finally {
    actionLoading.value = false;
  }
};

const triggerBuyCredits = async () => {
  if (!user.value) return;
  await handleBuyCredits(user.value.uid, loadProfile);
};
</script>
