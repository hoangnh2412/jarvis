import { useCallback, useState } from 'react'
import { Navigate } from 'react-router-dom'
import { LoginPage } from '@platform/core'
import { isAuthenticated, setAccessToken, setRefreshToken } from './accessToken'
import { LoginScopeFields } from './LoginScopeFields'
import { loginWithJwt } from './loginWithJwt'
import { setLoginScope } from './loginScope'

export function SampleLoginPage() {
  const [tenantId, setTenantId] = useState('')
  const [tenantCode, setTenantCode] = useState('')
  const [orgId, setOrgId] = useState('')
  const [orgCode, setOrgCode] = useState('')
  const [tenantError, setTenantError] = useState<string>()
  const [orgError, setOrgError] = useState<string>()

  const handleTenantChange = useCallback((nextTenantId: string, nextTenantCode: string) => {
    setTenantId(nextTenantId)
    setTenantCode(nextTenantCode)
    setOrgId('')
    setOrgCode('')
    setTenantError(undefined)
    setOrgError(undefined)
  }, [])

  const handleOrgChange = useCallback((nextOrgId: string, nextOrgCode: string) => {
    setOrgId(nextOrgId)
    setOrgCode(nextOrgCode)
    setOrgError(undefined)
  }, [])

  if (isAuthenticated()) {
    return <Navigate to="/" replace />
  }

  return (
    <LoginPage
      content={({ DefaultContent }) => (
        <>
          <LoginScopeFields
            tenantId={tenantId}
            orgId={orgId}
            tenantError={tenantError}
            orgError={orgError}
            onTenantChange={handleTenantChange}
            onOrgChange={handleOrgChange}
          />
          {DefaultContent}
        </>
      )}
      callback={{
        onSubmit: async (payload) => {
          const nextTenantError = tenantId && tenantCode ? undefined : 'Vui lòng chọn tenant'
          const nextOrgError = orgId && orgCode ? undefined : 'Vui lòng chọn tổ chức'
          setTenantError(nextTenantError)
          setOrgError(nextOrgError)
          if (nextTenantError) throw new Error(nextTenantError)
          if (nextOrgError) throw new Error(nextOrgError)

          setLoginScope({ tenantId, orgId })
          const result = await loginWithJwt({
            email: payload.email,
            password: payload.password,
            tenantId,
            orgId,
            tenantCode,
            orgCode,
          })
          const token = result.tokens?.accessToken
          const refreshToken = result.tokens?.refreshToken
          if (!token || !refreshToken) {
            throw new Error('Server không trả access token.')
          }
          setAccessToken(token)
          setRefreshToken(refreshToken)
          return result
        },
      }}
    />
  )
}
