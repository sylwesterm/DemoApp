import { screen } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import App from './App'
import { getCurrencies, getProviders } from './api/client'
import { renderWithProviders } from './test/render'

vi.mock('./api/client')

beforeEach(() => {
  vi.mocked(getProviders).mockResolvedValue([])
  vi.mocked(getCurrencies).mockResolvedValue([])
})

describe('App', () => {
  it('shows the Polish title, the form and the data source', async () => {
    renderWithProviders(<App />)

    expect(screen.getByRole('heading', { level: 1, name: 'Kursy walut' })).toBeInTheDocument()
    expect(await screen.findByRole('button', { name: 'Przelicz' })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'API Narodowego Banku Polskiego' })).toHaveAttribute(
      'href',
      'https://api.nbp.pl/',
    )
  })
})
