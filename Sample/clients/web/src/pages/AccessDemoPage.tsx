import { useMemo, useState } from 'react'
import {
  AccessProvider,
  Can,
  allowApproveLimit,
  allowOwner,
  allowSameOrg,
  type AccessSnapshot,
} from '@platform/core'

type Expense = {
  id: string
  title: string
  orgId: string
  createdBy: string
  amount: number
}

const EXPENSES: Expense[] = [
  {
    id: 'exp-1',
    title: 'Văn phòng phẩm',
    orgId: 'org-a',
    createdBy: 'user-employee',
    amount: 1_200_000,
  },
  {
    id: 'exp-2',
    title: 'Chi nhánh khác',
    orgId: 'org-b',
    createdBy: 'user-other',
    amount: 800_000,
  },
  {
    id: 'exp-3',
    title: 'Hợp đồng vượt hạn mức',
    orgId: 'org-a',
    createdBy: 'user-director',
    amount: 20_000_000,
  },
]

const ACTORS: Record<string, AccessSnapshot & { label: string }> = {
  admin: {
    label: 'Admin (System.All)',
    permissions: ['System.All'],
    userId: 'user-admin',
    orgId: 'org-a',
    approveLimit: 1_000_000_000,
  },
  director: {
    label: 'Director — Expense.Approve, org-a, hạn mức 5 triệu',
    permissions: ['Expense.View', 'Expense.Approve'],
    userId: 'user-director',
    orgId: 'org-a',
    approveLimit: 5_000_000,
  },
  employee: {
    label: 'Employee — chỉ Expense.View',
    permissions: ['Expense.View'],
    userId: 'user-employee',
    orgId: 'org-a',
    approveLimit: 0,
  },
}

const money = new Intl.NumberFormat('vi-VN')

export function AccessDemoPage() {
  const [actorId, setActorId] = useState<keyof typeof ACTORS>('director')
  const actor = ACTORS[actorId] ?? ACTORS.director

  const snapshot = useMemo<AccessSnapshot>(
    () => ({
      permissions: actor.permissions,
      userId: actor.userId,
      orgId: actor.orgId,
      approveLimit: actor.approveLimit,
    }),
    [actor],
  )

  return (
    <AccessProvider value={snapshot}>
      <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
        <h1 className="m-0 text-xl font-semibold text-slate-900">
          Phiếu chi — Can (RBAC + ABAC)
        </h1>
        <p className="mt-2 text-sm text-slate-600">
          Nút Duyệt hiện khi có permission <code>Expense.Approve</code> và dòng
          cùng tổ chức, số tiền không vượt hạn mức. Nút Của tôi hiện khi dòng
          thuộc user đang chọn.
        </p>
        <label className="mt-4 flex max-w-xl flex-col gap-1.5 text-sm text-slate-700">
          Người đang đăng nhập
          <select
            className="h-11 rounded-xl border border-slate-200 px-3"
            value={actorId}
            onChange={(event) =>
              setActorId(event.target.value as keyof typeof ACTORS)
            }
          >
            {Object.entries(ACTORS).map(([id, item]) => (
              <option key={id} value={id}>
                {item.label}
              </option>
            ))}
          </select>
        </label>

        <Can
          permission="Expense.View"
          fallback={
            <p className="mt-6 text-sm text-slate-500">
              Không có quyền xem phiếu chi.
            </p>
          }
        >
          <table className="mt-6 w-full border-collapse text-left text-sm">
            <thead>
              <tr className="border-b border-slate-200 text-xs uppercase tracking-wide text-slate-500">
                <th className="py-2 pr-3">Phiếu</th>
                <th className="py-2 pr-3">Tổ chức</th>
                <th className="py-2 pr-3 text-right">Số tiền</th>
                <th className="py-2 text-right">Thao tác</th>
              </tr>
            </thead>
            <tbody>
              {EXPENSES.map((expense) => {
                const sameOrg = allowSameOrg(actor.orgId, expense.orgId)
                const withinLimit = allowApproveLimit(
                  actor.approveLimit,
                  expense.amount,
                )
                const mine = allowOwner(actor.userId, expense.createdBy)
                return (
                  <tr key={expense.id} className="border-b border-slate-100">
                    <td className="py-3 pr-3 font-medium text-slate-800">
                      {expense.title}
                    </td>
                    <td className="py-3 pr-3 text-slate-600">{expense.orgId}</td>
                    <td className="py-3 pr-3 text-right tabular-nums text-slate-800">
                      {money.format(expense.amount)}
                    </td>
                    <td className="py-3 text-right">
                      <div className="flex justify-end gap-2">
                        <Can permission="Expense.View" allowed={mine}>
                          <span className="rounded-lg bg-slate-100 px-2 py-1 text-xs text-slate-600">
                            Của tôi
                          </span>
                        </Can>
                        <Can
                          permission="Expense.Approve"
                          allowed={sameOrg && withinLimit}
                        >
                          <button
                            type="button"
                            className="rounded-lg bg-teal-700 px-3 py-1.5 text-xs font-semibold text-white"
                          >
                            Duyệt
                          </button>
                        </Can>
                      </div>
                    </td>
                  </tr>
                )
              })}
            </tbody>
          </table>
        </Can>
      </div>
    </AccessProvider>
  )
}
