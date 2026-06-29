<template>
  <div class="glass-panel p-8 relative overflow-hidden mt-8">
    <div class="absolute -top-10 -left-10 w-40 h-40 bg-brand-primary/10 rounded-full blur-2xl"></div>

    <h3 class="text-xl font-bold mb-6 text-gradient">AI Cover Letter & Pitch Generator</h3>

    <div v-if="authLoading" class="flex justify-center py-8">
      <svg class="animate-spin h-8 w-8 text-brand-primary" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
        <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
        <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
      </svg>
    </div>

    <div v-else-if="!user" class="text-center py-6 space-y-4">
      <p class="text-text-secondary text-sm">Sign in to sync your CV and generate customized cover letters/pitches.</p>
      <button
        @click="handleSignIn"
        class="px-6 h-12 bg-white text-bg-deep font-semibold rounded-lg shadow-md hover:bg-white/90 transition-colors flex items-center justify-center gap-2 mx-auto cursor-pointer"
      >
        <svg class="w-5 h-5" viewBox="0 0 24 24">
          <path fill="#EA4335" d="M12.24 10.285V14.4h6.887c-.648 2.41-2.519 4.09-5.142 4.09-3.218 0-5.832-2.614-5.832-5.832s2.614-5.832 5.832-5.832c1.393 0 2.67.491 3.67 1.304l3.125-3.125C18.884 1.83 15.79 1 12.24 1 5.922 1 12s4.922 11 11.24 11c6.545 0 11.24-4.7 11.24-11.24 0-.764-.09-1.3-.24-1.85H12.24z"/>
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
        <div class="flex items-center gap-4">
          <div class="text-right">
            <p class="text-xs text-text-secondary font-medium">Credits</p>
            <p class="text-lg font-bold text-gradient">{{ credits }} left</p>
          </div>
          <button
            @click="handleBuyCredits"
            class="px-4 h-9 bg-brand-primary/20 hover:bg-brand-primary/30 text-brand-gradient-start text-xs font-semibold rounded-md border border-brand-primary/30 transition-all cursor-pointer"
          >
            Buy Credits
          </button>
        </div>
      </div>

      <div class="space-y-2">
        <div class="flex justify-between items-center">
          <label class="text-sm font-semibold text-text-primary">Your Resume / CV Profile</label>
          <span v-if="cvSaved" class="text-xs text-semaphore-safe font-medium">✓ Saved</span>
        </div>
        <textarea
          v-model="cvText"
          placeholder="Paste your professional experience, skills, and summary here to align cover letters with your background..."
          class="w-full h-32 bg-bg-deep/50 border border-white/10 rounded-lg p-3 text-xs text-text-primary focus:outline-none focus:border-brand-primary/50 transition-colors resize-none placeholder-text-secondary/50"
          :disabled="actionLoading"
        ></textarea>
        <button
          @click="handleSaveCv"
          :disabled="actionLoading || !cvText.trim()"
          class="px-4 h-9 bg-white/10 hover:bg-white/15 text-text-primary text-xs font-semibold rounded-md transition-colors cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
        >
          Save CV
        </button>
      </div>

      <div class="pt-4 border-t border-white/5 space-y-4">
        <div v-if="credits <= 0" class="p-6 rounded-lg border border-semaphore-danger/20 bg-semaphore-danger/5 text-center space-y-3">
          <p class="text-sm text-semaphore-danger font-semibold">No credits remaining</p>
          <p class="text-xs text-text-secondary">Please purchase a credit package to unlock AI cover letter generation aligned with your resume.</p>
          <button
            @click="handleBuyCredits"
            class="px-6 h-10 bg-brand-primary text-white font-semibold rounded-lg shadow-md hover:bg-brand-primary/95 transition-all text-xs cursor-pointer glow-primary"
          >
            Get Credits Now
          </button>
        </div>

        <button
          v-else
          @click="handleGenerate"
          :disabled="actionLoading || !cvText.trim()"
          class="w-full h-12 bg-brand-primary hover:bg-brand-primary/95 text-white font-semibold rounded-lg shadow-lg hover:shadow-brand-primary/20 transition-all duration-200 flex items-center justify-center gap-2 cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed glow-primary"
        >
          <span v-if="actionLoading">
            <svg class="animate-spin h-5 w-5 text-white inline mr-2" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
              <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
              <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
            </svg>
            Generating customized pitch...
          </span>
          <span v-else>Generate Cover Letter / Pitch (1 Credit)</span>
        </button>

        <p v-if="error" class="text-sm text-semaphore-danger text-center font-medium">{{ error }}</p>
      </div>

      <div v-if="proposal || credits <= 0" class="relative mt-6">
        <h4 class="text-sm font-semibold text-text-primary mb-3">Your Customized Cover Letter</h4>
        
        <div :class="['p-6 rounded-lg bg-bg-deep/50 border border-white/10 text-sm leading-relaxed max-h-96 overflow-y-auto space-y-4 font-sans select-text', credits <= 0 && !proposal ? 'blur-[6px]' : '']">
          <template v-if="proposal">
            <pre class="whitespace-pre-wrap font-sans text-sm">{{ proposal }}</pre>
          </template>
          <template v-else>
            <p class="font-bold text-sm">Dear Hiring Team,</p>
            <p>I am writing to express my strong interest in the Software Engineer position. With over 4 years of experience building modern web architectures using .NET 8 and Vue.js, I am confident that my technical skillset aligns perfectly with the requirements of this role.</p>
            <p>At my previous company, I spearheaded the migration of legacy APIs to Serverless architectures, resulting in a 40% reduction in operating costs and a significant increase in payload performance. I am excited about the opportunity to bring similar optimization and code quality to your engineering team...</p>
            <p>Thank you for your time and consideration.</p>
            <p>Best regards,<br/>[Your Name]</p>
          </template>
        </div>

        <button
          v-if="proposal"
          @click="handleCopy"
          class="absolute top-9 right-3 px-3 py-1.5 bg-white/10 hover:bg-white/15 text-text-primary text-xs font-semibold rounded transition-colors cursor-pointer"
        >
          {{ copied ? 'Copied!' : 'Copy' }}
        </button>
      </div>

      <!-- Transaction History Section -->
      <div class="mt-8 pt-6 border-t border-white/5 space-y-4">
        <h4 class="text-sm font-semibold text-text-primary">Historial de Transacciones / Uso</h4>
        <div class="glass-panel p-4 overflow-x-auto">
          <table class="w-full text-left text-xs text-text-secondary border-collapse">
            <thead>
              <tr class="border-b border-white/10">
                <th class="py-2 pr-4 text-text-primary">Fecha</th>
                <th class="py-2 pr-4 text-text-primary">Detalle</th>
                <th class="py-2 pr-4 text-text-primary">Créditos</th>
                <th class="py-2 text-text-primary">Uso Detallado</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="t in transactionsList" :key="t.id" class="border-b border-white/5 hover:bg-white/5">
                <td class="py-2 pr-4 font-mono">{{ new Date(t.createdAt).toLocaleDateString() }}</td>
                <td class="py-2 pr-4 capitalize">
                  {{ t.reason.startsWith('payment_purchase:') ? 'Compra de Créditos' : t.reason === 'job_safety_scan' ? 'Escaneo de Seguridad' : t.reason === 'proposal_generation' ? 'Carta de Presentación' : t.reason === 'welcome_balance' ? 'Bono de Bienvenida' : t.reason }}
                </td>
                <td :class="['py-2 pr-4 font-bold', t.creditsChanged > 0 ? 'text-semaphore-safe' : t.creditsChanged < 0 ? 'text-semaphore-danger' : 'text-text-secondary']">
                  {{ t.creditsChanged > 0 ? '+' : '' }}{{ t.creditsChanged }}
                </td>
                <td class="py-2 font-mono text-[10px] text-text-secondary/70">
                  <span v-if="t.usage">
                    Tokens: {{ t.usage.promptTokens + t.usage.completionTokens }} (In: {{ t.usage.promptTokens }} | Out: {{ t.usage.completionTokens }}) | DB: R{{ t.usage.dbReads }} W{{ t.usage.dbWrites }}
                  </span>
                  <span v-else>-</span>
                </td>
              </tr>
              <tr v-if="transactionsList.length === 0">
                <td colspan="4" class="py-4 text-center text-text-secondary/60">No se encontraron registros de transacciones.</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue';
import { useAuth } from '../composables/useAuth';
import { fetchUserProfile, saveUserCv, generateProposal, fetchUserTransactions } from '../services/apiService';
import { API_BASE_URL } from '../config';

const props = defineProps({
  jobText: {
    type: String,
    required: true
  }
});

const { user, authLoading, signInWithGoogle } = useAuth();
const credits = ref(0);
const cvText = ref('');
const proposal = ref('');

const actionLoading = ref(false);
const cvSaved = ref(false);
const copied = ref(false);
const error = ref('');
const transactionsList = ref([]);

const LEMON_SQUEEZY_CHECKOUT_URL = import.meta.env.PUBLIC_LEMON_SQUEEZY_CHECKOUT_URL || 'https://pharos-ai.lemonsqueezy.com/buy/variant-id';

onMounted(() => {
  if (!document.getElementById('lemonsqueezy-script')) {
    const script = document.createElement('script');
    script.id = 'lemonsqueezy-script';
    script.src = 'https://app.lemonsqueezy.com/js/checkout.js';
    script.defer = true;
    document.head.appendChild(script);
  }
});

const loadTransactions = async () => {
  if (!user.value) return;
  try {
    const token = await user.value.getIdToken();
    transactionsList.value = await fetchUserTransactions(token);
  } catch (err) {
    // Fail silently for list load
  }
};

const loadProfile = async () => {
  if (!user.value) return;
  try {
    const token = await user.value.getIdToken();
    const profile = await fetchUserProfile(token);
    credits.value = profile.credits;
    cvText.value = profile.cvText || '';
    await loadTransactions();
  } catch (err) {
    error.value = 'Failed to fetch user profile. Please try logging in again.';
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
  }
}, { immediate: true });

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
    }, 3000);
  } catch (err) {
    error.value = err.message || 'Error updating CV.';
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
    await loadTransactions();
  } catch (err) {
    error.value = err.message || 'An error occurred. Please try again.';
  } finally {
    actionLoading.value = false;
  }
};

const handleBuyCredits = async () => {
  if (!user.value) {
    error.value = 'Please sign in first before purchasing credits.';
    return;
  }

  if (API_BASE_URL.includes('localhost') || API_BASE_URL.includes('127.0.0.1')) {
    actionLoading.value = true;
    error.value = '';
    try {
      const response = await fetch(`${API_BASE_URL}/api/payment/webhook`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          meta: {
            event_name: 'order_created',
            custom_data: {
              user_id: user.value.uid
            }
          },
          data: {
            id: 'mock-order-' + Date.now(),
            attributes: {
              first_order_item: {
                quantity: 10
              },
              total: 599,
              currency: 'USD'
            }
          }
        })
      });

      if (!response.ok) {
        throw new Error('Failed to simulate payment webhook.');
      }
      
      await loadProfile();
    } catch (err) {
      error.value = 'Dev payment simulation failed: ' + err.message;
    } finally {
      actionLoading.value = false;
    }
    return;
  }

  const checkoutUrl = `${LEMON_SQUEEZY_CHECKOUT_URL}?checkout[custom][user_id]=${user.value.uid}&embed=1`;

  if (window.LemonSqueezy) {
    window.LemonSqueezy.Url.Open(checkoutUrl);
  } else {
    window.open(checkoutUrl, '_blank');
  }
};

const handleCopy = () => {
  if (!proposal.value) return;
  navigator.clipboard.writeText(proposal.value);
  copied.value = true;
  setTimeout(() => {
    copied.value = false;
  }, 2000);
};
</script>
