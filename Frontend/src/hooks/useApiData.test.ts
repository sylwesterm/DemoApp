import { act, renderHook, waitFor } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { ApiError } from '../api/ApiError'
import { useApiData } from './useApiData'

/** A promise the test resolves or rejects by hand, to control the order in which responses arrive. */
function deferred<T>() {
  let resolve!: (value: T) => void
  let reject!: (reason: unknown) => void
  const promise = new Promise<T>((res, rej) => {
    resolve = res
    reject = rej
  })
  return { promise, resolve, reject }
}

describe('useApiData', () => {
  it('ignores a slow response for a key that is no longer current', async () => {
    const nbp = deferred<string>()
    const ecb = deferred<string>()
    const load = vi.fn((key: string) => (key === 'NBP' ? nbp.promise : ecb.promise))
    const { result, rerender } = renderHook(({ key }) => useApiData(key, load), { initialProps: { key: 'NBP' } })

    rerender({ key: 'ECB' })
    await act(async () => ecb.resolve('waluty ECB'))
    await act(async () => nbp.resolve('waluty NBP')) // arrives last, but is outdated

    expect(result.current.data).toBe('waluty ECB')
  })

  it('cancels the request for a key that is no longer current', () => {
    const signals: AbortSignal[] = []
    const load = vi.fn((_key: string, signal: AbortSignal) => {
      signals.push(signal)
      return new Promise<string>(() => {})
    })
    const { rerender } = renderHook(({ key }) => useApiData(key, load), { initialProps: { key: 'NBP' } })

    rerender({ key: 'ECB' })

    expect(signals[0].aborted).toBe(true)
    expect(signals[1].aborted).toBe(false)
  })

  it('loads again on reload after an error', async () => {
    const load = vi
      .fn<(key: string, signal: AbortSignal) => Promise<string>>()
      .mockRejectedValueOnce(new ApiError(504, 'Przekroczono czas oczekiwania.'))
      .mockResolvedValueOnce('dane')
    const { result } = renderHook(() => useApiData('providers', load))
    await waitFor(() => expect(result.current.error?.message).toBe('Przekroczono czas oczekiwania.'))

    act(() => result.current.reload())

    expect(result.current.loading).toBe(true)
    await waitFor(() => expect(result.current.data).toBe('dane'))
    expect(result.current.error).toBeUndefined()
  })

  it('loads nothing for a null key', () => {
    const load = vi.fn()
    const { result } = renderHook(() => useApiData(null, load))

    expect(load).not.toHaveBeenCalled()
    expect(result.current.loading).toBe(false)
  })
})
