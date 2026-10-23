import CurrencyExchangeIcon from '@mui/icons-material/CurrencyExchange'
import { AppBar, Box, Container, Link, Toolbar, Typography } from '@mui/material'

function App() {
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
        <Typography color="text.secondary">
          Przeliczanie kursów walut i ich historia na podstawie danych dostawców kursów.
        </Typography>
        {/* The conversion form and the results are added in the next steps. */}
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
