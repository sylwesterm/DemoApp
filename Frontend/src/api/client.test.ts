import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { ApiError, NETWORK_ERROR_STATUS } from './ApiError'
import { getCurrencies, getExchangeRates, getProviders } from './client'
import type { ExchangeRateHistory } from './types'

const fetchMock = vi.fn<typeof fetch>()

beforeEach(() => {
  vi.stubGlobal('fetch', fetchMock)
})

afterEach(() => {
  fetchMock.mockReset()
  vi.unstubAllGlobals()
})

function json(body: unknown, status = 200, contentType = 'application/json; charset=utf-8'): Response {
  return new Response(JSON.stringify(body), { status, headers: { 'Content-Type': contentType } })
}

function problem(status: number, body: object): Response {
  return json({ status, ...body }, status, 'application/problem+json')
}

function requestedUrl(): string {
  return String(fetchMock.mock.calls[0][0])
}

async function failure(promise: Promise<unknown>): Promise<ApiError> {
  const error = await promise.then(
    () => undefined,
    (reason: unknown) => reason,
  )
  expect(error).toBeInstanceOf(ApiError)
  return error as ApiError
}

const history: ExchangeRateHistory = {
  provider: 'NBP',
  sourceCurrency: 'EUR',
  targetCurrency: 'USD',
  dateFrom: '2026-10-14',
  dateTo: '2026-10-15',
  statistics: { minimum: 1.075, maximum: 1.2, average: 1.1375 },
  rates: [
    { date: '2026-10-14', formattedDate: '14.10.2026', value: 1.2 },
    { date: '2026-10-15', formattedDate: '15.10.2026', value: 1.075 },
  ],
}

describe('getProviders', () => {
  it('requests JSON from /api/providers and returns the list', async () => {
    const providers = [{ key: 'NBP', displayName: 'Narodowy Bank Polski', baseCurrencyCode: 'PLN' }]
    fetchMock.mockResolvedValue(json(providers))

    await expect(getProviders()).resolves.toEqual(providers)

    expect(requestedUrl()).toBe('/api/providers')
    expect(fetchMock.mock.calls[0][1]?.headers).toEqual({ Accept: 'application/json' })
  })
})

describe('getCurrencies', () => {
  it('puts the encoded provider key in the path', async () => {
    fetchMock.mockResolvedValue(json([]))

    await getCurrencies('NBP/ECB')

    expect(requestedUrl()).toBe('/api/providers/NBP%2FECB/currencies')
  })
})

describe('getExchangeRates', () => {
  it('sends currencies and dates as query parameters and returns the history', async () => {
    fetchMock.mockResolvedValue(json(history))

    const result = await getExchangeRates({
      provider: 'NBP',
      sourceCurrency: 'EUR',
      targetCurrency: 'USD',
      dateFrom: '2026-10-14',
      dateTo: '2026-10-15',
    })

    expect(result).toEqual(history)
    expect(requestedUrl()).toBe(
      '/api/providers/NBP/rates?sourceCurrency=EUR&targetCurrency=USD&dateFrom=2026-10-14&dateTo=2026-10-15',
    )
  })

  it('leaves out missing dates, so the backend uses today', async () => {
    fetchMock.mockResolvedValue(json(history))

    await getExchangeRates({ provider: 'NBP', sourceCurrency: 'EUR', targetCurrency: 'PLN' })

    expect(requestedUrl()).toBe('/api/providers/NBP/rates?sourceCurrency=EUR&targetCurrency=PLN')
  })

  it('returns null for 204 - no rates in the period', async () => {
    fetchMock.mockResolvedValue(new Response(null, { status: 204 }))

    await expect(getExchangeRates({ provider: 'NBP', sourceCurrency: 'EUR', targetCurrency: 'USD' })).resolves.toBeNull()
  })
})

describe('errors', () => {
  it('turns a validation problem into an ApiError with messages per field', async () => {
    fetchMock.mockResolvedValue(
      problem(400, {
        title: 'Nieprawidłowe parametry żądania.',
        errors: { targetCurrency: ['Waluta docelowa musi być inna niż waluta źródłowa.'] },
        traceId: '00-abc-01',
      }),
    )

    const error = await failure(getExchangeRates({ provider: 'NBP', sourceCurrency: 'EUR', targetCurrency: 'EUR' }))

    expect(error.status).toBe(400)
    expect(error.message).toBe('Nieprawidłowe parametry żądania.')
    expect(error.fieldErrors).toEqual({ targetCurrency: ['Waluta docelowa musi być inna niż waluta źródłowa.'] })
    expect(error.traceId).toBe('00-abc-01')
  })

  it('prefers the problem detail, which names the actual cause', async () => {
    fetchMock.mockResolvedValue(
      problem(404, { title: 'Nie znaleziono zasobu.', detail: "Nie znaleziono dostawcy kursów walut 'ECB'." }),
    )

    const error = await failure(getCurrencies('ECB'))

    expect(error.status).toBe(404)
    expect(error.message).toBe("Nie znaleziono dostawcy kursów walut 'ECB'.")
    expect(error.fieldErrors).toEqual({})
  })

  it('shows the provider failure described by the backend (502 with ProblemDetails)', async () => {
    fetchMock.mockResolvedValue(
      problem(502, { title: 'Błąd dostawcy kursów walut.', detail: 'Nie udało się połączyć z serwisem NBP.' }),
    )

    const error = await failure(getProviders())

    expect(error.status).toBe(502)
    expect(error.message).toBe('Nie udało się połączyć z serwisem NBP.')
  })

  it.each([500, 502, 504])(
    'reports the backend as unavailable for %i without ProblemDetails (e.g. the dev proxy with the backend down)',
    async (status) => {
      // What the Vite proxy really answers when the backend is not running: 502, text/plain, empty body.
      fetchMock.mockResolvedValue(new Response('', { status, headers: { 'Content-Type': 'text/plain' } }))

      const error = await failure(getProviders())

      expect(error.status).toBe(status)
      expect(error.message).toBe(
        'Serwer jest niedostępny lub nie odpowiada poprawnie. Sprawdź, czy backend jest uruchomiony, i spróbuj ponownie.',
      )
    },
  )

  it('uses a Polish message by status for a client error without ProblemDetails', async () => {
    fetchMock.mockResolvedValue(new Response('', { status: 404 }))

    const error = await failure(getProviders())

    expect(error.message).toBe('Nie znaleziono zasobu.')
  })

  it('reports a network failure with a Polish message', async () => {
    fetchMock.mockRejectedValue(new TypeError('Failed to fetch'))

    const error = await failure(getProviders())

    expect(error.status).toBe(NETWORK_ERROR_STATUS)
    expect(error.message).toBe('Nie udało się połączyć z serwerem. Sprawdź połączenie i czy backend jest uruchomiony.')
  })

  it('passes a cancellation through unchanged, so the caller can ignore it', async () => {
    const abort = new DOMException('The operation was aborted.', 'AbortError')
    fetchMock.mockRejectedValue(abort)

    await expect(getProviders(AbortSignal.abort())).rejects.toBe(abort)
  })
})
