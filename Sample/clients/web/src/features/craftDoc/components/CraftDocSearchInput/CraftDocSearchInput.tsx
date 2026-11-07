import type { ChangeEvent } from 'react'
import { Search } from 'lucide-react'
import { IconField } from 'primereact/iconfield'
import { InputText } from 'primereact/inputtext'
import { fieldSearchInputClass } from '../fieldStyles'

export type CraftDocSearchInputProps = {
  value: string
  onChange: (value: string) => void
  placeholder?: string
  className?: string
}

export function CraftDocSearchInput({
  value,
  onChange,
  placeholder = 'Search...',
  className = '',
}: CraftDocSearchInputProps) {
  return (
    <IconField.Root className={['relative block w-full', className].filter(Boolean).join(' ')}>
      <IconField.Inset className="pointer-events-none absolute left-3 top-1/2 z-[1] -translate-y-1/2 text-slate-400">
        <Search className="size-4" aria-hidden />
      </IconField.Inset>
      <InputText
        value={value}
        unstyled
        placeholder={placeholder}
        className={fieldSearchInputClass}
        onChange={(event: ChangeEvent<HTMLInputElement>) => onChange(event.target.value)}
      />
    </IconField.Root>
  )
}
