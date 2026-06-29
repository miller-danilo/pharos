<template>
  <div class="w-full max-w-2xl mx-auto glass-panel p-8 relative overflow-hidden">
    <!-- Glow Decorator -->
    <div class="absolute -top-10 -right-10 w-40 h-40 bg-brand-primary/10 rounded-full blur-2xl"></div>
    
    <h2 class="text-2xl font-bold mb-6 text-center text-gradient">Scan Job Listing</h2>
    
    <form @submit.prevent="handleScanSubmit" class="space-y-6">
      <!-- Text Input -->
      <div class="space-y-2">
        <label class="block text-sm font-semibold text-text-primary">Paste Job Description</label>
        <textarea
          v-model="jobText"
          placeholder="Paste the full job offer description, requirements, benefits, and contact information here..."
          class="w-full h-44 bg-bg-deep/50 border border-white/10 rounded-lg p-4 text-sm text-text-primary focus:outline-none focus:border-brand-primary/50 transition-colors resize-none placeholder-text-secondary/50"
          :disabled="loading"
        ></textarea>
      </div>

      <!-- File Dropzone -->
      <div class="space-y-2">
        <label class="block text-sm font-semibold text-text-primary">Or Upload Job File (PDF or Image)</label>
        <div
          @dragover.prevent="dragOver = true"
          @dragleave.prevent="dragOver = false"
          @drop.prevent="handleDrop"
          :class="[
            'border border-dashed rounded-lg p-6 text-center cursor-pointer transition-all duration-200',
            dragOver ? 'border-brand-primary bg-brand-primary/5' : 'border-white/10 hover:border-white/20 bg-bg-deep/20'
          ]"
          @click="triggerFileInput"
        >
          <input
            type="file"
            ref="fileInput"
            class="hidden"
            accept=".pdf,image/*"
            @change="handleFileChange"
            :disabled="loading"
          />
          
          <div class="flex flex-col items-center justify-center gap-2">
            <svg xmlns="http://www.w3.org/2000/svg" class="w-8 h-8 text-brand-primary/70" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4M17 8l-5-5-5 5M12 3v12"/>
            </svg>
            <p class="text-sm text-text-primary font-medium">
              {{ selectedFile ? selectedFile.name : 'Drag & drop file here or click to browse' }}
            </p>
            <p class="text-xs text-text-secondary">Supports PDF, PNG, JPG, or Screenshot images</p>
          </div>
        </div>
      </div>

      <!-- Submit Button -->
      <button
        type="submit"
        :disabled="loading || (!jobText && !selectedFile)"
        class="w-full h-12 bg-brand-primary hover:bg-brand-primary/95 text-white font-semibold rounded-lg shadow-lg hover:shadow-brand-primary/20 transition-all duration-200 flex items-center justify-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed cursor-pointer"
      >
        <LoadingSpinner v-if="loading" message="Analyzing vacancy patterns..." />
        <template v-else>
          <svg xmlns="http://www.w3.org/2000/svg" class="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/>
          </svg>
          <span>Analyze Safety Level</span>
        </template>
      </button>
      
      <p v-if="error" class="text-sm text-semaphore-danger text-center mt-2 font-medium">{{ error }}</p>
    </form>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useScanner } from '../composables/useScanner';
import LoadingSpinner from './LoadingSpinner.vue';

const {
  jobText,
  selectedFile,
  loading,
  dragOver,
  error,
  handleFileChange,
  handleDrop,
  performScan
} = useScanner();

const fileInput = ref<HTMLInputElement | null>(null);

const triggerFileInput = () => {
  if (!loading.value && fileInput.value) {
    fileInput.value.click();
  }
};

const handleScanSubmit = async () => {
  try {
    await performScan();
    window.location.href = '/dashboard';
  } catch {
    // Error is set in the composable and displayed in template
  }
};

defineExpose({
  jobText,
  selectedFile,
  fileInput,
  dragOver,
  error
});
</script>
