import { useEffect, useRef, useState } from 'react'
import { ChevronDown } from 'lucide-react'
import { Select } from '@platform/core'
import {
  formatWorkingScopeLabel,
  listLoginOrgs,
  listLoginTenants,
  type LoginScopeOption,
} from './loginCatalog'
import { getLoginOrgId, getLoginTenantId, setLoginScope } from './loginScope'

type WorkingScopeSelectsProps = {
  onChange: () => void
}

const triggerClass =
  'flex h-9 w-full min-w-0 items-center justify-between gap-2 overflow-hidden rounded-lg border border-slate-200 bg-white px-3 text-left text-sm text-slate-800 shadow-sm outline-none hover:border-slate-300 hover:bg-slate-50'

function ScopeSelect({
  label,
  value,
  options,
  disabled,
  onChange,
}: {
  label: string
  value: string
  options: LoginScopeOption[]
  disabled?: boolean
  onChange: (value: string) => void
}) {
  const [open, setOpen] = useState(false)
  const list = options.map((item) => ({
    label: formatWorkingScopeLabel(item),
    value: item.id,
  }))

  return (
    <div className="kit-admin-header-scope">
    <Select.Root
      value={value || undefined}
      open={open}
      options={list}
      optionLabel="label"
      optionValue="value"
      disabled={disabled || list.length === 0}
      onOpenChange={(event: { value: boolean }) => setOpen(event.value)}
      onValueChange={(event: { value?: unknown }) => {
        if (typeof event.value === 'string' && event.value) onChange(event.value)
      }}
    >
      <Select.Trigger type="button" aria-label={label} className={triggerClass}>
        <Select.Value placeholder={label} className="min-w-0 flex-1 truncate text-left" />
        <Select.Indicator className="inline-flex shrink-0 text-slate-500">
          <ChevronDown className="size-4" aria-hidden />
        </Select.Indicator>
      </Select.Trigger>
      <Select.Portal>
        <Select.Positioner className="z-[120]">
          <Select.Popup className="min-w-[var(--px-positioner-anchor-width)] overflow-hidden rounded-xl border border-slate-200 bg-white py-1 shadow-lg">
            <Select.List className="m-0 max-h-72 list-none overflow-auto p-0 outline-none">
              {list.map((option, index) => (
                <Select.Option
                  key={option.value}
                  index={index}
                  className="cursor-pointer px-3 py-2 text-sm text-slate-800 outline-none data-[focused]:bg-slate-50 data-[selected]:bg-teal-50 data-[selected]:font-medium data-[selected]:text-teal-800"
                >
                  {option.label}
                </Select.Option>
              ))}
            </Select.List>
          </Select.Popup>
        </Select.Positioner>
      </Select.Portal>
    </Select.Root>
    </div>
  )
}

export function WorkingScopeSelects({ onChange }: WorkingScopeSelectsProps) {
  const onChangeRef = useRef(onChange)
  onChangeRef.current = onChange
  const [tenants, setTenants] = useState<LoginScopeOption[]>([])
  const [orgs, setOrgs] = useState<LoginScopeOption[]>([])
  const [tenantId, setTenantId] = useState(() => getLoginTenantId() ?? '')
  const [orgId, setOrgId] = useState(() => getLoginOrgId() ?? '')
  const [catalogReady, setCatalogReady] = useState(false)

  useEffect(() => {
    let cancelled = false
    void listLoginTenants()
      .then((rows) => {
        if (cancelled) return
        setTenants(rows)
        const stored = getLoginTenantId()
        const next = rows.some((row) => row.id === stored) ? (stored ?? '') : (rows[0]?.id ?? '')
        setTenantId(next)
      })
      .finally(() => {
        if (!cancelled) setCatalogReady(true)
      })
    return () => {
      cancelled = true
    }
  }, [])

  useEffect(() => {
    if (!catalogReady) return
    if (!tenantId) {
      setOrgs([])
      setOrgId('')
      return
    }
    let cancelled = false
    void listLoginOrgs(tenantId).then((rows) => {
      if (cancelled) return
      setOrgs(rows)
      const stored = getLoginOrgId()
      const next = rows.some((row) => row.id === stored) ? (stored ?? '') : (rows[0]?.id ?? '')
      setOrgId(next)
      const prevTenant = getLoginTenantId()
      const prevOrg = getLoginOrgId()
      if (prevTenant === tenantId && prevOrg === next) return
      setLoginScope({ tenantId, orgId: next })
      onChangeRef.current()
    })
    return () => {
      cancelled = true
    }
  }, [catalogReady, tenantId])

  const handleTenant = (nextTenantId: string) => {
    if (nextTenantId === tenantId) return
    setTenantId(nextTenantId)
  }

  const handleOrg = (nextOrgId: string) => {
    if (!tenantId || nextOrgId === orgId) return
    setOrgId(nextOrgId)
    setLoginScope({ tenantId, orgId: nextOrgId })
    onChangeRef.current()
  }

  return (
    <>
      <ScopeSelect
        label="Tenant"
        value={tenantId}
        options={tenants}
        disabled={!catalogReady}
        onChange={handleTenant}
      />
      <ScopeSelect
        label="Tổ chức"
        value={orgId}
        options={orgs}
        disabled={!catalogReady || !tenantId}
        onChange={handleOrg}
      />
    </>
  )
}
