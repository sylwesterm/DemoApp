import { render } from '@testing-library/react'
import type { ReactElement } from 'react'
import { AppProviders } from '../AppProviders'

/** Renders a component the way the application does - with the MUI theme and Polish dates. */
export function renderWithProviders(ui: ReactElement) {
  return render(ui, { wrapper: AppProviders })
}
