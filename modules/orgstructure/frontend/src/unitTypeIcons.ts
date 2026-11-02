import type { LucideIcon } from 'lucide-react'
import {
  Briefcase,
  Building,
  Building2,
  Circle,
  Factory,
  GitBranch,
  Landmark,
  Layers3,
  LayoutGrid,
  MapPinned,
  Network,
  Store,
  Users,
  Warehouse,
} from 'lucide-react'

export type UnitTypeIconKey =
  | 'building-2'
  | 'building'
  | 'landmark'
  | 'map-pinned'
  | 'git-branch'
  | 'layers-3'
  | 'briefcase'
  | 'layout-grid'
  | 'users'
  | 'circle'
  | 'network'
  | 'factory'
  | 'store'
  | 'warehouse'

export const UNIT_TYPE_ICON_OPTIONS: ReadonlyArray<{ key: UnitTypeIconKey; label: string; Icon: LucideIcon }> = [
  { key: 'building-2', label: 'Tập đoàn / tòa nhà', Icon: Building2 },
  { key: 'building', label: 'Công ty', Icon: Building },
  { key: 'landmark', label: 'Pháp nhân', Icon: Landmark },
  { key: 'map-pinned', label: 'Vùng / địa bàn', Icon: MapPinned },
  { key: 'git-branch', label: 'Chi nhánh', Icon: GitBranch },
  { key: 'layers-3', label: 'Khối', Icon: Layers3 },
  { key: 'briefcase', label: 'Phòng ban', Icon: Briefcase },
  { key: 'layout-grid', label: 'Bộ phận', Icon: LayoutGrid },
  { key: 'users', label: 'Tổ / nhóm', Icon: Users },
  { key: 'network', label: 'Mạng lưới', Icon: Network },
  { key: 'factory', label: 'Nhà máy', Icon: Factory },
  { key: 'store', label: 'Cửa hàng', Icon: Store },
  { key: 'warehouse', label: 'Kho', Icon: Warehouse },
  { key: 'circle', label: 'Mặc định', Icon: Circle },
]

/** Tên icon rút gọn mà host/catalog có thể lưu thay cho khóa đầy đủ. */
const ICON_KEY_ALIASES: Record<string, UnitTypeIconKey> = {
  layers: 'layers-3',
  layer: 'layers-3',
  building2: 'building-2',
  branch: 'git-branch',
  grid: 'layout-grid',
  group: 'users',
  team: 'users',
  default: 'circle',
}

const ICON_BY_KEY = new Map(UNIT_TYPE_ICON_OPTIONS.map((item) => [item.key, item.Icon]))

export function resolveUnitTypeIcon(iconKey: string | undefined | null): LucideIcon {
  const raw = (iconKey ?? '').trim().toLowerCase()
  const key = (ICON_KEY_ALIASES[raw] ?? raw) as UnitTypeIconKey
  return ICON_BY_KEY.get(key) ?? Circle
}
