import { screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import App from './App'
import { renderWithProviders } from './test/render'

describe('App', () => {
  it('shows the Polish title and the data source', () => {
    renderWithProviders(<App />)

    expect(screen.getByRole('heading', { level: 1, name: 'Kursy walut' })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'API Narodowego Banku Polskiego' })).toHaveAttribute(
      'href',
      'https://api.nbp.pl/',
    )
  })
})
