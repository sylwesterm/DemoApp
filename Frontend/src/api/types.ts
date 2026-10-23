// Shapes of the backend JSON (ASP.NET Core, camelCase). Dates are ISO strings, e.g. "2026-10-16".

/** GET /api/providers */
export interface Provider {
  key: string
  displayName: string
  baseCurrencyCode: string
}

/** GET /api/providers/{provider}/currencies */
export interface Currency {
  code: string
  name: string
  /** "code - name", e.g. "EUR - euro". */
  label: string
}

export interface ExchangeRate {
  date: string
  /** The date in the format set in the backend configuration (Display:DateFormat). */
  formattedDate: string
  /** Value of 1 unit of the source currency in the target currency. */
  value: number
}

export interface RateStatistics {
  minimum: number
  maximum: number
  average: number
}

/** GET /api/providers/{provider}/rates */
export interface ExchangeRateHistory {
  provider: string
  sourceCurrency: string
  targetCurrency: string
  dateFrom: string
  dateTo: string
  statistics: RateStatistics
  rates: ExchangeRate[]
}

export interface ExchangeRatesQuery {
  provider: string
  sourceCurrency: string
  targetCurrency: string
  /** ISO date; the backend uses today (in Poland) when omitted. */
  dateFrom?: string
  /** ISO date; the backend uses today (in Poland) when omitted. */
  dateTo?: string
}

/** RFC 9457 ProblemDetails returned by the backend for every error. */
export interface ProblemDetails {
  type?: string
  title?: string
  status?: number
  detail?: string
  traceId?: string
  /** Validation messages keyed by query parameter, e.g. { sourceCurrency: ["Waluta źródłowa jest wymagana."] }. */
  errors?: Record<string, string[]>
}
