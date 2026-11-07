import { useEffect, useState } from 'react'
import { FieldSelect, type FieldSelectOption } from '@platform/core'
import {
  formatLoginScopeLabel,
  listLoginOrgs,
  listLoginTenants,
  type LoginScopeOption,
} from './loginCatalog'

type LoginScopeFieldsProps = {
  tenantId: string
  orgId: string
  tenantError?: string
  orgError?: string
  onTenantChange: (tenantId: string, tenantCode: string) => void
  onOrgChange: (orgId: string, orgCode: string) => void
}

function toOptions(items: LoginScopeOption[]): FieldSelectOption<string>[] {
  return items.map((item) => ({
    value: item.id,
    label: formatLoginScopeLabel(item),
  }))
}

export function LoginScopeFields({
  tenantId,
  orgId,
  tenantError,
  orgError,
  onTenantChange,
  onOrgChange,
}: LoginScopeFieldsProps) {
  const [tenantRows, setTenantRows] = useState<LoginScopeOption[]>([])
  const [orgState, setOrgState] = useState<{
    tenantId: string
    rows: LoginScopeOption[]
  }>({ tenantId: '', rows: [] })
  const tenants = toOptions(tenantRows)
  const orgRows = orgState.tenantId === tenantId ? orgState.rows : []
  const [tenantStatus, setTenantStatus] = useState('Đang tải tenant…')
  const [orgStatus, setOrgStatus] = useState('Chọn tenant trước')
  const orgs = toOptions(orgRows)

  useEffect(() => {
    let cancelled = false
    listLoginTenants()
      .then((items) => {
        if (cancelled) return
        setTenantRows(items)
        setTenantStatus(items.length === 0 ? 'Không có tenant' : 'Chọn tenant')
      })
      .catch(() => {
        if (cancelled) return
        setTenantRows([])
        setTenantStatus('Không tải được tenant')
      })
    return () => {
      cancelled = true
    }
  }, [])

  useEffect(() => {
    if (tenantId || tenantRows.length !== 1) return
    onTenantChange(tenantRows[0].id, tenantRows[0].code)
  }, [onTenantChange, tenantId, tenantRows])

  useEffect(() => {
    if (!tenantId) {
      setOrgStatus('Chọn tenant trước')
      return
    }

    let cancelled = false
    setOrgStatus('Đang tải tổ chức…')
    listLoginOrgs(tenantId)
      .then((items) => {
        if (cancelled) return
        setOrgState({ tenantId, rows: items })
        setOrgStatus(items.length === 0 ? 'Không có tổ chức' : 'Chọn tổ chức')
      })
      .catch(() => {
        if (cancelled) return
        setOrgState({ tenantId, rows: [] })
        setOrgStatus('Không tải được tổ chức')
      })
    return () => {
      cancelled = true
    }
  }, [tenantId])

  useEffect(() => {
    if (!tenantId) return
    if (orgId && orgs.some((option) => option.value === orgId)) return
    if (orgRows.length === 1) onOrgChange(orgRows[0].id, orgRows[0].code)
    else if (orgId) onOrgChange('', '')
  }, [onOrgChange, orgId, orgRows, orgs, tenantId])

  return (
    <>
      <div>
        <label htmlFor="login-tenant" className="mb-1.5 block text-sm font-medium text-slate-700">
          Tenant <span className="text-red-500">*</span>
        </label>
        <FieldSelect
          id="login-tenant"
          value={tenantId || null}
          options={tenants}
          onChange={(value) => {
            const row = tenantRows.find((item) => item.id === value)
            onTenantChange(value ?? '', row?.code ?? '')
          }}
          placeholder={tenantStatus}
          disabled={tenants.length === 0}
          invalid={Boolean(tenantError)}
        />
        {tenantError && <p className="mt-1.5 text-sm text-red-600">{tenantError}</p>}
      </div>

      <div>
        <label htmlFor="login-org" className="mb-1.5 block text-sm font-medium text-slate-700">
          Tổ chức <span className="text-red-500">*</span>
        </label>
        <FieldSelect
          id="login-org"
          value={orgId || null}
          options={orgs}
          onChange={(value) => {
            const row = orgRows.find((item) => item.id === value)
            onOrgChange(value ?? '', row?.code ?? '')
          }}
          placeholder={orgStatus}
          disabled={!tenantId || orgs.length === 0}
          invalid={Boolean(orgError)}
        />
        {orgError && <p className="mt-1.5 text-sm text-red-600">{orgError}</p>}
      </div>
    </>
  )
}
