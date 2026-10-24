import { act, renderHook } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { getExchangeRates } from '../../api/client'
import type { ExchangeRateHistory, ExchangeRatesQuery } from '../../api/types'
import { useExchangeRates } from './useExchangeRates'

vi.mock('../../api/client')

const query = (sourceCurrency: string): ExchangeRatesQuery => ({ provider: 'NBP', sourceCurrency, targetCurrency: 'PLN' })

const history = (sourceCurrency: string): ExchangeRateHistory => ({
  provider: 'NBP',
  sourceCurrency,
  targetCurrency: 'PLN',
  dateFrom: '2026-10-16',
  dateTo: '2026-10-16',
  statistics: { minimum: 4.2, maximum: 4.2, average: 4.2 },
  rates: [{ date: '2026-10-16', formattedDate: '2026-10-16', value: 4.2 }],
})

beforeEach(() => {
  vi.mocked(getExchangeRates).mockReset()
})

describe('useExchangeRates', () => {
  it('cancels the previous request and shows only the latest answer', async () => {
    let resolveFirst!: (value: ExchangeRateHistory) => void
    vi.mocked(getExchangeRates)
      .mockImplementationOnce(() => new Promise((resolve) => (resolveFirst = resolve)))
      .mockResolvedValueOnce(history('USD'))
    const { result } = renderHook(() => useExchangeRates())

    await act(async () => {
      void result.current.load(query('EUR'))
    })
    await act(async () => result.current.load(query('USD')))
    await act(async () => resolveFirst(history('EUR'))) // the first answer arrives last

    expect(vi.mocked(getExchangeRates).mock.calls[0][1]?.aborted).toBe(true)
    expect(result.current.state).toEqual({ status: 'success', history: history('USD') })
  })

  it('reports no data (204) as an empty result', async () => {
    vi.mocked(getExchangeRates).mockResolvedValue(null)
    const { result } = renderHook(() => useExchangeRates())

    await act(async () => result.current.load(query('EUR')))

    expect(result.current.state).toEqual({ status: 'empty' })
  })
})
