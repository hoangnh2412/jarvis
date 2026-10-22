import { USER_ROUTES } from './paths'

export {
  USER_ROUTES,
  getUserCreatePath,
  getUserDetailPath,
  getUserEditPath,
  getUserListPath,
} from './paths'
export { configureUserNavigate, navigateUser } from './navigation'

export const userPaths = Object.values(USER_ROUTES)

export function getUserRouteList() {
  return userPaths
}
