import { describe, expect, it } from 'vitest'
import { formatConversion, formatRate } from './format'

describe('formatRate', () => {
  it('uses the Polish decimal comma', () => {
    expect(formatRate(4.21)).toBe('4,21')
  })

  it('keeps up to 6 significant digits', () => {
    expect(formatRate(4.21345678)).toBe('4,21346')
  })

  it('keeps tiny cross rates readable', () => {
    expect(formatRate(0.0000567812)).toBe('0,0000567812')
  })
})

describe('formatConversion', () => {
  it('uses the format from the specification', () => {
    expect(formatConversion(4.21, 'EUR', 'PLN')).toBe('1 EUR - 4,21 PLN')
  })
})
