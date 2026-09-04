import { ToggleSwitch } from 'primereact/toggleswitch'
import type { NotToggleProps } from 'react-querybuilder'

export function PrimeNotToggle({
  className,
  handleOnChange,
  label,
  checked,
  title,
  disabled,
}: NotToggleProps) {
  return (
    <label
      title={title}
      className={[
        'inline-flex items-center gap-2 rounded-lg bg-slate-100/80 px-2 py-1 text-xs font-medium text-slate-600',
        className,
      ]
        .filter(Boolean)
        .join(' ')}
    >
      <ToggleSwitch.Root
        checked={Boolean(checked)}
        disabled={disabled}
        onCheckedChange={(e: { checked: boolean }) => handleOnChange(e.checked)}
        className="relative inline-flex h-5 w-9 shrink-0"
        inputClassName="absolute inset-0 z-10 m-0 h-full w-full cursor-pointer appearance-none opacity-0"
        ariaLabel={typeof label === 'string' ? label : 'Not'}
      >
        <ToggleSwitch.Control className="pointer-events-none relative inline-flex h-5 w-9 items-center rounded-full bg-slate-200 transition-colors data-[checked]:bg-teal-700">
          <ToggleSwitch.Handle className="absolute left-0.5 top-0.5 h-4 w-4 rounded-full bg-white shadow transition-transform data-[checked]:translate-x-4" />
        </ToggleSwitch.Control>
      </ToggleSwitch.Root>
      <span>{label}</span>
    </label>
  )
}
