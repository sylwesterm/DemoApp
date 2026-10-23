/**
 * Polish number format with up to 6 significant digits: "4,2135" for EUR→PLN, but also "0,0000567812"
 * for tiny cross rates such as IDR→EUR, which a fixed number of decimal places would show as 0.
 */
const rateFormat = new Intl.NumberFormat('pl-PL', { maximumSignificantDigits: 6 })

export function formatRate(value: number): string {
  return rateFormat.format(value)
}

/** The table format from the specification: "1 EUR - 4,21 PLN". */
export function formatConversion(value: number, source: string, target: string): string {
  return `1 ${source} - ${formatRate(value)} ${target}`
}
