import { useEffect, useState } from 'react'
import { ApiError } from '../api/ApiError'

export interface ApiData<T> {
  data: T | undefined
  loading: boolean
  error: ApiError | undefined
}

/** A finished request, remembered together with the key it was made for. */
interface Settled<T> {
  key: string
  data?: T
  error?: ApiError
}

/**
 * Loads data for `key` and reloads when it changes; `null` means nothing to load yet.
 * A request for an outdated key is cancelled, and a result is only shown for the key it belongs to,
 * so a slow old response can never overwrite a newer one. `loading` is derived during render
 * instead of being set in the effect, which avoids an extra render on every change.
 * `load` must be stable (a module-level function), otherwise every render would start a new request.
 */
export function useApiData<T>(key: string | null, load: (key: string, signal: AbortSignal) => Promise<T>): ApiData<T> {
  const [settled, setSettled] = useState<Settled<T> | null>(null)

  useEffect(() => {
    if (key === null) return

    const controller = new AbortController()
    load(key, controller.signal).then(
      (data) => {
        if (!controller.signal.aborted) setSettled({ key, data })
      },
      (error: unknown) => {
        if (!controller.signal.aborted) setSettled({ key, error: toApiError(error) })
      },
    )

    return () => controller.abort()
  }, [key, load])

  const current = settled?.key === key ? settled : undefined
  return { data: current?.data, error: current?.error, loading: key !== null && current === undefined }
}

export function toApiError(error: unknown): ApiError {
  return error instanceof ApiError ? error : new ApiError(0, 'Wystąpił nieoczekiwany błąd. Spróbuj ponownie.')
}
