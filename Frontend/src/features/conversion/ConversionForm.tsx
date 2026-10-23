import SwapHorizIcon from '@mui/icons-material/SwapHoriz'
import { Alert, Autocomplete, Box, Button, IconButton, MenuItem, Paper, Stack, TextField, Tooltip, Typography } from '@mui/material'
import { DatePicker } from '@mui/x-date-pickers/DatePicker'
import dayjs from 'dayjs'
import { useState, type FormEvent } from 'react'
import { getCurrencies, getProviders } from '../../api/client'
import type { Currency, ExchangeRatesQuery } from '../../api/types'
import { useApiData } from '../../hooks/useApiData'
import {
  initialValues,
  pickDefaultProvider,
  toQuery,
  validate,
  type ConversionField,
  type ConversionFormValues,
} from './conversionFormModel'

// Module-level loaders keep their identity between renders, as useApiData requires.
const loadProviders = (_key: string, signal: AbortSignal) => getProviders(signal)
const loadCurrencies = (provider: string, signal: AbortSignal) => getCurrencies(provider, signal)

interface ConversionFormProps {
  onSubmit: (query: ExchangeRatesQuery) => void
  /** Shows the button as busy while the rates are being loaded. */
  submitting?: boolean
  /** Validation messages from the backend, keyed by query parameter (e.g. `dateFrom`). */
  serverErrors?: Readonly<Record<string, string[]>>
}

export function ConversionForm({ onSubmit, submitting = false, serverErrors = {} }: ConversionFormProps) {
  const [values, setValues] = useState(() => initialValues(dayjs()))
  // Errors appear after the first "Przelicz" and then follow every change.
  const [submitted, setSubmitted] = useState(false)

  const providers = useApiData('providers', loadProviders)
  const provider = values.provider || pickDefaultProvider(providers.data)
  const currencies = useApiData(provider || null, loadCurrencies)

  const current: ConversionFormValues = { ...values, provider }
  const errors = submitted ? validate(current, dayjs()) : {}
  const errorFor = (field: ConversionField) => errors[field] ?? serverErrors[field]?.[0]
  const loadError = providers.error ?? currencies.error

  function update(changes: Partial<ConversionFormValues>) {
    setValues((previous) => ({ ...previous, ...changes }))
  }

  function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setSubmitted(true)
    if (Object.keys(validate(current, dayjs())).length > 0) return
    onSubmit(toQuery(current))
  }

  const currencyField = (field: 'sourceCurrency' | 'targetCurrency', label: string) => (
    <Autocomplete<Currency>
      options={currencies.data ?? []}
      value={values[field]}
      onChange={(_event, currency) => update({ [field]: currency })}
      getOptionLabel={(currency) => currency.label}
      isOptionEqualToValue={(option, value) => option.code === value.code}
      loading={currencies.loading}
      disabled={!provider}
      fullWidth
      renderInput={(params) => (
        <TextField
          {...params}
          label={label}
          required
          error={Boolean(errorFor(field))}
          helperText={errorFor(field) ?? ' '}
        />
      )}
    />
  )

  const dateField = (field: 'dateFrom' | 'dateTo', label: string) => (
    <DatePicker
      label={label}
      value={values[field]}
      onChange={(date) => update({ [field]: date })}
      disableFuture
      slotProps={{
        field: { clearable: true },
        textField: { fullWidth: true, error: Boolean(errorFor(field)), helperText: errorFor(field) ?? ' ' },
      }}
    />
  )

  return (
    <Paper component="form" variant="outlined" onSubmit={handleSubmit} noValidate sx={{ p: { xs: 2, sm: 3 } }}>
      <Stack spacing={1}>
        {loadError && <Alert severity="error">{loadError.message}</Alert>}

        <TextField
          select
          label="Dostawca kursów (API)"
          value={providers.data ? provider : ''}
          onChange={(event) => update({ provider: event.target.value, sourceCurrency: null, targetCurrency: null })}
          disabled={!providers.data}
          error={Boolean(errorFor('provider'))}
          helperText={errorFor('provider') ?? ' '}
        >
          {(providers.data ?? []).map((item) => (
            <MenuItem key={item.key} value={item.key}>
              {item.displayName}
            </MenuItem>
          ))}
        </TextField>

        <Box
          sx={{
            display: 'grid',
            gridTemplateColumns: { xs: '1fr', sm: '1fr auto 1fr' },
            alignItems: 'start',
            columnGap: 1,
          }}
        >
          {currencyField('sourceCurrency', 'Waluta źródłowa')}
          <Tooltip title="Zamień waluty">
            <span>
              <IconButton
                aria-label="Zamień waluty"
                onClick={() => update({ sourceCurrency: values.targetCurrency, targetCurrency: values.sourceCurrency })}
                disabled={!values.sourceCurrency && !values.targetCurrency}
                sx={{ mt: 1, mb: { xs: 1, sm: 0 }, justifySelf: 'center' }}
              >
                <SwapHorizIcon />
              </IconButton>
            </span>
          </Tooltip>
          {currencyField('targetCurrency', 'Waluta docelowa')}
        </Box>

        <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr' }, columnGap: 2 }}>
          {dateField('dateFrom', 'Data od')}
          {dateField('dateTo', 'Data do')}
        </Box>

        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 2, flexWrap: 'wrap' }}>
          <Typography variant="body2" color="text.secondary">
            Puste pole daty oznacza dzień dzisiejszy.
          </Typography>
          <Button type="submit" variant="contained" size="large" loading={submitting}>
            Przelicz
          </Button>
        </Box>
      </Stack>
    </Paper>
  )
}
