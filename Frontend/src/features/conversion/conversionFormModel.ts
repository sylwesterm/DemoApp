import dayjs, { type Dayjs } from 'dayjs'
import type { Currency, ExchangeRatesQuery, Provider } from '../../api/types'

/** Same limit as the backend validator. */
export const MAX_RANGE_DAYS = 366

/** Provider preselected in the drop-down, as the specification requires. */
export const DEFAULT_PROVIDER_KEY = 'NBP'

export interface ConversionFormValues {
  /** Provider key chosen by the user; empty until they choose, then the default provider is used. */
  provider: string
  sourceCurrency: Currency | null
  targetCurrency: Currency | null
  /** Empty means today - the backend applies the same rule. */
  dateFrom: Dayjs | null
  dateTo: Dayjs | null
}

/** Field names equal the API query parameters, so server validation errors land on the same fields. */
export type ConversionField = 'provider' | 'sourceCurrency' | 'targetCurrency' | 'dateFrom' | 'dateTo'

export type ConversionFormErrors = Partial<Record<ConversionField, string>>

const FIELDS: readonly ConversionField[] = ['provider', 'sourceCurrency', 'targetCurrency', 'dateFrom', 'dateTo']

/** Backend validation keys that differ from the form's field names. */
const SERVER_KEY_ALIASES: Readonly<Record<string, ConversionField>> = { providerKey: 'provider' }

/**
 * Today's date in Poland - the backend's "today" (rates are published in Polish time). Using the browser's
 * time zone instead would make a user east of Poland send "tomorrow" just after their midnight.
 */
export function todayInPoland(now: Date = new Date()): Dayjs {
  // en-CA formats as YYYY-MM-DD.
  const isoDate = new Intl.DateTimeFormat('en-CA', {
    timeZone: 'Europe/Warsaw',
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
  }).format(now)
  return dayjs(isoDate)
}

export function initialValues(today: Dayjs): ConversionFormValues {
  return { provider: '', sourceCurrency: null, targetCurrency: null, dateFrom: today, dateTo: today }
}

export function pickDefaultProvider(providers: readonly Provider[] | undefined): string {
  if (!providers || providers.length === 0) return ''
  return providers.find((provider) => provider.key === DEFAULT_PROVIDER_KEY)?.key ?? providers[0].key
}

/**
 * Checks the form before sending it. Messages and the order of the date rules are the same as in the backend
 * validator (GetExchangeRatesQueryValidator), so the user sees the same text whichever side catches the problem.
 */
export function validate(values: ConversionFormValues, today: Dayjs): ConversionFormErrors {
  const errors: ConversionFormErrors = {}

  if (!values.provider) {
    errors.provider = 'Należy wybrać dostawcę kursów walut.'
  }

  if (!values.sourceCurrency) {
    errors.sourceCurrency = 'Waluta źródłowa jest wymagana.'
  }

  if (!values.targetCurrency) {
    errors.targetCurrency = 'Waluta docelowa jest wymagana.'
  } else if (values.sourceCurrency?.code === values.targetCurrency.code) {
    errors.targetCurrency = 'Waluta docelowa musi być inna niż waluta źródłowa.'
  }

  Object.assign(errors, validateDates(values.dateFrom, values.dateTo, today))
  return errors
}

function validateDates(dateFrom: Dayjs | null, dateTo: Dayjs | null, today: Dayjs): ConversionFormErrors {
  // A half-typed date in the field is an invalid Dayjs, not null; the backend never sees such a value.
  if (dateFrom && !dateFrom.isValid()) return { dateFrom: 'Nieprawidłowa data.' }
  if (dateTo && !dateTo.isValid()) return { dateTo: 'Nieprawidłowa data.' }

  const from = (dateFrom ?? today).startOf('day')
  const to = (dateTo ?? today).startOf('day')

  // Like the backend, only the first broken rule is reported - the others would repeat the same problem.
  if (to.isAfter(today, 'day')) return { dateTo: 'Data do nie może być datą z przyszłości.' }
  if (from.isAfter(to)) return { dateFrom: 'Data od nie może być późniejsza niż data do.' }
  if (to.diff(from, 'day') + 1 > MAX_RANGE_DAYS) {
    return { dateFrom: `Zakres dat nie może przekraczać ${MAX_RANGE_DAYS} dni.` }
  }

  return {}
}

/**
 * Splits the backend's validation messages into those shown under a form field and the rest,
 * which the page shows next to the error message so that none is lost.
 */
export function splitServerErrors(fieldErrors: Readonly<Record<string, string[]>>): {
  byField: ConversionFormErrors
  other: string[]
} {
  const byField: ConversionFormErrors = {}
  const other: string[] = []

  for (const [key, messages] of Object.entries(fieldErrors)) {
    const field = SERVER_KEY_ALIASES[key] ?? (FIELDS as readonly string[]).find((name) => name === key)
    if (field && messages.length > 0) {
      byField[field as ConversionField] ??= messages[0]
    } else {
      other.push(...messages)
    }
  }

  return { byField, other }
}

/** Builds the API query from valid values; an empty date is left out, so the backend uses today. */
export function toQuery(values: ConversionFormValues): ExchangeRatesQuery {
  return {
    provider: values.provider,
    sourceCurrency: values.sourceCurrency!.code,
    targetCurrency: values.targetCurrency!.code,
    dateFrom: values.dateFrom?.format('YYYY-MM-DD'),
    dateTo: values.dateTo?.format('YYYY-MM-DD'),
  }
}
