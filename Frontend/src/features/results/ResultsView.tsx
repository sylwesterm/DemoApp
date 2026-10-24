import { Box, Paper, Stack, Typography } from '@mui/material'
import type { ExchangeRateHistory } from '../../api/types'
import { RatesChart } from './RatesChart'
import { RatesTable } from './RatesTable'
import { StatisticsCards } from './StatisticsCards'

/** Statistics, chart and table for a loaded period; the backend answers 204 instead of an empty list. */
export function ResultsView({ history }: { history: ExchangeRateHistory }) {
  const { sourceCurrency: source, targetCurrency: target, statistics } = history
  // The backend returns rates in date order, but the period and the line chart must not depend on it.
  // ISO dates sort correctly as text.
  const rates = [...history.rates].sort((a, b) => a.date.localeCompare(b.date))
  const first = rates[0]
  const last = rates[rates.length - 1]

  return (
    <Stack component="section" spacing={3} aria-labelledby="results-heading">
      <Box>
        <Typography id="results-heading" variant="h5" component="h2">
          {source} → {target}
        </Typography>
        <Typography color="text.secondary">
          Kursy z okresu {first.formattedDate} – {last.formattedDate} · liczba notowań: {rates.length}
        </Typography>
      </Box>

      <StatisticsCards statistics={statistics} source={source} target={target} />

      <Paper variant="outlined" sx={{ p: 2 }}>
        <Typography variant="subtitle1" component="h3" gutterBottom>
          Wykres kursu
        </Typography>
        <RatesChart rates={rates} source={source} target={target} />
      </Paper>

      <Box>
        <Typography variant="subtitle1" component="h3" gutterBottom>
          Kursy w poszczególnych dniach
        </Typography>
        <RatesTable rates={rates} source={source} target={target} />
      </Box>
    </Stack>
  )
}
