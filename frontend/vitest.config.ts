import { defineConfig } from 'vitest/config';
import vue from '@vitejs/plugin-vue';

export default defineConfig({
  plugins: [vue()],
  test: {
    environment: 'jsdom',
    globals: true,
    coverage: {
      provider: 'v8',
      reporter: ['text', 'json', 'html'],
      thresholds: {
        lines: 90,
        functions: 80,
        branches: 80,
        statements: 85
      },
      exclude: [
        'src/lib/firebase.ts',
        'src/config.ts',
        'src/layouts/**',
        'dist/**',
        '**/*.astro',
        '**/*.d.ts',
        'tests/**',
        'src/components/AdminDashboard.vue'
      ]
    }
  }
});
