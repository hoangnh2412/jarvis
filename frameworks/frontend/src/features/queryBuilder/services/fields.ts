import type { PagedListQueryParams } from '../types'
import instance from './req'

/** GET /api/v1/company/employees */
export const callGetCompanyEmployees = async (params?: PagedListQueryParams) => {
  return await instance.get('v1/company/employees', { params })
}

/** GET /api/v1/company/employees/list-custom */
export const callGetCompanyEmployeesListCustom = async (
  params?: PagedListQueryParams & { currentEmployeeId?: string },
) => {
  return await instance.get('v1/company/employees/list-custom', { params })
}
