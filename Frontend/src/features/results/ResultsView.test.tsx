import { screen, within } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import type { ExchangeRateHistory } from '../../api/types'
import { renderWithProviders } from '../../test/render'
import { ResultsView } from './ResultsView'

const history: ExchangeRateHistory = {
  provider: 'NBP',
  sourceCurrency: 'EUR',
  targetCurrency: 'USD',
  dateFrom: '2026-10-14',
  dateTo: '2026-10-15',
  statistics: { minimum: 1.075, maximum: 1.2, average: 1.1375 },
  rates: [
    { date: '2026-10-15', formattedDate: '15.10.2026', value: 1.075 },
    { date: '2026-10-14', formattedDate: '14.10.2026', value: 1.2 },
  ],
}

describe('ResultsView', () => {
  it('names the currency pair and the period of the rates', () => {
    renderWithProviders(<ResultsView history={history} />)

    expect(screen.getByRole('heading', { level: 2, name: 'EUR → USD' })).toBeInTheDocument()
    // The rows arrive out of order on purpose: the period and the chart must not depend on it.
    expect(screen.getByText(/Kursy z okresu 14\.10\.2026 – 15\.10\.2026 · liczba notowań: 2/)).toBeInTheDocument()
  })

  it('shows the minimum, maximum and average rate', () => {
    renderWithProviders(<ResultsView history={history} />)

    expect(within(screen.getByRole('group', { name: 'Minimum' })).getByText('1,075')).toBeInTheDocument()
    expect(within(screen.getByRole('group', { name: 'Maksimum' })).getByText('1,2')).toBeInTheDocument()
    expect(within(screen.getByRole('group', { name: 'Średnia' })).getByText('1,1375')).toBeInTheDocument()
    expect(screen.getAllByText('USD za 1 EUR')).toHaveLength(3)
  })

  it('lists the rates by date in the configured format and the specification format', () => {
    renderWithProviders(<ResultsView history={history} />)

    const table = screen.getByRole('grid', { name: 'Tabela kursów EUR → USD' })
    const rows = within(table).getAllByRole('row').slice(1)
    expect(rows.map((row) => within(row).getAllByRole('gridcell').map((cell) => cell.textContent))).toEqual([
      ['14.10.2026', '1 EUR - 1,2 USD'],
      ['15.10.2026', '1 EUR - 1,075 USD'],
    ])
  })

  it('pages the table with Polish labels', () => {
    renderWithProviders(<ResultsView history={history} />)

    expect(screen.getByText('Wierszy na stronę:')).toBeInTheDocument()
  })

  it('has a chart section', () => {
    renderWithProviders(<ResultsView history={history} />)

    expect(screen.getByRole('heading', { level: 3, name: 'Wykres kursu' })).toBeInTheDocument()
  })
})
