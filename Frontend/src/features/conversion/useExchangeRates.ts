import { useCallback, useEffect, useRef, useState } from 'react'
import type { ApiError } from '../../api/ApiError'
import { getExchangeRates } from '../../api/client'
import type { ExchangeRateHistory, ExchangeRatesQuery } from '../../api/types'
import { toApiError } from '../../hooks/useApiData'

export type ExchangeRatesState =
  | { status: 'idle' }
  | { status: 'loading' }
  | { status: 'success'; history: ExchangeRateHistory }
  /** HTTP 204 - the provider published no rates in the period. */
  | { status: 'empty' }
  | { status: 'error'; error: ApiError }

/** Loads rates on demand; a new request cancels the previous one, so only the latest answer is shown. */
export function useExchangeRates() {
  const [state, setState] = useState<ExchangeRatesState>({ status: 'idle' })
  const current = useRef<AbortController | null>(null)

  useEffect(() => () => current.current?.abort(), [])

  const load = useCallback(async (query: ExchangeRatesQuery) => {
    current.current?.abort()
    const controller = new AbortController()
    current.current = controller
    setState({ status: 'loading' })

    try {
      const history = await getExchangeRates(query, controller.signal)
      if (controller.signal.aborted) return
      setState(history ? { status: 'success', history } : { status: 'empty' })
    } catch (error) {
      if (controller.signal.aborted) return
      setState({ status: 'error', error: toApiError(error) })
    }
  }, [])

  return { state, load }
}
