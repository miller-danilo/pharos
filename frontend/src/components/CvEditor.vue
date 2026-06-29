<template>
  <div class="space-y-2">
    <div class="flex justify-between items-center">
      <label class="text-sm font-semibold text-text-primary">Your Resume / CV Profile</label>
      <span v-if="cvSaved" class="text-xs text-semaphore-safe font-medium">✓ Saved</span>
    </div>
    <textarea
      :value="modelValue"
      @input="emitUpdate"
      placeholder="Paste your professional experience, skills, and summary here to align cover letters with your background..."
      class="w-full h-32 bg-bg-deep/50 border border-white/10 rounded-lg p-3 text-xs text-text-primary focus:outline-none focus:border-brand-primary/50 transition-colors resize-none placeholder-text-secondary/50"
      :disabled="loading"
    ></textarea>
    <button
      @click="$emit('save')"
      :disabled="loading || !modelValue.trim()"
      class="px-4 h-9 bg-white/10 hover:bg-white/15 text-text-primary text-xs font-semibold rounded-md transition-colors cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
    >
      Save CV
    </button>
  </div>
</template>

<script setup lang="ts">
defineProps<{
  modelValue: string;
  loading: boolean;
  cvSaved: boolean;
}>();

const emit = defineEmits<{
  (e: 'update:modelValue', value: string): void;
  (e: 'save'): void;
}>();

const emitUpdate = (e: Event) => {
  emit('update:modelValue', (e.target as HTMLTextAreaElement).value);
};
</script>
