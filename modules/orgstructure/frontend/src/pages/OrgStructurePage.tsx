import { useMemo, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ConfirmDialog, FeatureDialog, PageHeader, Tag, notify } from '@platform/core'
import {
  ChevronDown,
  ChevronRight,
  ChevronsDown,
  ChevronsUp,
  Landmark,
  Layers3,
  Pencil,
  Plus,
  Search,
} from 'lucide-react'
import { btnAuxClass, btnPrimaryClass, btnSecondaryClass, fieldInputClass } from '../components/fieldStyles'
import { notifyApiError } from '../notify'
import { resolveUnitTypeIcon } from '../unitTypeIcons'
import {
  ORG_STRUCTURE_TEMPLATES,
  applyOrgTemplate,
  createOrgUnit,
  listLegalEntities,
  listOrgUnits,
  listUnitMemberSummary,
  listUnitTypes,
  moveOrgUnit,
  orgStructureQueryKeys,
  previewOrgTemplate,
  setOrgUnitActive,
  updateOrgUnit,
  type LegalEntity,
  type OrgStructureTemplateCode,
  type OrgUnit,
  type OrgUnitTypeCatalogItem,
  type TemplateNode,
} from '../services/orgStructure'
import { LegalEntitiesManagerDialog } from './LegalEntitiesManagerDialog'
import { LegalEntityProfileDialog } from './LegalEntityProfileDialog'
import { UnitMembersManagerDialog, type OrgStructureAssignableUser } from './UnitMembersManagerDialog'
import { UnitTypesManagerDialog } from './UnitTypesManagerDialog'

type UnitDraft = {
  code: string
  name: string
  unitType: string
  parentId: string
  legalEntityId: string
  sortOrder: number
}

type EditorState = {
  mode: 'create' | 'edit'
  unit: OrgUnit | null
  draft: UnitDraft
}

const TREE_INDENT_PX = 22

/** Khi catalog API chưa có tên (hoặc chưa migrate DB). */
const UNIT_TYPE_NAME_FALLBACK: Record<string, string> = {
  Group: 'Tập đoàn',
  Company: 'Công ty',
  LegalEntityNode: 'Pháp nhân',
  Region: 'Vùng',
  Branch: 'Chi nhánh',
  Division: 'Khối',
  Department: 'Phòng ban',
  Section: 'Bộ phận',
  TeamNode: 'Tổ / nhóm',
}

function unitTypeBadgeClass(code: string): string {
  if (code === 'Group') return 'border-teal-600/30 bg-teal-700/10 text-teal-700'
  if (code === 'Company' || code === 'LegalEntityNode') return 'border-slate-200 bg-slate-50 text-slate-800'
  if (code === 'Region' || code === 'Branch') return 'border-slate-200 bg-slate-50 text-slate-500'
  if (code === 'Division') return 'border-teal-600/20 bg-slate-50 text-slate-800'
  return 'border-slate-200 bg-white text-slate-500'
}

const treeMetaChip =
  'shrink-0 rounded border px-1.5 py-0.5 text-[11px] font-medium leading-none'
const treeMemberChip = `${treeMetaChip} border-teal-600/30 bg-teal-700/5 tabular-nums text-teal-700 hover:bg-teal-700/10`
const treeRowMiniBtn =
  'inline-flex size-6 shrink-0 items-center justify-center rounded border border-slate-200 bg-slate-50 text-slate-500 hover:bg-slate-100 disabled:cursor-not-allowed disabled:opacity-40'
const treeRowMiniBtnSuccess =
  'inline-flex size-6 shrink-0 items-center justify-center rounded border border-emerald-600/50 bg-slate-50 text-emerald-600 hover:bg-emerald-50 disabled:cursor-not-allowed disabled:opacity-40'

function UnitTypeGlyph({
  unitType,
  catalog,
  className,
}: {
  unitType: string
  catalog: Map<string, OrgUnitTypeCatalogItem>
  className?: string
}) {
  const Icon = resolveUnitTypeIcon(catalog.get(unitType)?.icon)
  return <Icon className={className} aria-hidden />
}

function compareSiblingUnits(left: OrgUnit, right: OrgUnit): number {
  if (left.sortOrder !== right.sortOrder) return left.sortOrder - right.sortOrder
  return left.name.localeCompare(right.name, 'vi')
}

function nextSiblingSortOrder(units: OrgUnit[], parentId: string): number {
  const siblings = units.filter((unit) => (unit.parentId ?? '') === parentId)
  if (siblings.length === 0) return 10
  return Math.max(...siblings.map((unit) => unit.sortOrder)) + 10
}

function emptyDraft(parentId = '', unitType = 'Department', sortOrder = 10): UnitDraft {
  return { code: '', name: '', unitType, parentId, legalEntityId: '', sortOrder }
}

function descendantIds(rootId: string, byParent: Map<string, OrgUnit[]>): Set<string> {
  const ids = new Set<string>()
  const walk = (id: string) => {
    for (const child of byParent.get(id) ?? []) {
      ids.add(child.id)
      walk(child.id)
    }
  }
  walk(rootId)
  return ids
}

export type OrgStructurePageProps = {
  /** Host quyết định quyền quản lý (mặc định cho phép; host bọc kiểm quyền bên ngoài nếu cần). */
  canManage?: boolean
  /** Host nạp danh sách người dùng để dialog thành viên gán user vào đơn vị. */
  loadUsers?: () => Promise<OrgStructureAssignableUser[]>
}

export function OrgStructurePage({ canManage = true, loadUsers }: OrgStructurePageProps) {
  const queryClient = useQueryClient()
  const unitsQuery = useQuery({ queryKey: orgStructureQueryKeys.units, queryFn: listOrgUnits })
  const typesQuery = useQuery({ queryKey: orgStructureQueryKeys.unitTypes, queryFn: listUnitTypes })
  const legalQuery = useQuery({ queryKey: orgStructureQueryKeys.legalEntities, queryFn: listLegalEntities })
  const memberSummaryQuery = useQuery({
    queryKey: orgStructureQueryKeys.unitMemberSummary,
    queryFn: listUnitMemberSummary,
  })

  const [search, setSearch] = useState('')
  const [collapsed, setCollapsed] = useState<Set<string>>(new Set())
  const [editor, setEditor] = useState<EditorState | null>(null)
  const [pendingActive, setPendingActive] = useState<OrgUnit | null>(null)
  const [templateOpen, setTemplateOpen] = useState(false)
  const [unitTypesOpen, setUnitTypesOpen] = useState(false)
  const [legalEntitiesOpen, setLegalEntitiesOpen] = useState(false)
  const [viewingLegal, setViewingLegal] = useState<LegalEntity | null>(null)
  const [membersUnit, setMembersUnit] = useState<OrgUnit | null>(null)

  const units = unitsQuery.data ?? []
  const unitTypeCatalog = typesQuery.data ?? []
  const unitTypeByCode = useMemo(
    () => new Map(unitTypeCatalog.map((item) => [item.code, item])),
    [unitTypeCatalog],
  )
  const unitTypeLabel = (code: string) => {
    const fromCatalog = unitTypeByCode.get(code)?.name?.trim()
    if (fromCatalog && fromCatalog !== code) return fromCatalog
    return UNIT_TYPE_NAME_FALLBACK[code] ?? fromCatalog ?? code
  }
  const legalById = useMemo(() => {
    const map = new Map<string, LegalEntity>()
    for (const item of legalQuery.data ?? []) map.set(item.id, item)
    return map
  }, [legalQuery.data])
  const memberCountByUnit = useMemo(() => {
    const map = new Map<string, { active: number; total: number }>()
    for (const row of memberSummaryQuery.data ?? []) {
      const unitId = row.organizationalUnitId.trim().toLowerCase()
      if (!unitId) continue
      map.set(unitId, { active: row.activeCount, total: row.totalCount })
    }
    return map
  }, [memberSummaryQuery.data])
  const byParent = useMemo(() => {
    const map = new Map<string, OrgUnit[]>()
    for (const unit of units) {
      const key = unit.parentId ?? ''
      const list = map.get(key) ?? []
      list.push(unit)
      map.set(key, list)
    }
    for (const list of map.values()) {
      list.sort(compareSiblingUnits)
    }
    return map
  }, [units])

  /** Thứ tự anh em từ API (sortOrder), dùng cho dropdown và đối chiếu form. */
  const sortSiblingUnits = (items: OrgUnit[]) => [...items].sort(compareSiblingUnits)

  const branchUnitIds = useMemo(() => {
    const ids: string[] = []
    for (const unit of units) {
      if ((byParent.get(unit.id)?.length ?? 0) > 0) ids.push(unit.id)
    }
    return ids
  }, [byParent, units])

  const visibleIds = useMemo(() => {
    const keyword = search.trim().toLowerCase()
    if (!keyword) return null
    const matched = new Set<string>()
    const byId = new Map(units.map((unit) => [unit.id, unit]))
    for (const unit of units) {
      const haystack = `${unit.name} ${unit.code} ${unitTypeLabel(unit.unitType)}`.toLowerCase()
      if (!haystack.includes(keyword)) continue
      let cursor: OrgUnit | undefined = unit
      while (cursor) {
        matched.add(cursor.id)
        cursor = cursor.parentId ? byId.get(cursor.parentId) : undefined
      }
    }
    return matched
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [search, units, unitTypeByCode])

  const refreshUnits = async () => {
    await queryClient.invalidateQueries({ queryKey: orgStructureQueryKeys.units })
  }

  const saveMutation = useMutation({
    mutationFn: async (state: EditorState) => {
      const input = {
        code: state.draft.code,
        name: state.draft.name,
        unitType: state.draft.unitType,
        parentId: state.draft.parentId || null,
        legalEntityId: state.draft.legalEntityId || null,
        sortOrder: state.draft.sortOrder,
      }
      if (state.mode === 'create') {
        return createOrgUnit(input)
      }
      if (!state.unit) return null
      const updated = await updateOrgUnit(state.unit.id, {
        code: input.code,
        name: input.name,
        unitType: input.unitType,
        legalEntityId: input.legalEntityId,
        sortOrder: input.sortOrder,
      })
      const nextParent = state.draft.parentId || null
      if ((state.unit.parentId ?? null) !== nextParent) {
        return moveOrgUnit(state.unit.id, nextParent)
      }
      return updated
    },
    onError: notifyApiError,
    onSuccess: async () => {
      setEditor(null)
      await refreshUnits()
      notify.success('Đã lưu đơn vị trên cây.')
    },
  })

  const activeMutation = useMutation({
    mutationFn: (unit: OrgUnit) => setOrgUnitActive(unit.id, !unit.isActive),
    onError: notifyApiError,
    onSuccess: async (_data, unit) => {
      setPendingActive(null)
      await refreshUnits()
      notify.success(unit.isActive ? 'Đã ngừng dùng đơn vị.' : 'Đã bật lại đơn vị.')
    },
  })

  const openCreate = (parentId = '') => {
    setEditor({
      mode: 'create',
      unit: null,
      draft: emptyDraft(parentId, 'Department', nextSiblingSortOrder(units, parentId)),
    })
  }

  const openEdit = (unit: OrgUnit) => {
    setEditor({
      mode: 'edit',
      unit,
      draft: {
        code: unit.code,
        name: unit.name,
        unitType: unit.unitType,
        parentId: unit.parentId ?? '',
        legalEntityId: unit.legalEntityId ?? '',
        sortOrder: unit.sortOrder,
      },
    })
  }

  const roots = byParent.get('') ?? []
  const shownRoots = roots.filter((unit) => !visibleIds || visibleIds.has(unit.id))

  return (
    <section className="flex h-full min-h-0 flex-col gap-4 overflow-hidden p-4 text-sm text-slate-800 sm:p-6">
      <PageHeader
        title="Cơ cấu tổ chức"
        description="Cây đơn vị của tổ chức đang chọn, loại đơn vị, pháp nhân và thành viên từng đơn vị."
        actions={(
          <>
            <label className="relative min-w-56 flex-1 sm:max-w-sm">
              <Search className="pointer-events-none absolute left-2.5 top-1/2 size-4 -translate-y-1/2 text-slate-400" aria-hidden />
              <input
                aria-label="Tìm trên cây"
                value={search}
                onChange={(event) => setSearch(event.target.value)}
                placeholder="Tìm tên hoặc mã…"
                className="h-10 w-full rounded-xl border border-slate-200 bg-white pl-9 pr-3 text-sm outline-none focus:border-teal-600"
              />
            </label>
            {units.length > 0 ? (
              <div className="flex shrink-0 items-center gap-1">
                <button
                  type="button"
                  className={`${btnSecondaryClass} size-10 justify-center px-0`}
                  title="Mở tất cả nhánh"
                  aria-label="Mở tất cả nhánh"
                  onClick={() => setCollapsed(new Set())}
                >
                  <ChevronsDown className="size-4" aria-hidden />
                </button>
                <button
                  type="button"
                  className={`${btnSecondaryClass} size-10 justify-center px-0`}
                  title="Thu tất cả nhánh"
                  aria-label="Thu tất cả nhánh"
                  onClick={() => setCollapsed(new Set(branchUnitIds))}
                >
                  <ChevronsUp className="size-4" aria-hidden />
                </button>
              </div>
            ) : null}
            <button type="button" className={`${btnAuxClass} h-10 whitespace-nowrap`} onClick={() => setUnitTypesOpen(true)}>
              <Layers3 className="size-4 shrink-0" aria-hidden />
              Quản lý loại đơn vị
            </button>
            <button type="button" className={`${btnAuxClass} h-10 whitespace-nowrap`} onClick={() => setLegalEntitiesOpen(true)}>
              <Landmark className="size-4 shrink-0" aria-hidden />
              Quản lý pháp nhân
            </button>
            {canManage && units.length === 0 && !unitsQuery.isLoading && !unitsQuery.isError ? (
              <>
                <button type="button" className={`${btnAuxClass} h-10 whitespace-nowrap`} onClick={() => setTemplateOpen(true)}>
                  Tạo từ mẫu
                </button>
                <button type="button" className={`${btnPrimaryClass} whitespace-nowrap`} onClick={() => openCreate()}>
                  <Plus className="size-4" aria-hidden />
                  Thêm đơn vị gốc
                </button>
              </>
            ) : null}
          </>
        )}
      />

      <div className="min-h-0 flex-1 overflow-auto rounded-xl border border-slate-200 bg-white p-3 shadow-sm md:p-4">
        {unitsQuery.isLoading ? (
          <p className="px-3 py-6 text-sm text-slate-500">Đang tải cây cơ cấu…</p>
        ) : unitsQuery.isError ? (
          <p className="px-3 py-6 text-sm text-red-600">Không tải được cơ cấu. Kiểm tra tổ chức đang chọn trên thanh trên.</p>
        ) : roots.length === 0 ? (
          <div className="mx-auto mt-16 max-w-md text-center">
            <h2 className="m-0 text-base font-semibold">Chưa có đơn vị nào trên cây</h2>
            <p className="mt-2 text-sm text-slate-500">
              Thêm đơn vị gốc, rồi thêm phòng ban hoặc chi nhánh bên dưới. Hóa đơn vẫn thuộc tổ chức đang chọn, không thuộc từng nút trên cây.
            </p>
            {canManage ? (
              <button type="button" className={`${btnPrimaryClass} mt-4`} onClick={() => openCreate()}>
                Thêm đơn vị gốc
              </button>
            ) : null}
          </div>
        ) : (
          <ul className="m-0 list-none p-0" aria-label="Cây cơ cấu tổ chức">
            {shownRoots.length === 0 ? (
              <li className="px-3 py-6 text-sm text-slate-500">Không có đơn vị khớp từ khóa.</li>
            ) : shownRoots.map((unit, rootIndex) => (
              <TreeRow
                key={unit.id}
                unit={unit}
                depth={0}
                isLast={rootIndex === shownRoots.length - 1}
                byParent={byParent}
                visibleIds={visibleIds}
                collapsed={collapsed}
                legalById={legalById}
                unitTypeByCode={unitTypeByCode}
                unitTypeLabel={unitTypeLabel}
                canManage={canManage}
                onToggle={(id) => {
                  setCollapsed((current) => {
                    const next = new Set(current)
                    if (next.has(id)) next.delete(id)
                    else next.add(id)
                    return next
                  })
                }}
                onCreateChild={(parent) => openCreate(parent.id)}
                onEdit={openEdit}
                onToggleActive={setPendingActive}
                onViewLegal={setViewingLegal}
                onManageMembers={setMembersUnit}
                memberCountByUnit={memberCountByUnit}
              />
            ))}
          </ul>
        )}
      </div>

      {editor ? (
        <UnitEditor
          state={editor}
          units={units}
          sortSiblingUnits={sortSiblingUnits}
          byParent={byParent}
          unitTypeCatalog={unitTypeCatalog}
          legalEntities={legalQuery.data ?? []}
          canManage={canManage}
          isSubmitting={saveMutation.isPending}
          onChange={(draft) => setEditor({ ...editor, draft })}
          onClose={() => {
            if (!saveMutation.isPending) setEditor(null)
          }}
          onSubmit={() => saveMutation.mutate(editor)}
        />
      ) : null}

      <ConfirmDialog
        open={Boolean(pendingActive)}
        title={
          pendingActive?.isActive
            ? 'Chuyển trạng thái thành ngừng dùng?'
            : 'Chuyển trạng thái thành đang sử dụng?'
        }
        description={pendingActive
          ? `Đơn vị ${pendingActive.name} sẽ chuyển sang trạng thái ${pendingActive.isActive ? 'ngừng dùng' : 'đang sử dụng'}. Đơn vị vẫn giữ trên cây; đơn vị con không bị xóa.`
          : undefined}
        confirmText={pendingActive?.isActive ? 'Chuyển sang ngừng dùng' : 'Chuyển sang đang sử dụng'}
        loading={activeMutation.isPending}
        onClose={() => setPendingActive(null)}
        onConfirm={() => pendingActive && activeMutation.mutate(pendingActive)}
      />

      {templateOpen ? (
        <TemplateDialog
          hasUnits={units.length > 0}
          unitTypeLabel={unitTypeLabel}
          onClose={() => setTemplateOpen(false)}
          onApplied={async () => {
            setTemplateOpen(false)
            await refreshUnits()
          }}
        />
      ) : null}

      {unitTypesOpen ? (
        <UnitTypesManagerDialog canManage={canManage} onClose={() => setUnitTypesOpen(false)} />
      ) : null}

      {legalEntitiesOpen ? (
        <LegalEntitiesManagerDialog canManage={canManage} onClose={() => setLegalEntitiesOpen(false)} />
      ) : null}

      {viewingLegal ? (
        <LegalEntityProfileDialog entity={viewingLegal} onClose={() => setViewingLegal(null)} />
      ) : null}

      {membersUnit ? (
        <UnitMembersManagerDialog
          unit={membersUnit}
          canManage={canManage}
          loadUsers={loadUsers}
          onClose={() => setMembersUnit(null)}
        />
      ) : null}

    </section>
  )
}

function TreeRow({
  unit,
  depth,
  isLast,
  byParent,
  visibleIds,
  collapsed,
  legalById,
  unitTypeByCode,
  unitTypeLabel,
  canManage,
  onToggle,
  onCreateChild,
  onEdit,
  onToggleActive,
  onViewLegal,
  onManageMembers,
  memberCountByUnit,
}: {
  unit: OrgUnit
  depth: number
  isLast: boolean
  byParent: Map<string, OrgUnit[]>
  visibleIds: Set<string> | null
  collapsed: Set<string>
  legalById: Map<string, LegalEntity>
  unitTypeByCode: Map<string, OrgUnitTypeCatalogItem>
  unitTypeLabel: (code: string) => string
  canManage: boolean
  onToggle: (id: string) => void
  onCreateChild: (unit: OrgUnit) => void
  onEdit: (unit: OrgUnit) => void
  onToggleActive: (unit: OrgUnit) => void
  onViewLegal: (legal: LegalEntity) => void
  onManageMembers: (unit: OrgUnit) => void
  memberCountByUnit: Map<string, { active: number; total: number }>
}) {
  if (visibleIds && !visibleIds.has(unit.id)) return null
  const children = (byParent.get(unit.id) ?? []).filter((child) => !visibleIds || visibleIds.has(child.id))
  const expanded = !collapsed.has(unit.id) || Boolean(visibleIds)
  const legal = unit.legalEntityId ? legalById.get(unit.legalEntityId) : undefined
  const isRoot = depth === 0
  const hasBranch = children.length > 0
  const rowTitle = [unit.path, unit.code].filter(Boolean).join(' · ')
  const memberStats = memberCountByUnit.get(unit.id.trim().toLowerCase())
  const memberTotal = memberStats?.total ?? 0
  const memberActive = memberStats?.active ?? 0
  const memberCountLabel =
    memberTotal > 0 && memberActive < memberTotal
      ? `${memberTotal} nhân sự (${memberActive} đang gắn)`
      : `${memberTotal} nhân sự`

  return (
    <li className="relative m-0 list-none">
      {depth > 0 ? (
        <>
          {!isLast ? (
            <span
              className="pointer-events-none absolute top-0 border-l border-slate-200/50"
              style={{
                left: (depth - 1) * TREE_INDENT_PX + 10,
                bottom: 0,
                width: 0,
              }}
              aria-hidden
            />
          ) : null}
          <span
            className="pointer-events-none absolute border-b border-l border-slate-200/50"
            style={{
              left: (depth - 1) * TREE_INDENT_PX + 10,
              top: 0,
              width: 12,
              height: 18,
              borderBottomLeftRadius: 4,
            }}
            aria-hidden
          />
        </>
      ) : null}
      <div
        className={[
          'group relative flex min-h-9 items-center gap-1.5 rounded-md py-1 pr-1 transition-colors hover:bg-slate-50',
          !unit.isActive ? 'opacity-70' : '',
        ].join(' ')}
        style={{ paddingLeft: depth * TREE_INDENT_PX }}
      >
        <button
          type="button"
          className="inline-flex size-6 shrink-0 items-center justify-center rounded text-slate-500 hover:bg-slate-100 disabled:pointer-events-none disabled:opacity-25"
          aria-label={expanded ? `Thu gọn ${unit.name}` : `Mở rộng ${unit.name}`}
          aria-expanded={hasBranch ? expanded : undefined}
          disabled={!hasBranch}
          onClick={() => onToggle(unit.id)}
        >
          {hasBranch ? (
            expanded ? <ChevronDown className="size-4" /> : <ChevronRight className="size-4" />
          ) : (
            <span className="size-1.5 rounded-full bg-slate-200" aria-hidden />
          )}
        </button>
        <UnitTypeGlyph
          unitType={unit.unitType}
          catalog={unitTypeByCode}
          className={`size-4 shrink-0 ${isRoot ? 'text-teal-700' : 'text-slate-500'}`}
        />
        <div className="flex min-w-0 flex-1 items-center gap-x-2 gap-y-1">
          <div className="flex min-w-0 flex-wrap items-center gap-x-2 gap-y-1">
            <span className={`${treeMetaChip} ${unitTypeBadgeClass(unit.unitType)}`}>
              {unitTypeLabel(unit.unitType)}
            </span>
            <span
              className={[
                'min-w-0 truncate',
                isRoot ? 'text-sm font-semibold' : 'text-sm font-medium',
                !unit.isActive ? 'text-slate-500 line-through' : 'text-slate-800',
              ].join(' ')}
              title={rowTitle || unit.name}
            >
              {unit.name}
            </span>
            {!expanded && hasBranch ? (
              <span className="shrink-0 rounded-full bg-slate-100 px-1.5 py-0.5 text-[10px] font-semibold tabular-nums text-slate-500">
                {children.length}
              </span>
            ) : null}
            {canManage ? (
              <>
                <button
                  type="button"
                  className={treeRowMiniBtnSuccess}
                  aria-label={`Thêm đơn vị con dưới ${unit.name}`}
                  title="Thêm đơn vị con"
                  onClick={() => onCreateChild(unit)}
                >
                  <Plus className="size-3" aria-hidden />
                </button>
                <button
                  type="button"
                  className={treeRowMiniBtn}
                  aria-label={`Sửa ${unit.name}`}
                  title="Sửa đơn vị"
                  onClick={() => onEdit(unit)}
                >
                  <Pencil className="size-3" aria-hidden />
                </button>
              </>
            ) : null}
          </div>
          <div className="ml-auto flex shrink-0 flex-wrap items-center justify-end gap-x-2 gap-y-1">
            {legal ? (
              <button
                type="button"
                className={`${treeMetaChip} inline-flex max-w-[14rem] items-center gap-1 border-slate-200/80 bg-slate-50 text-left font-normal text-slate-500 hover:border-teal-600/30 hover:bg-teal-700/5 hover:text-teal-700`}
                title={`Xem pháp nhân: ${legal.name}`}
                onClick={() => onViewLegal(legal)}
              >
                <Landmark className="size-3 shrink-0" aria-hidden />
                <span className="truncate">{legal.name}</span>
              </button>
            ) : null}
            <button
              type="button"
              className={treeMemberChip}
              aria-label={`Thành viên ${unit.name}: ${memberCountLabel}`}
              title="Quản lý thành viên"
              onClick={() => onManageMembers(unit)}
            >
              {memberCountLabel}
            </button>
            {canManage ? (
              <button
                type="button"
                className={
                  unit.isActive
                    ? `${treeMetaChip} border-emerald-600/30 bg-emerald-600/5 text-emerald-600 hover:bg-emerald-600/10`
                    : `${treeMetaChip} border-slate-200 bg-slate-50 text-slate-500 hover:bg-slate-100`
                }
                aria-label={
                  unit.isActive
                    ? `Trạng thái đang sử dụng — chuyển thành ngừng dùng cho ${unit.name}`
                    : `Trạng thái ngừng dùng — chuyển thành đang sử dụng cho ${unit.name}`
                }
                title="Đổi trạng thái đơn vị"
                onClick={() => onToggleActive(unit)}
              >
                {unit.isActive ? 'Đang sử dụng' : 'Ngừng dùng'}
              </button>
            ) : unit.isActive ? (
              <Tag severity="success" className="shrink-0">Đang sử dụng</Tag>
            ) : (
              <Tag severity="secondary" className="shrink-0">Ngừng dùng</Tag>
            )}
          </div>
        </div>
      </div>
      {expanded && hasBranch ? (
        <ul className="m-0 list-none p-0">
          {children.map((child, index) => (
            <TreeRow
              key={child.id}
              unit={child}
              depth={depth + 1}
              isLast={index === children.length - 1}
              byParent={byParent}
              visibleIds={visibleIds}
              collapsed={collapsed}
              legalById={legalById}
              unitTypeByCode={unitTypeByCode}
              unitTypeLabel={unitTypeLabel}
              canManage={canManage}
              onToggle={onToggle}
              onCreateChild={onCreateChild}
              onEdit={onEdit}
              onToggleActive={onToggleActive}
              onViewLegal={onViewLegal}
              onManageMembers={onManageMembers}
              memberCountByUnit={memberCountByUnit}
            />
          ))}
        </ul>
      ) : null}
    </li>
  )
}

function UnitEditor({
  state,
  units,
  sortSiblingUnits,
  byParent,
  unitTypeCatalog,
  legalEntities,
  canManage,
  isSubmitting,
  onChange,
  onClose,
  onSubmit,
}: {
  state: EditorState
  units: OrgUnit[]
  sortSiblingUnits: (items: OrgUnit[]) => OrgUnit[]
  byParent: Map<string, OrgUnit[]>
  unitTypeCatalog: OrgUnitTypeCatalogItem[]
  legalEntities: LegalEntity[]
  canManage: boolean
  isSubmitting: boolean
  onChange: (draft: UnitDraft) => void
  onClose: () => void
  onSubmit: () => void
}) {
  const [legalPickerOpen, setLegalPickerOpen] = useState(false)
  const blocked = state.unit
    ? descendantIds(state.unit.id, byParent).add(state.unit.id)
    : new Set<string>()
  const parentOptions = sortSiblingUnits(units.filter((unit) => !blocked.has(unit.id)))
  const editSiblings = state.mode === 'edit' && state.unit
    ? sortSiblingUnits(
      units.filter(
        (item) => (item.parentId ?? '') === (state.unit?.parentId ?? '') && item.id !== state.unit?.id,
      ),
    )
    : []
  const showParentPicker =
    (state.mode === 'create' && !state.draft.parentId)
    || (state.mode === 'edit' && state.unit?.parentId != null)
  const title = state.mode === 'create'
    ? (state.draft.parentId ? 'Thêm đơn vị con' : 'Thêm đơn vị gốc')
    : 'Sửa đơn vị'
  return (
    <FeatureDialog
      open
      onClose={onClose}
      title={title}
      description="Đơn vị nằm trên cây của tổ chức đang chọn. Gắn pháp nhân tại trường bên dưới hoặc qua nút +."
      size="md"
      onSubmit={(event) => {
        event.preventDefault()
        onSubmit()
      }}
      submitting={isSubmitting}
      submitLabel="Lưu"
    >
      <div className="grid gap-3">
        <label className="grid gap-1 text-sm">
          Tên đơn vị
          <input className={fieldInputClass} value={state.draft.name} required onChange={(event) => onChange({ ...state.draft, name: event.target.value })} />
        </label>
        <label className="grid gap-1 text-sm">
          Mã
          <input className={fieldInputClass} value={state.draft.code} required onChange={(event) => onChange({ ...state.draft, code: event.target.value })} />
        </label>
        <label className="grid gap-1 text-sm">
          Loại đơn vị
          <select className={fieldInputClass} value={state.draft.unitType} onChange={(event) => onChange({ ...state.draft, unitType: event.target.value })}>
            {unitTypeCatalog.map((type) => (
              <option key={type.code} value={type.code}>{type.name}</option>
            ))}
          </select>
        </label>
        <label className="grid gap-1 text-sm">
          Thứ tự hiển thị (SortOrder)
          <input
            className={fieldInputClass}
            type="number"
            inputMode="numeric"
            step={1}
            value={state.draft.sortOrder}
            onChange={(event) => onChange({
              ...state.draft,
              sortOrder: Number.parseInt(event.target.value, 10) || 0,
            })}
          />
          {state.mode === 'edit' ? (
            <>
              <span className="text-xs text-slate-500">
                Giá trị đang lưu trên server: <strong className="font-semibold tabular-nums">{state.unit?.sortOrder ?? 0}</strong>.
                Số nhỏ hiển thị trước trong cùng một đơn vị cha (ví dụ Hà Nội 10, Đà Nẵng 20, HCM 30).
              </span>
              {editSiblings.length > 0 ? (
                <span className="text-xs text-slate-500">
                  Cùng cấp (theo SortOrder API):{' '}
                  {editSiblings.map((item) => `${item.name} (${item.sortOrder})`).join(' · ')}
                </span>
              ) : null}
            </>
          ) : (
            <span className="text-xs text-slate-500">
              Mặc định gợi ý cuối danh sách cùng cấp. Số nhỏ hiển thị trước.
            </span>
          )}
        </label>
        {showParentPicker ? (
          <label className="grid gap-1 text-sm">
            Thuộc đơn vị
            <select className={fieldInputClass} value={state.draft.parentId} onChange={(event) => onChange({ ...state.draft, parentId: event.target.value })}>
              <option value="">Đây là đơn vị gốc</option>
              {parentOptions.map((unit) => (
                <option key={unit.id} value={unit.id}>{unit.name}</option>
              ))}
            </select>
          </label>
        ) : null}
        <label className="grid gap-1 text-sm">
          Pháp nhân
          <div className="flex gap-2">
            <select
              className={`${fieldInputClass} min-w-0 flex-1`}
              value={state.draft.legalEntityId}
              onChange={(event) => onChange({ ...state.draft, legalEntityId: event.target.value })}
            >
              <option value="">Không gắn pháp nhân</option>
              {legalEntities.filter((item) => item.isActive).map((item) => (
                <option key={item.id} value={item.id}>{item.name}</option>
              ))}
            </select>
            <button
              type="button"
              className="inline-flex size-10 shrink-0 items-center justify-center rounded-xl border border-slate-200 bg-white hover:bg-slate-50 disabled:opacity-40"
              aria-label="Quản lý và chọn pháp nhân"
              disabled={!canManage}
              onClick={() => setLegalPickerOpen(true)}
            >
              <Plus className="size-4" aria-hidden />
            </button>
          </div>
        </label>
      </div>
      {legalPickerOpen ? (
        <LegalEntitiesManagerDialog
          canManage={canManage}
          selectedId={state.draft.legalEntityId}
          onSelect={(id) => onChange({ ...state.draft, legalEntityId: id })}
          onClose={() => setLegalPickerOpen(false)}
        />
      ) : null}
    </FeatureDialog>
  )
}

function TemplateDialog({
  hasUnits,
  unitTypeLabel,
  onClose,
  onApplied,
}: {
  hasUnits: boolean
  unitTypeLabel: (code: string) => string
  onClose: () => void
  onApplied: () => Promise<void>
}) {
  const [templateCode, setTemplateCode] = useState<OrgStructureTemplateCode>('simple-company')
  const [rootName, setRootName] = useState('')
  const [preview, setPreview] = useState<TemplateNode[] | null>(null)
  const previewMutation = useMutation({
    mutationFn: () => previewOrgTemplate(templateCode, rootName),
    onError: notifyApiError,
    onSuccess: (nodes) => setPreview(nodes),
  })
  const applyMutation = useMutation({
    mutationFn: () => applyOrgTemplate({ templateCode, rootName, nodes: preview ?? [] }),
    onError: notifyApiError,
    onSuccess: async () => {
      notify.success('Đã ghi mẫu vào cây.')
      await onApplied()
    },
  })
  return (
    <FeatureDialog
      open
      onClose={onClose}
      title="Tạo cây từ mẫu"
      description="Xem các đơn vị sẽ được ghi trước khi xác nhận. Mẫu chỉ dùng khi cây còn trống."
      size="md"
      footer={(
        <>
          <button type="button" className={btnSecondaryClass} onClick={onClose}>Đóng</button>
          <button
            type="button"
            className={btnPrimaryClass}
            disabled={!preview || hasUnits || applyMutation.isPending}
            onClick={() => applyMutation.mutate()}
          >
            Xác nhận ghi mẫu
          </button>
        </>
      )}
    >
      {hasUnits ? (
        <p className="m-0 text-sm text-slate-500">Cây đã có đơn vị. Thêm đơn vị con trên cây thay vì ghi đè bằng mẫu.</p>
      ) : (
        <form
          className="grid gap-3"
          onSubmit={(event) => {
            event.preventDefault()
            previewMutation.mutate()
          }}
        >
          <label className="grid gap-1 text-sm">
            Mô hình doanh nghiệp
            <select
              aria-label="Mô hình doanh nghiệp"
              className={fieldInputClass}
              value={templateCode}
              onChange={(event) => {
                setTemplateCode(event.target.value as OrgStructureTemplateCode)
                setPreview(null)
              }}
            >
              {ORG_STRUCTURE_TEMPLATES.map((item) => (
                <option key={item.code} value={item.code}>{item.label}</option>
              ))}
            </select>
          </label>
          <label className="grid gap-1 text-sm">
            Tên gốc mẫu
            <input
              aria-label="Tên gốc mẫu"
              className={fieldInputClass}
              value={rootName}
              required
              onChange={(event) => {
                setRootName(event.target.value)
                setPreview(null)
              }}
            />
          </label>
          <button type="submit" className={btnSecondaryClass} disabled={previewMutation.isPending}>Xem trước</button>
          {preview ? (
            <ul className="m-0 list-none space-y-1 p-0 text-sm">
              {preview.map((node) => (
                <li key={node.code}>
                  {node.parentCode ? `${node.parentCode} / ` : ''}{node.name} ({unitTypeLabel(node.unitType)})
                </li>
              ))}
            </ul>
          ) : null}
        </form>
      )}
    </FeatureDialog>
  )
}
