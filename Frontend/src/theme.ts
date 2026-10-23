import { plPL as materialPlPL } from '@mui/material/locale'
import { createTheme } from '@mui/material/styles'
import { plPL as datePickersPlPL } from '@mui/x-date-pickers/locales'
// Adds the date picker components (e.g. MuiLocalizationProvider) to the theme's TypeScript types.
import type {} from '@mui/x-date-pickers/themeAugmentation'

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
)
