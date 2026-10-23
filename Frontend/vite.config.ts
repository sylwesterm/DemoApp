import react from '@vitejs/plugin-react'
import { defineConfig } from 'vitest/config'

// Address of the backend started with `dotnet run --project src/CurrencyRates.Api` (launch profile "http").
const apiUrl = process.env.API_URL ?? 'http://localhost:5178'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    // The browser calls /api on the same origin, so the backend needs no CORS configuration.
    proxy: {
      '/api': apiUrl,
    },
  },
  test: {
    environment: 'jsdom',
    setupFiles: ['./src/test/setup.ts'],
  },
})
