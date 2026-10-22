import { useCallback, useEffect, useState } from 'react'
import { Button } from 'primereact/button'
import ConfirmDialog from '../../../../common/ConfirmDialog'
import { notify } from '../../../../common/Toaster'
import { getErrorMessage } from '../../../../lib/getErrorMessage'
import { handleAction, type ActionProps } from '../../../../lib/handleAction'
import { UserDetailView } from '../../components/UserDetailView'
import { UserPageShell } from '../../components/UserPageShell'
import { btnOutlinedClass, btnPrimaryClass } from '../../components/fieldStyles'
import { getUserMessages, type UserLocale } from '../../localization'
import { getUserEditPath, getUserListPath, navigateUser } from '../../routes'
import { mockDeleteUser, mockGetUser, mockUpdateUser } from '../../services'
import type { AdminUser } from '../../types'

export type UserDetailPageProps = {
  userId: string
  locale?: UserLocale
  useRoutes?: boolean
  callback?: {
    lock?: ActionProps<
      { user: AdminUser; isLocked: boolean },
      { id: string; isLocked: boolean },
      AdminUser
    >
    delete?: ActionProps<{ user: AdminUser }, AdminUser>
  }
}

export function UserDetailPage({
  userId,
  locale = 'vi',
  useRoutes = true,
  callback,
}: UserDetailPageProps) {
  const messages = getUserMessages(locale)
  const [user, setUser] = useState<AdminUser | null>(null)
  const [loading, setLoading] = useState(true)
  const [pendingDelete, setPendingDelete] = useState(false)
  const [working, setWorking] = useState(false)

  const load = useCallback(async () => {
    setLoading(true)
    try {
      const next = await mockGetUser(userId)
      if (!next) {
        notify.error(messages.form.notFound)
        if (useRoutes) navigateUser(getUserListPath())
        return
      }
      setUser(next)
    } catch (error) {
      notify.error(getErrorMessage(error, messages.form.saveError))
    } finally {
      setLoading(false)
    }
  }, [messages.form.notFound, messages.form.saveError, useRoutes, userId])

  useEffect(() => {
    void load()
  }, [load])

  const toggleLock = async () => {
    if (!user) return
    setWorking(true)
    const nextLocked = !user.isLocked
    try {
      const outcome = await handleAction<
        { user: AdminUser; isLocked: boolean },
        { id: string; isLocked: boolean },
        AdminUser
      >({
        ctx: { user, isLocked: nextLocked },
        callback: callback?.lock,
        defaultSubmit: ({ id, isLocked }) =>
          mockUpdateUser(id, { isLocked, roleId: user.roleId }),
        getPayload: ({ user: current, isLocked }) => ({
          id: current.id,
          isLocked,
        }),
      })
      if (outcome.status === 'cancelled') return
      setUser(outcome.result)
      notify.success(nextLocked ? messages.form.lockSuccess : messages.form.unlockSuccess)
    } catch (error) {
      notify.error(getErrorMessage(error, messages.form.saveError))
    } finally {
      setWorking(false)
    }
  }

  const confirmDelete = async () => {
    if (!user) return
    setWorking(true)
    try {
      const outcome = await handleAction({
        ctx: { user },
        callback: callback?.delete,
        defaultSubmit: (current) => mockDeleteUser(current.id),
        getPayload: ({ user: current }) => current,
      })
      if (outcome.status === 'cancelled') return
      notify.success(messages.form.deleteSuccess)
      if (useRoutes) navigateUser(getUserListPath())
    } catch (error) {
      notify.error(getErrorMessage(error, messages.form.saveError))
    } finally {
      setWorking(false)
      setPendingDelete(false)
    }
  }

  return (
    <>
      <UserPageShell
        title={messages.form.detailTitle}
        headerActions={
          user ? (
            <>
              <Button
                type="button"
                unstyled
                className={btnOutlinedClass}
                disabled={working}
                onClick={() => void toggleLock()}
              >
                {user.isLocked ? messages.form.unlock : messages.form.lock}
              </Button>
              <Button
                type="button"
                unstyled
                className={btnPrimaryClass}
                onClick={() => {
                  if (useRoutes) navigateUser(getUserEditPath(user.id))
                }}
              >
                {messages.actions.edit}
              </Button>
            </>
          ) : null
        }
      >
        {loading || !user ? (
          <p className="m-0 text-sm text-mute">{messages.list.loading}</p>
        ) : (
          <div className="flex flex-col gap-4">
            <UserDetailView user={user} messages={messages} />
            <div>
              <Button
                type="button"
                unstyled
                className="pr-btn-danger inline-flex h-10 items-center rounded-xl px-4 text-sm font-medium"
                onClick={() => setPendingDelete(true)}
              >
                {messages.actions.delete}
              </Button>
            </div>
          </div>
        )}
      </UserPageShell>
      <ConfirmDialog
        open={pendingDelete}
        onClose={() => setPendingDelete(false)}
        onConfirm={() => void confirmDelete()}
        loading={working}
        title={messages.confirm.deleteTitle}
        description={
          user ? messages.confirm.deleteDescription(user.username) : undefined
        }
        confirmText={messages.confirm.confirm}
        cancelText={messages.confirm.cancel}
      />
    </>
  )
}
