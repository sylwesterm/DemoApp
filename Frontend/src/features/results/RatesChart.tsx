import { Box } from '@mui/material'
import { LineChart } from '@mui/x-charts/LineChart'
import dayjs from 'dayjs'
import type { ExchangeRate } from '../../api/types'
import { formatConversion, formatRate } from './format'

interface RatesChartProps {
  /** Rates in date order. */
  rates: readonly ExchangeRate[]
  source: string
  target: string
}

/** Up to about a month every rate gets a dot; on longer periods the dots would only blur the line. */
const MAX_POINTS_WITH_MARKS = 31

/** Above about four months the axis shows months; day labels would be crowded. */
const MAX_DAYS_WITH_DAY_TICKS = 120

export function RatesChart({ rates, source, target }: RatesChartProps) {
  // Local midnight (not new Date('yyyy-MM-dd'), which is UTC and may land on the previous day).
  const dates = rates.map((rate) => dayjs(rate.date).toDate())
  // The tooltip shows the date in the format configured in the backend, like the table.
  const formattedDates = new Map(rates.map((rate) => [rate.date, rate.formattedDate]))

  const first = dayjs(rates[0].date)
  const last = dayjs(rates[rates.length - 1].date)
  // Labels without a year would be ambiguous on a range that crosses New Year.
  const tickFormat =
    last.diff(first, 'day') > MAX_DAYS_WITH_DAY_TICKS ? 'MMM YYYY' : first.year() === last.year() ? 'D MMM' : 'D MMM YYYY'

  return (
    // The chart is drawn as SVG, so screen readers get a summary; the table below has the same data.
    <Box
      role="img"
      aria-label={`Wykres kursu ${source} → ${target} od ${rates[0].formattedDate} do ${rates[rates.length - 1].formattedDate}`}
    >
      <LineChart
        height={300}
        hideLegend
        grid={{ horizontal: true }}
        xAxis={[
          {
            data: dates,
            scaleType: 'time',
            valueFormatter: (date: Date, context) =>
              context.location === 'tick'
                ? dayjs(date).locale('pl').format(tickFormat)
                : (formattedDates.get(dayjs(date).format('YYYY-MM-DD')) ?? dayjs(date).format('YYYY-MM-DD')),
          },
        ]}
        yAxis={[{ valueFormatter: (value: number) => formatRate(value) }]}
        series={[
          {
            data: rates.map((rate) => rate.value),
            label: `Kurs ${source} → ${target}`,
            showMark: rates.length <= MAX_POINTS_WITH_MARKS,
            valueFormatter: (value) => (value === null ? '' : formatConversion(value, source, target)),
          },
        ]}
      />
    </Box>
  )
}
