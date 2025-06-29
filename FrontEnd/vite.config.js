import {defineConfig} from 'vite';
import react from '@vitejs/plugin-react-swc';
import {resolve} from 'path';
import postcss from 'postcss';
import autoprefixer from 'autoprefixer';

export default defineConfig({
  plugins: [
    react(),
    {
      name: 'vite:postcss-autoprefixer',
      config() {
        return {
          css: {
            postcss: {
              plugins: [autoprefixer()],
            },
          },
        };
      },
    },
  ],
  resolve: {
    alias: {
      '@': '/src',
      'pdfjs-dist/build/pdf.worker.min.js': 'pdfjs-dist/build/pdf.worker.min.js',
    },
  },
  server: {
    host: true,
    port: 44447,
    allowedHosts: [
      'mydomain.local',
    ],
    proxy: {
      '/api': {
        target: 'https://localhost:5001',
        secure: false,
        changeOrigin: true,
      },
      '/Identity': {
        target: 'https://localhost:5001',
        secure: false,
        changeOrigin: true,
      },
      '/uploads': {
        target: 'https://localhost:5001',
        secure: false,
        changeOrigin: true,
      },
    },
  },
  build: {
    outDir: '../wwwroot',
    emptyOutDir: true,
    rollupOptions: {
      input: {
        app: resolve(__dirname, 'index.html'),
        landing: resolve(__dirname, 'public/landing.html'),
      },
      output: {
        entryFileNames: 'assets/js/[name].[hash].js',
        chunkFileNames: 'assets/js/[name].[hash].js',
        assetFileNames: (assetInfo) => {
          if (assetInfo.name.endsWith('.css')) {
            return 'assets/css/[name].[hash][extname]';
          }
          return 'assets/[name].[hash][extname]';
        },
      },
    },
  },
  base: '/',
});