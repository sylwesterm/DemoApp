import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import App from './App'

describe('App', () => {
  it('shows the Polish title and the data source', () => {
    render(<App />)

    expect(screen.getByRole('heading', { level: 1, name: 'Kursy walut' })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'API Narodowego Banku Polskiego' })).toHaveAttribute(
      'href',
      'https://api.nbp.pl/',
    )
  })
})
