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
        // and no single file exceeds Vite's 500 kB warning limit. A group also takes the dependencies of its
        // modules, so the shared groups (react, mui) need a higher priority than the MUI X groups -
        // otherwise the first MUI X group would swallow the whole of MUI. The table and the chart are only
        // needed for the results, which are loaded lazily, so their groups are not downloaded at start.
        codeSplitting: {
          groups: [
            { name: 'react', test: /node_modules[\\/](react|react-dom|scheduler)[\\/]/, priority: 5 },
            { name: 'mui', test: /node_modules[\\/](@mui[\\/](?!x-)|@mui[\\/]x-internals|@emotion)/, priority: 4 },
            { name: 'mui-date-pickers', test: /node_modules[\\/](@mui[\\/]x-date-pickers|dayjs)[\\/]/, priority: 3 },
            { name: 'mui-data-grid', test: /node_modules[\\/]@mui[\\/]x-data-grid[\\/]/, priority: 3 },
            { name: 'mui-charts', test: /node_modules[\\/]@mui[\\/]x-charts/, priority: 3 },
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
