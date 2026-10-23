import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import App from './App'
import { ApiError } from './api/ApiError'
import { getCurrencies, getExchangeRates, getProviders } from './api/client'
import { renderWithProviders } from './test/render'

vi.mock('./api/client')

beforeEach(() => {
  vi.mocked(getProviders).mockResolvedValue([
    { key: 'NBP', displayName: 'Narodowy Bank Polski', baseCurrencyCode: 'PLN' },
  ])
  vi.mocked(getCurrencies).mockResolvedValue([
    { code: 'EUR', name: 'euro', label: 'EUR - euro' },
    { code: 'USD', name: 'dolar amerykański', label: 'USD - dolar amerykański' },
  ])
})

/** Chooses EUR → USD and clicks "Przelicz". */
async function convert() {
  const user = userEvent.setup()
  const source = screen.getByRole('combobox', { name: /Waluta źródłowa/ })
  await waitFor(() => expect(source).toBeEnabled())
  await user.click(source)
  await user.click(await screen.findByRole('option', { name: 'EUR - euro' }))
  await user.click(screen.getByRole('combobox', { name: /Waluta docelowa/ }))
  await user.click(await screen.findByRole('option', { name: 'USD - dolar amerykański' }))
  await user.click(screen.getByRole('button', { name: 'Przelicz' }))
}

describe('App', () => {
  it('shows the Polish title, the form and the data source', async () => {
    renderWithProviders(<App />)

    expect(screen.getByRole('heading', { level: 1, name: 'Kursy walut' })).toBeInTheDocument()
    expect(await screen.findByRole('button', { name: 'Przelicz' })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'API Narodowego Banku Polskiego' })).toHaveAttribute(
      'href',
      'https://api.nbp.pl/',
    )
  })

  it('shows the results after a conversion', async () => {
    vi.mocked(getExchangeRates).mockResolvedValue({
      provider: 'NBP',
      sourceCurrency: 'EUR',
      targetCurrency: 'USD',
      dateFrom: '2026-10-16',
      dateTo: '2026-10-16',
      statistics: { minimum: 1.16, maximum: 1.16, average: 1.16 },
      rates: [{ date: '2026-10-16', formattedDate: '2026-10-16', value: 1.16 }],
    })
    renderWithProviders(<App />)

    await convert()

    // The results view is loaded lazily; the first load of the grid and chart modules in the test runner is slow.
    expect(
      await screen.findByRole('heading', { level: 2, name: 'EUR → USD' }, { timeout: 10_000 }),
    ).toBeInTheDocument()
  })

  it('explains that there are no rates in the period (204)', async () => {
    vi.mocked(getExchangeRates).mockResolvedValue(null)
    renderWithProviders(<App />)

    await convert()

    expect(await screen.findByText(/Brak kursów w wybranym okresie/)).toBeInTheDocument()
  })

  it('shows the error message and the field errors from the server', async () => {
    vi.mocked(getExchangeRates).mockRejectedValue(
      new ApiError(400, 'Nieprawidłowe parametry żądania.', { dateFrom: ['Zakres dat nie może przekraczać 366 dni.'] }),
    )
    renderWithProviders(<App />)

    await convert()

    expect(await screen.findByText('Nieprawidłowe parametry żądania.')).toBeInTheDocument()
    expect(screen.getByText('Zakres dat nie może przekraczać 366 dni.')).toBeInTheDocument()
  })
})
