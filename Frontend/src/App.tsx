import CurrencyExchangeIcon from '@mui/icons-material/CurrencyExchange'
import { Alert, AppBar, Box, Container, LinearProgress, Link, Toolbar, Typography } from '@mui/material'
import { lazy, Suspense } from 'react'
import { ConversionForm } from './features/conversion/ConversionForm'
import { splitServerErrors } from './features/conversion/conversionFormModel'
import { useExchangeRates, type ExchangeRatesState } from './features/conversion/useExchangeRates'

// The table and the chart are large and only needed after the first conversion, so they are loaded on demand.
const ResultsView = lazy(() =>
  import('./features/results/ResultsView').then((module) => ({ default: module.ResultsView })),
)

/** Hidden on screen, read by screen readers. */
const visuallyHidden = {
  position: 'absolute',
  width: 1,
  height: 1,
  overflow: 'hidden',
  clip: 'rect(0 0 0 0)',
  whiteSpace: 'nowrap',
} as const

/** A short announcement for screen readers; the results themselves are not a live region. */
function statusText(state: ExchangeRatesState): string {
  switch (state.status) {
    case 'loading':
      return 'Wczytywanie kursów…'
    case 'success':
      return `Liczba wczytanych kursów ${state.history.sourceCurrency} → ${state.history.targetCurrency}: ${state.history.rates.length}.`
    default:
      // Empty results and errors are announced by their alerts.
      return ''
  }
}

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

        <Box role="status" aria-live="polite" sx={visuallyHidden}>
          {statusText(state)}
        </Box>

        <Box sx={{ mt: 3 }}>
          {state.status === 'empty' && (
            <Alert severity="info">
              Brak kursów w wybranym okresie – dostawca nie publikuje kursów w weekendy i święta, a kursy z bieżącego
              dnia mogą jeszcze nie być dostępne. Wybierz inny zakres dat.
            </Alert>
          )}
          {state.status === 'error' && <ErrorAlert state={state} />}
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
            Źródło danych: dostawca wybrany w formularzu, domyślnie{' '}
            <Link href="https://api.nbp.pl/" color="inherit">
              Narodowy Bank Polski
            </Link>
            .
          </Typography>
        </Container>
      </Box>
    </Box>
  )
}

function ErrorAlert({ state }: { state: Extract<ExchangeRatesState, { status: 'error' }> }) {
  const { error } = state
  // Messages shown under form fields are not repeated here; any others would otherwise be lost.
  const otherMessages = splitServerErrors(error.fieldErrors).other

  return (
    <Alert severity="error">
      {error.message}
      {otherMessages.length > 0 && (
        <Box component="ul" sx={{ m: 0, mt: 0.5, pl: 2.5 }}>
          {otherMessages.map((message) => (
            <li key={message}>{message}</li>
          ))}
        </Box>
      )}
      {/* Server-side failures are logged with this id (README: Logi), which makes them easy to find. */}
      {error.status >= 500 && error.traceId && (
        <Typography variant="body2" sx={{ mt: 0.5, opacity: 0.8 }}>
          Identyfikator błędu: {error.traceId}
        </Typography>
      )}
    </Alert>
  )
}

export default App
