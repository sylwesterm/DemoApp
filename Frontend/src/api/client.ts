import { ApiError } from './ApiError'
import type { Currency, ExchangeRateHistory, ExchangeRatesQuery, ProblemDetails, Provider } from './types'

/** Relative URL: the Vite dev server proxies /api to the backend, so the browser stays on one origin. */
const API_BASE = '/api'

export function getProviders(signal?: AbortSignal): Promise<Provider[]> {
  return getJson<Provider[]>('/providers', signal)
}

export function getCurrencies(provider: string, signal?: AbortSignal): Promise<Currency[]> {
  return getJson<Currency[]>(`/providers/${encodeURIComponent(provider)}/currencies`, signal)
}

/** Returns `null` when the provider has no rates in the period (HTTP 204, e.g. a weekend). */
export function getExchangeRates(query: ExchangeRatesQuery, signal?: AbortSignal): Promise<ExchangeRateHistory | null> {
  const params = new URLSearchParams({
    sourceCurrency: query.sourceCurrency,
    targetCurrency: query.targetCurrency,
  })
  if (query.dateFrom) params.set('dateFrom', query.dateFrom)
  if (query.dateTo) params.set('dateTo', query.dateTo)

  return getJson<ExchangeRateHistory>(`/providers/${encodeURIComponent(query.provider)}/rates?${params}`, signal, {
    allowNoContent: true,
  })
}

async function getJson<T>(path: string, signal: AbortSignal | undefined, options: { allowNoContent: true }): Promise<T | null>
async function getJson<T>(path: string, signal: AbortSignal | undefined): Promise<T>
async function getJson<T>(
  path: string,
  signal: AbortSignal | undefined,
  options?: { allowNoContent: boolean },
): Promise<T | null> {
  let response: Response
  try {
    response = await fetch(`${API_BASE}${path}`, { headers: { Accept: 'application/json' }, signal })
  } catch (error) {
    // A cancelled request is not a failure - let the caller recognise and ignore it.
    if (error instanceof DOMException && error.name === 'AbortError') throw error
    throw ApiError.network()
  }

  if (!response.ok) {
    throw ApiError.fromProblem(response.status, await readProblem(response))
  }

  if (response.status === 204) {
    if (options?.allowNoContent) return null
    throw ApiError.fromProblem(response.status, null)
  }

  return (await response.json()) as T
}

/** The backend answers errors with ProblemDetails; anything else (e.g. a proxy error page) yields null. */
async function readProblem(response: Response): Promise<ProblemDetails | null> {
  const contentType = response.headers.get('Content-Type') ?? ''
  if (!contentType.includes('json')) return null

  try {
    return (await response.json()) as ProblemDetails
  } catch {
    return null
  }
}
