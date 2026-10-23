import { DataGrid, type GridColDef } from '@mui/x-data-grid'
import { plPL } from '@mui/x-data-grid/locales'
import type { ExchangeRate } from '../../api/types'
import { formatConversion } from './format'

// Polish texts ("Wierszy na stronę:", "Brak danych") set here rather than in the theme: importing the grid's
// locale in the theme would put the whole grid into the start bundle, although it is loaded lazily.
const polishLocaleText = plPL.components.MuiDataGrid.defaultProps.localeText

interface RatesTableProps {
  rates: readonly ExchangeRate[]
  source: string
  target: string
}

const PAGE_SIZE_OPTIONS = [10, 25, 50, 100]

export function RatesTable({ rates, source, target }: RatesTableProps) {
  const columns: GridColDef<ExchangeRate>[] = [
    {
      // Sorted by the ISO date, displayed in the format configured in the backend (Display:DateFormat).
      field: 'date',
      headerName: 'Data',
      flex: 1,
      minWidth: 120,
      valueFormatter: (_value, row) => row.formattedDate,
    },
    {
      field: 'value',
      headerName: `Kurs ${source} → ${target}`,
      type: 'number',
      flex: 1,
      minWidth: 180,
      valueFormatter: (value: number) => formatConversion(value, source, target),
    },
  ]

  return (
    <DataGrid
      aria-label={`Tabela kursów ${source} → ${target}`}
      rows={rates}
      columns={columns}
      localeText={polishLocaleText}
      getRowId={(rate) => rate.date}
      initialState={{
        sorting: { sortModel: [{ field: 'date', sort: 'asc' }] },
        pagination: { paginationModel: { pageSize: PAGE_SIZE_OPTIONS[0] } },
      }}
      pageSizeOptions={PAGE_SIZE_OPTIONS}
      density="compact"
      disableRowSelectionOnClick
      disableColumnMenu
      autoHeight
    />
  )
}
