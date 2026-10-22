export { UserDetailPage } from './pages/UserDetail'
export type { UserDetailPageProps } from './pages/UserDetail'
export { UserFormPage } from './pages/UserForm'
export type { UserFormPageProps } from './pages/UserForm'
export { UserListPage } from './pages/UserList'
export type {
  UserListPageContentContext,
  UserListPageProps,
} from './pages/UserList'

export {
  UserDetailView,
  UserForm,
  UserPageShell,
  UserRowActions,
  UserTable,
} from './components'
export type {
  UserDetailViewProps,
  UserFormProps,
  UserPageShellProps,
  UserRowActionsProps,
  UserTableProps,
} from './components'

export type {
  AdminUser,
  CreateUserPayload,
  GetUserListParams,
  UpdateUserPayload,
  UserListResult,
  UserRoleOption,
} from './types'

export {
  mockCreateUser,
  mockDeleteUser,
  mockGetUser,
  mockGetUserList,
  mockGetUserRoleOptions,
  mockUpdateUser,
  callCreateUser,
  callDeleteUser,
  callGetUser,
  callGetUserList,
  callUpdateUser,
  userHttp,
  configureUserHttp,
} from './services'

export {
  createUserFormSchema,
  editUserFormSchema,
  userFormDefaultValues,
} from './validation'
export type { UserFormData } from './validation'

export { useUserForm } from './hooks'
export type { UseUserFormOptions } from './hooks'

export {
  USER_ROUTES,
  configureUserNavigate,
  getUserCreatePath,
  getUserDetailPath,
  getUserEditPath,
  getUserListPath,
  getUserRouteList,
  navigateUser,
  userPaths,
} from './routes'

export { USER_PERMISSIONS, hasUserPermission } from './permission'
export type { UserPermissionKey } from './permission'

export { userMenuItems } from './menu'
export type { UserMenuItem } from './menu'

export { getUserMessages, userMessages } from './localization'
export type { UserLocale, UserMessages } from './localization'

export { resolveUserContent } from './utils'
export type { UserSlotContent } from './utils'
