import dayjs from 'dayjs'
import { describe, expect, it } from 'vitest'
import type { Currency } from '../../api/types'
import {
  initialValues,
  pickDefaultProvider,
  splitServerErrors,
  todayInPoland,
  toQuery,
  validate,
  type ConversionFormValues,
} from './conversionFormModel'

const today = dayjs('2026-10-16')
const eur: Currency = { code: 'EUR', name: 'euro', label: 'EUR - euro' }
const usd: Currency = { code: 'USD', name: 'dolar amerykański', label: 'USD - dolar amerykański' }

function values(overrides: Partial<ConversionFormValues> = {}): ConversionFormValues {
  return { ...initialValues(today), provider: 'NBP', sourceCurrency: eur, targetCurrency: usd, ...overrides }
}

describe('initialValues', () => {
  it('starts with both dates set to today and no currencies', () => {
    const initial = initialValues(today)

    expect(initial.dateFrom?.isSame(today, 'day')).toBe(true)
    expect(initial.dateTo?.isSame(today, 'day')).toBe(true)
    expect(initial.sourceCurrency).toBeNull()
    expect(initial.targetCurrency).toBeNull()
  })
})

describe('pickDefaultProvider', () => {
  it('prefers NBP, as the specification requires', () => {
    const providers = [
      { key: 'ECB', displayName: 'Europejski Bank Centralny', baseCurrencyCode: 'EUR' },
      { key: 'NBP', displayName: 'Narodowy Bank Polski', baseCurrencyCode: 'PLN' },
    ]

    expect(pickDefaultProvider(providers)).toBe('NBP')
  })

  it('falls back to the first provider, or nothing while the list is loading', () => {
    expect(pickDefaultProvider([{ key: 'ECB', displayName: 'EBC', baseCurrencyCode: 'EUR' }])).toBe('ECB')
    expect(pickDefaultProvider(undefined)).toBe('')
  })
})

describe('validate', () => {
  it('accepts a complete form', () => {
    expect(validate(values(), today)).toEqual({})
  })

  it('requires both currencies, with the same messages as the backend', () => {
    expect(validate(values({ sourceCurrency: null, targetCurrency: null }), today)).toEqual({
      sourceCurrency: 'Waluta źródłowa jest wymagana.',
      targetCurrency: 'Waluta docelowa jest wymagana.',
    })
  })

  it('rejects the same source and target currency', () => {
    expect(validate(values({ targetCurrency: eur }), today)).toEqual({
      targetCurrency: 'Waluta docelowa musi być inna niż waluta źródłowa.',
    })
  })

  it('treats empty dates as today', () => {
    expect(validate(values({ dateFrom: null, dateTo: null }), today)).toEqual({})
  })

  it('rejects a future "date to"', () => {
    expect(validate(values({ dateTo: today.add(1, 'day') }), today)).toEqual({
      dateTo: 'Data do nie może być datą z przyszłości.',
    })
  })

  it('reports a future "date from" with an empty "date to" (= today) as a reversed range, like the backend', () => {
    expect(validate(values({ dateFrom: today.add(1, 'day'), dateTo: null }), today)).toEqual({
      dateFrom: 'Data od nie może być późniejsza niż data do.',
    })
  })

  it('rejects "date from" later than "date to"', () => {
    expect(validate(values({ dateFrom: today, dateTo: today.subtract(1, 'day') }), today)).toEqual({
      dateFrom: 'Data od nie może być późniejsza niż data do.',
    })
  })

  it('allows at most 366 days', () => {
    expect(validate(values({ dateFrom: today.subtract(365, 'day') }), today)).toEqual({})
    expect(validate(values({ dateFrom: today.subtract(366, 'day') }), today)).toEqual({
      dateFrom: 'Zakres dat nie może przekraczać 366 dni.',
    })
  })

  it('reports a half-typed date as invalid', () => {
    expect(validate(values({ dateFrom: dayjs('nieprawidłowa data') }), today)).toEqual({ dateFrom: 'Nieprawidłowa data.' })
  })
})

describe('toQuery', () => {
  it('sends currency codes and ISO dates', () => {
    expect(toQuery(values({ dateFrom: dayjs('2026-10-01') }))).toEqual({
      provider: 'NBP',
      sourceCurrency: 'EUR',
      targetCurrency: 'USD',
      dateFrom: '2026-10-01',
      dateTo: '2026-10-16',
    })
  })

  it('leaves out empty dates, so the backend uses today', () => {
    const query = toQuery(values({ dateFrom: null, dateTo: null }))

    expect(query.dateFrom).toBeUndefined()
    expect(query.dateTo).toBeUndefined()
  })
})

describe('todayInPoland', () => {
  it('uses the date in Poland, not in the browser time zone', () => {
    // 23:30 UTC on 16 October is already 01:30 on 17 October in Warsaw (CEST, UTC+2).
    expect(todayInPoland(new Date('2026-10-16T23:30:00Z')).format('YYYY-MM-DD')).toBe('2026-10-17')
    // 21:30 UTC is still 23:30 on 16 October in Warsaw.
    expect(todayInPoland(new Date('2026-10-16T21:30:00Z')).format('YYYY-MM-DD')).toBe('2026-10-16')
  })
})

describe('splitServerErrors', () => {
  it('puts messages under form fields, mapping the backend key providerKey to the provider field', () => {
    expect(
      splitServerErrors({
        dateFrom: ['Zakres dat nie może przekraczać 366 dni.'],
        providerKey: ['Należy wybrać dostawcę kursów walut.'],
      }),
    ).toEqual({
      byField: { dateFrom: 'Zakres dat nie może przekraczać 366 dni.', provider: 'Należy wybrać dostawcę kursów walut.' },
      other: [],
    })
  })

  it('keeps messages that belong to no field, so they can be shown elsewhere', () => {
    expect(splitServerErrors({ '': ['Ogólny błąd zapytania.'] })).toEqual({ byField: {}, other: ['Ogólny błąd zapytania.'] })
  })
})
