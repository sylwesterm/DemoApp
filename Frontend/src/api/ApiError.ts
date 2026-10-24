import type { ProblemDetails } from './types'

/** Status used when no HTTP response arrived at all (network failure, backend not running). */
export const NETWORK_ERROR_STATUS = 0

/**
 * A failed API call with a message ready to show to the user (in Polish).
 * The backend already sends Polish ProblemDetails; the fallbacks cover responses without them.
 */
export class ApiError extends Error {
  readonly status: number
  /** Validation messages per query parameter (400), empty for other errors. */
  readonly fieldErrors: Readonly<Record<string, string[]>>
  /** Correlates the error with the backend log file. */
  readonly traceId?: string

  constructor(status: number, message: string, fieldErrors: Record<string, string[]> = {}, traceId?: string) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.fieldErrors = fieldErrors
    this.traceId = traceId
  }

  static fromProblem(status: number, problem: ProblemDetails | null): ApiError {
    const message = problem?.detail ?? problem?.title ?? fallbackMessage(status)
    return new ApiError(status, message, problem?.errors ?? {}, problem?.traceId)
  }

  static network(): ApiError {
    return new ApiError(
      NETWORK_ERROR_STATUS,
      'Nie udało się połączyć z serwerem. Sprawdź połączenie i czy backend jest uruchomiony.',
    )
  }
}

/**
 * Message for a response without ProblemDetails. The backend sends ProblemDetails for all its errors
 * (including provider failures, 502/504), so a 5xx without them means the backend was not reached -
 * e.g. the Vite proxy answers 502 with an empty body when the backend is not running.
 */
function fallbackMessage(status: number): string {
  if (status >= 500) {
    return 'Serwer jest niedostępny lub nie odpowiada poprawnie. Sprawdź, czy backend jest uruchomiony, i spróbuj ponownie.'
  }

  switch (status) {
    case 400:
      return 'Nieprawidłowe parametry żądania.'
    case 404:
      return 'Nie znaleziono zasobu.'
    case 429:
      return 'Zbyt wiele zapytań. Odczekaj chwilę i spróbuj ponownie.'
    default:
      return `Serwer zwrócił nieoczekiwaną odpowiedź (kod ${status}).`
  }
}
