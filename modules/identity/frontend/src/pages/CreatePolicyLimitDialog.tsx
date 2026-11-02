import { useEffect, useMemo, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { FeatureDialog, notify } from '@platform/core'
import { Plus, Trash2 } from 'lucide-react'
import {
  createAuthorizationPolicy,
  listAuthorizationOperands,
  listAuthorizationOperators,
  updateAuthorizationPolicy,
  type AuthorizationPolicyDetail,
} from '../services/policies'
import { identityQueryKeys } from '../services/users'
import { notifyApiError } from '../notify'
import {
  canMutatePolicyCatalog,
  inferRightInputMode,
  newConditionDraft,
  operandDisplayLabel,
  operatorDisplayLabel,
  operatorPrefersLiteral,
  policyToDraft,
  type ConditionDraft,
} from './policyLimitUi'
import { btnAuxClass, btnPrimaryClass, btnSecondaryClass, fieldInputClass, labelClass } from '../components/fieldStyles'

export type CreatePolicyLimitDialogProps = {
  open: boolean
  editPolicy: AuthorizationPolicyDetail | null
  onClose: () => void
  onSaved?: () => void
}

export function CreatePolicyLimitDialog({ open, editPolicy, onClose, onSaved }: CreatePolicyLimitDialogProps) {
  const queryClient = useQueryClient()
  const isEdit = Boolean(editPolicy)
  const editLocked = editPolicy ? !canMutatePolicyCatalog(editPolicy) : false

  const operandsQuery = useQuery({
    queryKey: identityQueryKeys.authorizationOperands,
    queryFn: listAuthorizationOperands,
    enabled: open,
  })
  const operatorsQuery = useQuery({
    queryKey: identityQueryKeys.authorizationOperators,
    queryFn: listAuthorizationOperators,
    enabled: open,
  })

  const operands = operandsQuery.data ?? []
  const operators = operatorsQuery.data ?? []
  const operandPaths = useMemo(() => operands.map((o) => o.path), [operands])
  const collectionOperandPaths = useMemo(
    () => operands.filter((o) => o.kind === 'Collection').map((o) => o.path),
    [operands],
  )
  const operatorsForUi = useMemo(
    () => (collectionOperandPaths.length > 0
      ? operators
      : operators.filter((op) => op.toUpperCase() !== 'IN')),
    [collectionOperandPaths.length, operators],
  )

  const [draft, setDraft] = useState(() => policyToDraft(null))

  useEffect(() => {
    if (!open) return
    setDraft(policyToDraft(editPolicy, operandPaths))
    // operandPaths nạp sau; chỉ khởi tạo lại khi mở dialog hoặc đổi policy sửa.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, editPolicy?.id])

  useEffect(() => {
    if (!open || collectionOperandPaths.length > 0) return
    setDraft((current) => {
      const needsFix = current.rules.some((rule) => rule.operator.toUpperCase() === 'IN')
      if (!needsFix) return current
      return {
        ...current,
        rules: current.rules.map((rule) => (
          rule.operator.toUpperCase() === 'IN'
            ? { ...rule, operator: '==', rightMode: 'operand' as const }
            : rule
        )),
      }
    })
  }, [collectionOperandPaths.length, open])

  const saveMutation = useMutation({
    mutationFn: async () => {
      const input = {
        code: draft.code.trim(),
        name: draft.name.trim(),
        resourceType: draft.resourceType.trim() || undefined,
        action: draft.action.trim() || undefined,
        rules: draft.rules.map((rule, index) => ({
          order: index + 1,
          leftOperand: rule.leftOperand,
          operator: rule.operator,
          rightOperand: rule.rightOperand,
        })),
      }
      return draft.id
        ? updateAuthorizationPolicy(draft.id, input)
        : createAuthorizationPolicy(input)
    },
    onError: notifyApiError,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: identityQueryKeys.authorizationPolicies })
      await queryClient.invalidateQueries({ queryKey: identityQueryKeys.policyLimits })
      await queryClient.invalidateQueries({ queryKey: identityQueryKeys.permissionCatalog })
      notify.success(isEdit ? 'Đã lưu phạm vi truy cập.' : 'Đã tạo phạm vi truy cập.')
      onSaved?.()
      onClose()
    },
  })

  const addConditionRow = () => {
    setDraft((current) => ({
      ...current,
      rules: [...current.rules, newConditionDraft(current.rules.length + 1)],
    }))
  }

  const removeConditionRow = (key: string) => {
    setDraft((current) => {
      const next = current.rules.filter((rule) => rule.key !== key)
      return { ...current, rules: next.length > 0 ? next : [newConditionDraft(1)] }
    })
  }

  const updateCondition = (key: string, patch: Partial<ConditionDraft>) => {
    setDraft((current) => ({
      ...current,
      rules: current.rules.map((rule) => (rule.key === key ? { ...rule, ...patch } : rule)),
    }))
  }

  const changeOperator = (key: string, operator: string) => {
    const rule = draft.rules.find((row) => row.key === key)
    if (!rule) return
    if (operatorPrefersLiteral(operator)) {
      updateCondition(key, {
        operator,
        rightMode: 'literal',
        rightOperand: rule.rightMode === 'literal' ? rule.rightOperand : '',
      })
      return
    }
    if (operator.toUpperCase() === 'IN') {
      const defaultRight = collectionOperandPaths.includes(rule.rightOperand)
        ? rule.rightOperand
        : collectionOperandPaths[0] ?? ''
      updateCondition(key, { operator: 'IN', rightMode: 'operand', rightOperand: defaultRight })
      return
    }
    updateCondition(key, { operator })
  }

  const canSave = !editLocked && draft.code.trim().length > 0 && draft.name.trim().length > 0

  return (
    <FeatureDialog
      open={open}
      onClose={onClose}
      title={isEdit ? 'Sửa phạm vi truy cập' : 'Tạo phạm vi truy cập'}
      description={isEdit ? editPolicy?.code : 'Điều kiện chi tiết (ABAC)'}
      size="xl"
      footer={(
        <>
          <button type="button" className={btnSecondaryClass} onClick={onClose}>Hủy</button>
          <button
            type="button"
            className={btnPrimaryClass}
            disabled={saveMutation.isPending || !canSave}
            onClick={() => saveMutation.mutate()}
          >
            {isEdit ? 'Lưu phạm vi' : 'Tạo phạm vi'}
          </button>
        </>
      )}
    >
      <section className="min-w-0 space-y-4">
        {editLocked ? (
          <p className="rounded-xl border border-amber-200 bg-amber-50 px-3 py-2 text-sm text-slate-800">
            Phạm vi này không thể sửa{editPolicy?.isSystemPolicy ? ' (phạm vi hệ thống)' : ''}
            {editPolicy?.isInUse ? ' — đang gán cho vai trò, cần gỡ gán trước khi sửa.' : '.'}
          </p>
        ) : null}
        <div className="space-y-2 text-pretty text-sm text-slate-500">
          <p>
            Mỗi điều kiện so sánh <strong>dữ liệu cần lọc</strong> với thuộc tính user hoặc giá trị cố định (vd. 20&nbsp;000&nbsp;000).
          </p>
          <p>
            Thêm nhiều điều kiện: user chỉ cần thỏa <strong>một</strong> điều kiện (HOẶC).
          </p>
        </div>
        <div className="grid gap-3 sm:grid-cols-2">
          <label className="block">
            <span className={labelClass}>Mã phạm vi</span>
            <input
              className={fieldInputClass}
              value={draft.code}
              disabled={editLocked || isEdit}
              onChange={(e) => setDraft((current) => ({ ...current, code: e.target.value }))}
              placeholder="VD: GIOI_HAN_DUYET"
            />
          </label>
          <label className="block">
            <span className={labelClass}>Mô tả hiển thị</span>
            <input
              className={fieldInputClass}
              value={draft.name}
              disabled={editLocked}
              onChange={(e) => setDraft((current) => ({ ...current, name: e.target.value }))}
              placeholder="VD: Hạn mức duyệt trong đơn vị"
            />
          </label>
          <label className="block">
            <span className={labelClass}>Loại dữ liệu (resource)</span>
            <input
              className={fieldInputClass}
              value={draft.resourceType}
              disabled={editLocked}
              onChange={(e) => setDraft((current) => ({ ...current, resourceType: e.target.value }))}
              placeholder="VD: Invoice"
            />
          </label>
          <label className="block">
            <span className={labelClass}>Hành động (action)</span>
            <input
              className={fieldInputClass}
              value={draft.action}
              disabled={editLocked}
              onChange={(e) => setDraft((current) => ({ ...current, action: e.target.value }))}
              placeholder="VD: Approve"
            />
          </label>
        </div>

        <div className="space-y-3">
          <div className="flex items-center justify-between">
            <h3 className="text-sm font-semibold text-slate-900">Điều kiện lọc dữ liệu</h3>
            {!editLocked ? (
              <button type="button" className={btnAuxClass} onClick={addConditionRow}>
                <Plus className="size-4" aria-hidden /> Thêm điều kiện
              </button>
            ) : null}
          </div>
          {draft.rules.map((rule, index) => {
            const useLiteral = rule.rightMode === 'literal'
            const isCollectionOp = rule.operator.toUpperCase() === 'IN'
            const rightOptions = isCollectionOp ? collectionOperandPaths : operandPaths
            return (
              <div key={rule.key} className="grid items-end gap-2 rounded-xl border border-slate-200 p-3 sm:grid-cols-[1fr_auto_1fr_auto]">
                <label className="block min-w-0">
                  <span className={labelClass}>Dữ liệu cần lọc</span>
                  <select
                    className={fieldInputClass}
                    value={rule.leftOperand}
                    disabled={editLocked}
                    onChange={(e) => updateCondition(rule.key, { leftOperand: e.target.value })}
                  >
                    {operandPaths.map((path) => (
                      <option key={path} value={path}>{operandDisplayLabel(path)} · {path}</option>
                    ))}
                  </select>
                </label>
                <label className="block">
                  <span className={labelClass}>So sánh</span>
                  <select
                    className={fieldInputClass}
                    value={rule.operator}
                    disabled={editLocked}
                    onChange={(e) => changeOperator(rule.key, e.target.value)}
                  >
                    {operatorsForUi.map((op) => (
                      <option key={op} value={op}>{operatorDisplayLabel(op)} ({op})</option>
                    ))}
                  </select>
                </label>
                <label className="block min-w-0">
                  <span className={labelClass}>{useLiteral ? 'Giá trị cố định' : 'Thuộc tính user'}</span>
                  {useLiteral ? (
                    <input
                      className={fieldInputClass}
                      value={rule.rightOperand}
                      disabled={editLocked}
                      onChange={(e) => updateCondition(rule.key, { rightOperand: e.target.value })}
                      placeholder="VD: 20000000"
                    />
                  ) : (
                    <select
                      className={fieldInputClass}
                      value={rule.rightOperand}
                      disabled={editLocked}
                      onChange={(e) => updateCondition(rule.key, { rightOperand: e.target.value })}
                    >
                      {rightOptions.map((path) => (
                        <option key={path} value={path}>{operandDisplayLabel(path)} · {path}</option>
                      ))}
                    </select>
                  )}
                  {!operatorPrefersLiteral(rule.operator) && !isCollectionOp ? (
                    <button
                      type="button"
                      className="mt-1 text-xs font-medium text-teal-700 hover:underline"
                      disabled={editLocked}
                      onClick={() => updateCondition(rule.key, {
                        rightMode: useLiteral ? 'operand' : 'literal',
                        rightOperand: useLiteral ? (operandPaths[0] ?? '') : '',
                      })}
                    >
                      {useLiteral ? 'Chọn thuộc tính' : 'Nhập giá trị cố định'}
                    </button>
                  ) : null}
                </label>
                <button
                  type="button"
                  className={btnAuxClass}
                  disabled={editLocked}
                  aria-label={`Xóa điều kiện ${index + 1}`}
                  onClick={() => removeConditionRow(rule.key)}
                >
                  <Trash2 className="size-4" aria-hidden />
                </button>
              </div>
            )
          })}
        </div>
      </section>
    </FeatureDialog>
  )
}

export { inferRightInputMode }
