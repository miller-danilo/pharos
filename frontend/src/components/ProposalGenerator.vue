<template>
  <div class="space-y-4">
    <button
      @click="$emit('generate')"
      :disabled="loading || disabled"
      class="w-full h-12 bg-brand-primary hover:bg-brand-primary/95 text-white font-semibold rounded-lg shadow-lg hover:shadow-brand-primary/20 transition-all duration-200 flex items-center justify-center gap-2 cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed glow-primary"
    >
      <LoadingSpinner v-if="loading" message="Generating customized pitch..." />
      <span v-else>Generate Cover Letter / Pitch (1 Credit)</span>
    </button>

    <div v-if="proposal || blurMode" class="relative mt-6">
      <h4 class="text-sm font-semibold text-text-primary mb-3">Your Customized Cover Letter</h4>
      
      <div :class="['p-6 rounded-lg bg-bg-deep/50 border border-white/10 text-sm leading-relaxed max-h-96 overflow-y-auto space-y-4 font-sans select-text', blurMode && !proposal ? 'blur-[6px]' : '']">
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
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import LoadingSpinner from './LoadingSpinner.vue';
import { UI_COPY_FEEDBACK_MS } from '../constants';

const props = defineProps<{
  proposal: string;
  loading: boolean;
  disabled: boolean;
  blurMode?: boolean;
}>();

defineEmits<{
  (e: 'generate'): void;
}>();

const copied = ref(false);

const handleCopy = () => {
  if (!props.proposal) return;
  navigator.clipboard.writeText(props.proposal);
  copied.value = true;
  setTimeout(() => {
    copied.value = false;
  }, UI_COPY_FEEDBACK_MS);
};
</script>
