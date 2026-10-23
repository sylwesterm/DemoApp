import type { Dayjs } from 'dayjs'
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

export function initialValues(today: Dayjs): ConversionFormValues {
  return { provider: '', sourceCurrency: null, targetCurrency: null, dateFrom: today, dateTo: today }
}

export function pickDefaultProvider(providers: readonly Provider[] | undefined): string {
  if (!providers || providers.length === 0) return ''
  return providers.find((provider) => provider.key === DEFAULT_PROVIDER_KEY)?.key ?? providers[0].key
}

/** Checks the form before sending it; messages mirror the backend validator. */
export function validate(values: ConversionFormValues, today: Dayjs): ConversionFormErrors {
  const errors: ConversionFormErrors = {}

  if (!values.provider) {
    errors.provider = 'Wybierz dostawcę kursów walut.'
  }

  if (!values.sourceCurrency) {
    errors.sourceCurrency = 'Wybierz walutę źródłową.'
  }

  if (!values.targetCurrency) {
    errors.targetCurrency = 'Wybierz walutę docelową.'
  } else if (values.sourceCurrency?.code === values.targetCurrency.code) {
    errors.targetCurrency = 'Waluta docelowa musi być inna niż waluta źródłowa.'
  }

  Object.assign(errors, validateDates(values.dateFrom, values.dateTo, today))
  return errors
}

function validateDates(dateFrom: Dayjs | null, dateTo: Dayjs | null, today: Dayjs): ConversionFormErrors {
  // A half-typed date in the field is an invalid Dayjs, not null.
  if (dateFrom && !dateFrom.isValid()) return { dateFrom: 'Nieprawidłowa data.' }
  if (dateTo && !dateTo.isValid()) return { dateTo: 'Nieprawidłowa data.' }

  const from = (dateFrom ?? today).startOf('day')
  const to = (dateTo ?? today).startOf('day')

  // Like the backend, only the first broken rule is reported - the others would repeat the same problem.
  if (to.isAfter(today, 'day')) return { dateTo: 'Data nie może być z przyszłości.' }
  if (from.isAfter(today, 'day')) return { dateFrom: 'Data nie może być z przyszłości.' }
  if (from.isAfter(to)) return { dateFrom: 'Data od nie może być późniejsza niż data do.' }
  if (to.diff(from, 'day') + 1 > MAX_RANGE_DAYS) {
    return { dateFrom: `Zakres dat nie może przekraczać ${MAX_RANGE_DAYS} dni.` }
  }

  return {}
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
