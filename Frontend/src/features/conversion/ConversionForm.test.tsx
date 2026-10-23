import { screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import dayjs from 'dayjs'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { ApiError } from '../../api/ApiError'
import { getCurrencies, getProviders } from '../../api/client'
import type { Currency } from '../../api/types'
import { renderWithProviders } from '../../test/render'
import { ConversionForm } from './ConversionForm'

vi.mock('../../api/client')

const currencies: Currency[] = [
  { code: 'EUR', name: 'euro', label: 'EUR - euro' },
  { code: 'PLN', name: 'złoty polski', label: 'PLN - złoty polski' },
  { code: 'USD', name: 'dolar amerykański', label: 'USD - dolar amerykański' },
]

beforeEach(() => {
  vi.mocked(getProviders).mockResolvedValue([
    { key: 'ECB', displayName: 'Europejski Bank Centralny', baseCurrencyCode: 'EUR' },
    { key: 'NBP', displayName: 'Narodowy Bank Polski', baseCurrencyCode: 'PLN' },
  ])
  vi.mocked(getCurrencies).mockResolvedValue(currencies)
})

const sourceField = () => screen.getByRole('combobox', { name: /Waluta źródłowa/ })
const targetField = () => screen.getByRole('combobox', { name: /Waluta docelowa/ })

/** Renders the form and waits until the providers are loaded - the currency fields are disabled before that. */
async function renderForm(props: Partial<Parameters<typeof ConversionForm>[0]> = {}) {
  const onSubmit = vi.fn()
  renderWithProviders(<ConversionForm onSubmit={onSubmit} {...props} />)
  await waitFor(() => expect(sourceField()).toBeEnabled())
  return { onSubmit, user: userEvent.setup() }
}

async function choose(user: ReturnType<typeof userEvent.setup>, field: HTMLElement, option: string) {
  await user.click(field)
  await user.click(await screen.findByRole('option', { name: option }))
}

describe('ConversionForm', () => {
  it('preselects NBP and loads its currencies', async () => {
    await renderForm()

    await waitFor(() =>
      expect(screen.getByRole('combobox', { name: /Dostawca kursów/ })).toHaveTextContent('Narodowy Bank Polski'),
    )
    expect(getCurrencies).toHaveBeenCalledWith('NBP', expect.any(AbortSignal))
  })

  it('finds a currency by its name', async () => {
    const { user } = await renderForm()

    await user.type(sourceField(), 'dolar')

    const options = await screen.findAllByRole('option')
    expect(options.map((option) => option.textContent)).toEqual(['USD - dolar amerykański'])
  })

  it('shows Polish messages instead of sending an incomplete form', async () => {
    const { onSubmit, user } = await renderForm()

    await user.click(screen.getByRole('button', { name: 'Przelicz' }))

    expect(await screen.findByText('Wybierz walutę źródłową.')).toBeInTheDocument()
    expect(screen.getByText('Wybierz walutę docelową.')).toBeInTheDocument()
    expect(onSubmit).not.toHaveBeenCalled()
  })

  it('does not allow the same source and target currency', async () => {
    const { onSubmit, user } = await renderForm()
    await choose(user, sourceField(), 'EUR - euro')
    await choose(user, targetField(), 'EUR - euro')

    await user.click(screen.getByRole('button', { name: 'Przelicz' }))

    expect(await screen.findByText('Waluta docelowa musi być inna niż waluta źródłowa.')).toBeInTheDocument()
    expect(onSubmit).not.toHaveBeenCalled()
  })

  it('sends the chosen currencies and today as both dates', async () => {
    const { onSubmit, user } = await renderForm()
    await choose(user, sourceField(), 'EUR - euro')
    await choose(user, targetField(), 'USD - dolar amerykański')

    await user.click(screen.getByRole('button', { name: 'Przelicz' }))

    const today = dayjs().format('YYYY-MM-DD')
    expect(onSubmit).toHaveBeenCalledWith({
      provider: 'NBP',
      sourceCurrency: 'EUR',
      targetCurrency: 'USD',
      dateFrom: today,
      dateTo: today,
    })
  })

  it('swaps the currencies', async () => {
    const { user } = await renderForm()
    await choose(user, sourceField(), 'EUR - euro')
    await choose(user, targetField(), 'USD - dolar amerykański')

    await user.click(screen.getByRole('button', { name: 'Zamień waluty' }))

    expect(sourceField()).toHaveValue('USD - dolar amerykański')
    expect(targetField()).toHaveValue('EUR - euro')
  })

  it('shows why the currencies could not be loaded', async () => {
    vi.mocked(getCurrencies).mockRejectedValue(new ApiError(504, 'Serwis NBP nie odpowiedział w wymaganym czasie.'))

    await renderForm()

    expect(await screen.findByRole('alert')).toHaveTextContent('Serwis NBP nie odpowiedział w wymaganym czasie.')
  })

  it('shows validation messages from the server next to their fields', async () => {
    await renderForm({ serverErrors: { dateFrom: ['Zakres dat nie może przekraczać 366 dni.'] } })

    const dateFrom = screen.getByRole('group', { name: 'Data od' })
    const field = dateFrom.closest('.MuiFormControl-root') as HTMLElement
    expect(within(field).getByText('Zakres dat nie może przekraczać 366 dni.')).toBeInTheDocument()
  })
})
