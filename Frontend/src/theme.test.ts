import { describe, expect, it } from 'vitest'
import { theme } from './theme'

describe('theme', () => {
  it('uses Polish texts in MUI components', () => {
    expect(theme.components?.MuiTablePagination?.defaultProps?.labelRowsPerPage).toBe('Wierszy na stronę:')
    expect(theme.components?.MuiAutocomplete?.defaultProps?.noOptionsText).toBe('Brak opcji')
  })

  it('uses Polish texts in the date pickers', () => {
    expect(theme.components?.MuiLocalizationProvider?.defaultProps?.localeText?.cancelButtonLabel).toBe('Anuluj')
  })

  it('uses Polish texts in the charts', () => {
    expect(theme.components?.MuiChartsLocalizationProvider?.defaultProps?.localeText?.noData).toBe(
      'Brak danych do wyświetlenia',
    )
  })

  it('provides light and dark colour schemes', () => {
    expect(theme).toHaveProperty('colorSchemes.light')
    expect(theme).toHaveProperty('colorSchemes.dark')
  })
})
