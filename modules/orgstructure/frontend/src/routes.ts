export type OrgStructureRouteSet = {
  orgStructure: string
  legalEntities: string
}

export const ORGSTRUCTURE_ROUTES: OrgStructureRouteSet = {
  orgStructure: '/system/org-structure',
  legalEntities: '/system/legal-entities',
}

/** Tương thích tên gọi mặc định trong đặc tả tích hợp. */
export const DEFAULT_ORGSTRUCTURE_ROUTES = ORGSTRUCTURE_ROUTES

export type OrgStructureMenuItem = {
  path: string
  label: string
  permission: string[]
}

export function orgStructureMenuItems(
  routes: OrgStructureRouteSet = ORGSTRUCTURE_ROUTES,
): OrgStructureMenuItem[] {
  return [
    { path: routes.orgStructure, label: 'Cơ cấu tổ chức', permission: ['System.Unit.View', 'System.Unit.Manage'] },
  ]
}
