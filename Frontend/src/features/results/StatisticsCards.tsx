import ArrowDownwardIcon from '@mui/icons-material/ArrowDownward'
import ArrowUpwardIcon from '@mui/icons-material/ArrowUpward'
import FunctionsIcon from '@mui/icons-material/Functions'
import { Box, Paper, Typography } from '@mui/material'
import type { ReactNode } from 'react'
import type { RateStatistics } from '../../api/types'
import { formatRate } from './format'

interface StatisticsCardsProps {
  statistics: RateStatistics
  source: string
  target: string
}

export function StatisticsCards({ statistics, source, target }: StatisticsCardsProps) {
  const card = (label: string, value: number, icon: ReactNode) => (
    <Paper variant="outlined" sx={{ p: 2 }} role="group" aria-label={label}>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, color: 'text.secondary' }}>
        {icon}
        <Typography variant="overline" component="span">
          {label}
        </Typography>
      </Box>
      <Typography variant="h5" component="p" sx={{ fontWeight: 600 }}>
        {formatRate(value)}
      </Typography>
      <Typography variant="body2" color="text.secondary">
        {target} za 1 {source}
      </Typography>
    </Paper>
  )

  return (
    <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: 'repeat(3, 1fr)' }, gap: 2 }}>
      {card('Minimum', statistics.minimum, <ArrowDownwardIcon fontSize="small" />)}
      {card('Maksimum', statistics.maximum, <ArrowUpwardIcon fontSize="small" />)}
      {card('Średnia', statistics.average, <FunctionsIcon fontSize="small" />)}
    </Box>
  )
}
