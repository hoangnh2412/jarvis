import { useEffect, useState } from 'react'
import { InputText } from 'primereact/inputtext'
import {
  formatNumberDisplay,
  formatNumberWhileTyping,
  toInvariantNumber,
} from '../utils/numberFormat'
import { getNumberDecimals } from '../utils/typeOptions'

type SettingNumberFieldProps = {
  value: string
  options?: string | null
  disabled?: boolean
  invalid?: boolean
  className?: string
  onChange: (invariantValue: string) => void
}

export function SettingNumberField({
  value,
  options,
  disabled = false,
  invalid = false,
  className,
  onChange,
}: SettingNumberFieldProps) {
  const decimals = getNumberDecimals(options)
  const [display, setDisplay] = useState(() =>
    formatNumberDisplay(value, decimals),
  )
  const [focused, setFocused] = useState(false)

  useEffect(() => {
    if (!focused) {
      setDisplay(formatNumberDisplay(value, decimals))
    }
  }, [value, decimals, focused])

  return (
    <InputText
      unstyled
      type="text"
      inputMode="decimal"
      className={className}
      value={display}
      disabled={disabled}
      aria-invalid={invalid || undefined}
      onFocus={() => setFocused(true)}
      onBlur={() => {
        setFocused(false)
        if (!display.trim()) {
          onChange('')
          setDisplay('')
          return
        }
        const invariant = toInvariantNumber(display, decimals)
        if (invariant === null) {
          setDisplay(formatNumberDisplay(value, decimals))
          return
        }
        onChange(invariant)
        setDisplay(formatNumberDisplay(invariant, decimals))
      }}
      onChange={(event: { target: { value: string } }) => {
        const nextDisplay = formatNumberWhileTyping(event.target.value, decimals)
        setDisplay(nextDisplay)
        if (!nextDisplay.trim()) {
          onChange('')
          return
        }
        const invariant = toInvariantNumber(nextDisplay, decimals)
        if (invariant !== null) onChange(invariant)
      }}
    />
  )
}
