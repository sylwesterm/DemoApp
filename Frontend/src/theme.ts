import { plPL as materialPlPL } from '@mui/material/locale'
import { createTheme, type ThemeOptions } from '@mui/material/styles'
import { plPL as datePickersPlPL } from '@mui/x-date-pickers/locales'
// Add the MUI X components (e.g. MuiLocalizationProvider) to the theme's TypeScript types.
// The data grid's Polish texts are set on the grid itself (RatesTable), so the grid stays out of the start bundle.
import type {} from '@mui/x-charts/themeAugmentation'
import type {} from '@mui/x-date-pickers/themeAugmentation'

/** MUI X Charts has no Polish locale; these are the only texts the free line chart can show. */
const chartsPlPL: ThemeOptions = {
  components: {
    MuiChartsLocalizationProvider: {
      defaultProps: {
        localeText: {
          loading: 'Ładowanie danych…',
          noData: 'Brak danych do wyświetlenia',
        },
      },
    },
  },
}

/**
 * Application theme: light and dark colour schemes that follow the system setting (CSS variables,
 * no JavaScript switching), and Polish texts of the MUI components - e.g. "Brak opcji" or "Anuluj".
 */
export const theme = createTheme(
  {
    cssVariables: true,
    colorSchemes: {
      light: {
        palette: {
          primary: { main: '#1e4f91' },
          background: { default: '#f5f7fa' },
        },
      },
      dark: {
        palette: {
          primary: { main: '#8ab4f8' },
        },
      },
    },
    typography: {
      fontFamily: 'system-ui, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif',
    },
    shape: {
      borderRadius: 10,
    },
  },
  materialPlPL,
  datePickersPlPL,
  chartsPlPL,
)
