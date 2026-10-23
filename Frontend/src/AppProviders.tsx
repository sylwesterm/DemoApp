import 'dayjs/locale/pl'
import CssBaseline from '@mui/material/CssBaseline'
import { ThemeProvider } from '@mui/material/styles'
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs'
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider'
import type { ReactNode } from 'react'
import { theme } from './theme'

/** Everything the components rely on: the MUI theme, base styles and Polish dates (calendar starts on Monday). */
export function AppProviders({ children }: { children: ReactNode }) {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline enableColorScheme />
      <LocalizationProvider dateAdapter={AdapterDayjs} adapterLocale="pl">
        {children}
      </LocalizationProvider>
    </ThemeProvider>
  )
}
