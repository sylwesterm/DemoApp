import CurrencyExchangeIcon from '@mui/icons-material/CurrencyExchange'
import { Alert, AppBar, Box, Container, LinearProgress, Link, Toolbar, Typography } from '@mui/material'
import { ConversionForm } from './features/conversion/ConversionForm'
import { lazy, Suspense } from 'react'
import { useExchangeRates } from './features/conversion/useExchangeRates'

// The table and the chart are large and only needed after the first conversion, so they are loaded on demand.
const ResultsView = lazy(() =>
  import('./features/results/ResultsView').then((module) => ({ default: module.ResultsView })),
)

function App() {
  const { state, load } = useExchangeRates()

  return (
    <Box sx={{ minHeight: '100svh', display: 'flex', flexDirection: 'column' }}>
      <AppBar position="static" elevation={0}>
        <Toolbar>
          <CurrencyExchangeIcon sx={{ mr: 1.5 }} />
          <Typography variant="h6" component="h1">
            Kursy walut
          </Typography>
        </Toolbar>
      </AppBar>

      <Container component="main" maxWidth="md" sx={{ flex: 1, py: 4 }}>
        <Typography color="text.secondary" sx={{ mb: 3 }}>
          Przeliczanie kursów walut i ich historia na podstawie danych dostawców kursów.
        </Typography>

        <ConversionForm
          onSubmit={load}
          submitting={state.status === 'loading'}
          serverErrors={state.status === 'error' ? state.error.fieldErrors : undefined}
        />

        <Box sx={{ mt: 3 }} aria-live="polite">
          {state.status === 'empty' && (
            <Alert severity="info">
              Brak kursów w wybranym okresie – dostawca nie publikuje kursów w weekendy i święta, a kursy z bieżącego
              dnia mogą jeszcze nie być dostępne. Wybierz inny zakres dat.
            </Alert>
          )}
          {state.status === 'error' && <Alert severity="error">{state.error.message}</Alert>}
          {state.status === 'success' && (
            <Suspense fallback={<LinearProgress aria-label="Ładowanie wyników" />}>
              <ResultsView history={state.history} />
            </Suspense>
          )}
        </Box>
      </Container>

      <Box component="footer" sx={{ py: 3, borderTop: 1, borderColor: 'divider' }}>
        <Container maxWidth="md">
          <Typography variant="body2" color="text.secondary">
            Dane:{' '}
            <Link href="https://api.nbp.pl/" color="inherit">
              API Narodowego Banku Polskiego
            </Link>
          </Typography>
        </Container>
      </Box>
    </Box>
  )
}

export default App
