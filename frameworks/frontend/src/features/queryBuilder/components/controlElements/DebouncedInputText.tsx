import { useEffect, useRef, useState } from 'react'
import { InputText } from 'primereact/inputtext'

const VALUE_DEBOUNCE_MS = 120

/** Local value + debounce commit — typing không rebuild cả query tree mỗi phím. */
export function DebouncedInputText({
  value,
  onCommit,
  debounceMs = VALUE_DEBOUNCE_MS,
  ...rest
}: {
  value: string
  onCommit: (value: string) => void
  debounceMs?: number
  type?: string
  title?: string
  disabled?: boolean
  placeholder?: string
  className?: string
}) {
  const [local, setLocal] = useState(value)
  const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null)
  const onCommitRef = useRef(onCommit)
  onCommitRef.current = onCommit

  useEffect(() => {
    setLocal(value)
  }, [value])

  useEffect(
    () => () => {
      if (timerRef.current) clearTimeout(timerRef.current)
    },
    [],
  )

  const flush = (next: string) => {
    if (timerRef.current) {
      clearTimeout(timerRef.current)
      timerRef.current = null
    }
    onCommitRef.current(next)
  }

  return (
    <InputText
      unstyled
      {...rest}
      value={local}
      onChange={(e: { target: { value: string } }) => {
        const next = e.target.value
        setLocal(next)
        if (timerRef.current) clearTimeout(timerRef.current)
        timerRef.current = setTimeout(() => {
          timerRef.current = null
          onCommitRef.current(next)
        }, debounceMs)
      }}
      onBlur={() => flush(local)}
    />
  )
}
