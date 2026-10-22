import { POLICY_CATALOG, type PolicyDefinition } from '../../../authorization/catalog'

export type PolicyPickerProps = {
  policies?: PolicyDefinition[]
  value: string[]
  onChange: (policies: string[]) => void
  disabled?: boolean
}

export function PolicyPicker({
  policies = POLICY_CATALOG,
  value,
  onChange,
  disabled = false,
}: PolicyPickerProps) {
  const selected = new Set(value)

  const toggle = (code: string, checked: boolean) => {
    const next = new Set(selected)
    if (checked) next.add(code)
    else next.delete(code)
    onChange([...next])
  }

  return (
    <ul className="m-0 flex list-none flex-col gap-2 p-0">
      {policies.map((policy) => (
        <li key={policy.code} className="rounded-xl border border-slate-200 p-3">
          <label className="flex items-start gap-3">
            <input
              type="checkbox"
              className="mt-1 size-4 accent-teal-700"
              checked={selected.has(policy.code)}
              disabled={disabled}
              onChange={(event) => toggle(policy.code, event.target.checked)}
            />
            <span>
              <span className="block text-sm font-medium text-slate-800">
                {policy.name}
              </span>
              <code className="text-xs text-slate-500">{policy.code}</code>
              <span className="mt-1 block text-xs text-slate-500">{policy.summary}</span>
            </span>
          </label>
        </li>
      ))}
    </ul>
  )
}
