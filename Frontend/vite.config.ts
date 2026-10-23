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
  build: {
    rolldownOptions: {
      output: {
        // Libraries in their own files: they are cached by the browser while the application code changes,
        // and no single file exceeds Vite's 500 kB warning limit.
        codeSplitting: {
          groups: [
            { name: 'react', test: /node_modules[\\/](react|react-dom|scheduler)[\\/]/, priority: 3 },
            { name: 'mui-x', test: /node_modules[\\/](@mui[\\/]x-[^\\/]+|dayjs)[\\/]/, priority: 2 },
            { name: 'mui', test: /node_modules[\\/](@mui|@emotion)[\\/]/, priority: 1 },
          ],
        },
      },
    },
  },
  test: {
    environment: 'jsdom',
    setupFiles: ['./src/test/setup.ts'],
  },
})
