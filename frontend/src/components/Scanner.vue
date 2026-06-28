<template>
  <div class="w-full max-w-2xl mx-auto glass-panel p-8 relative overflow-hidden">
    <!-- Glow Decorator -->
    <div class="absolute -top-10 -right-10 w-40 h-40 bg-brand-primary/10 rounded-full blur-2xl"></div>
    
    <h2 class="text-2xl font-bold mb-6 text-center text-gradient">Scan Job Listing</h2>
    
    <form @submit.prevent="handleScan" class="space-y-6">
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
        <template v-if="loading">
          <!-- Spinner -->
          <svg class="animate-spin h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
          </svg>
          <span>Analyzing vacancy patterns...</span>
        </template>
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

<script setup>
import { ref } from 'vue';
import { API_BASE_URL, STORAGE_KEYS } from '../config';

const jobText = ref('');
const selectedFile = ref(null);
const fileInput = ref(null);
const loading = ref(false);
const dragOver = ref(false);
const error = ref('');

const triggerFileInput = () => {
  if (!loading.value) {
    fileInput.value.click();
  }
};

const handleFileChange = (e) => {
  if (e.target.files && e.target.files.length > 0) {
    selectedFile.value = e.target.files[0];
    error.value = '';
  }
};

const handleDrop = (e) => {
  dragOver.value = false;
  if (loading.value) return;
  
  if (e.dataTransfer.files && e.dataTransfer.files.length > 0) {
    selectedFile.value = e.dataTransfer.files[0];
    error.value = '';
  }
};

const handleScan = async () => {
  if (loading.value) return;
  error.value = '';
  loading.value = true;

  try {
    const formData = new FormData();
    if (selectedFile.value) {
      formData.append('file', selectedFile.value);
    }
    if (jobText.value) {
      formData.append('text', jobText.value);
    }

    const response = await fetch(`${API_BASE_URL}/api/shield/analyze`, {
      method: 'POST',
      body: formData,
    });

    if (!response.ok) {
      const errMsg = await response.text();
      throw new Error(errMsg || 'Analysis request failed. Please check the backend connection.');
    }

    const result = await response.json();
    
    sessionStorage.setItem(STORAGE_KEYS.SCAN_RESULT, JSON.stringify(result));
    sessionStorage.setItem(STORAGE_KEYS.SCANNED_JOB_TEXT, jobText.value || selectedFile.value?.name || 'Scanned job file');

    window.location.href = '/dashboard';
  } catch (err) {
    error.value = err.message || 'An error occurred during scanning. Please try again.';
  } finally {
    loading.value = false;
  }
};
</script>
