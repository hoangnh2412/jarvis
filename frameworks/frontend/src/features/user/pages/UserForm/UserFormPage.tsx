import { useEffect, useState, type FormEventHandler } from 'react'
import { notify } from '../../../../common/Toaster'
import { getErrorMessage } from '../../../../lib/getErrorMessage'
import { handleAction, type ActionProps } from '../../../../lib/handleAction'
import { UserForm } from '../../components/UserForm'
import { UserPageShell } from '../../components/UserPageShell'
import { useUserForm } from '../../hooks'
import { getUserMessages, type UserLocale } from '../../localization'
import { getUserListPath, navigateUser } from '../../routes'
import {
  mockCreateUser,
  mockGetUser,
  mockGetUserRoleOptions,
  mockUpdateUser,
} from '../../services'
import type { CreateUserPayload, UpdateUserPayload, UserRoleOption } from '../../types'
import type { UserFormData } from '../../validation'

export type UserFormPageProps = {
  mode: 'create' | 'edit'
  userId?: string
  locale?: UserLocale
  useRoutes?: boolean
  onCancel?: () => void
  onSaved?: () => void
  callback?: {
    create?: ActionProps<{ data: UserFormData }, CreateUserPayload>
    update?: ActionProps<
      { userId: string; data: UserFormData },
      { id: string; data: UpdateUserPayload }
    >
  }
}

function toPayload(data: UserFormData): CreateUserPayload {
  return {
    username: data.username.trim(),
    email: data.email.trim(),
    password: data.password,
    roleId: data.roleId,
    tenantId: data.tenantId?.trim() || null,
    orgId: data.orgId?.trim() || null,
  }
}

export function UserFormPage({
  mode,
  userId,
  locale = 'vi',
  useRoutes = true,
  onCancel,
  onSaved,
  callback,
}: UserFormPageProps) {
  const messages = getUserMessages(locale)
  const [roles, setRoles] = useState<UserRoleOption[]>([])
  const [loading, setLoading] = useState(mode === 'edit')
  const {
    register,
    control,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useUserForm({ mode })

  useEffect(() => {
    let cancelled = false
    void mockGetUserRoleOptions().then((next) => {
      if (!cancelled) setRoles(next)
    })
    return () => {
      cancelled = true
    }
  }, [])

  useEffect(() => {
    if (mode !== 'edit' || !userId) return
    let cancelled = false
    setLoading(true)
    void mockGetUser(userId)
      .then((user) => {
        if (cancelled) return
        if (!user) {
          notify.error(messages.form.notFound)
          if (useRoutes) navigateUser(getUserListPath())
          return
        }
        reset({
          username: user.username,
          email: user.email,
          password: '',
          roleId: user.roleId,
          tenantId: user.tenantId ?? '',
          orgId: user.orgId ?? '',
          isLocked: user.isLocked,
        })
      })
      .catch((error: unknown) => {
        if (!cancelled) {
          notify.error(getErrorMessage(error, messages.form.saveError))
        }
      })
      .finally(() => {
        if (!cancelled) setLoading(false)
      })
    return () => {
      cancelled = true
    }
  }, [messages.form.notFound, messages.form.saveError, mode, reset, useRoutes, userId])

  const leave = () => {
    if (onSaved) {
      onSaved()
      return
    }
    if (onCancel) {
      onCancel()
      return
    }
    if (useRoutes) navigateUser(getUserListPath())
  }

  const onSubmit: FormEventHandler<HTMLFormElement> = handleSubmit(async (data) => {
    try {
      if (mode === 'create') {
        const outcome = await handleAction({
          ctx: { data },
          callback: callback?.create,
          defaultSubmit: mockCreateUser,
          getPayload: ({ data: payload }) => toPayload(payload),
        })
        if (outcome.status === 'cancelled') return
      } else if (userId) {
        const outcome = await handleAction({
          ctx: { userId, data },
          callback: callback?.update,
          defaultSubmit: ({ id, data: payload }) => mockUpdateUser(id, payload),
          getPayload: ({ userId: id, data: payload }) => ({
            id,
            data: { isLocked: payload.isLocked, roleId: payload.roleId },
          }),
        })
        if (outcome.status === 'cancelled') return
      }
      notify.success(messages.form.saveSuccess)
      leave()
    } catch (error) {
      notify.error(getErrorMessage(error, messages.form.saveError))
    }
  })

  return (
    <UserPageShell
      title={mode === 'create' ? messages.form.createTitle : messages.form.editTitle}
      asForm
      onSubmit={onSubmit}
      onCancel={() => {
        if (onCancel) onCancel()
        else if (useRoutes) navigateUser(getUserListPath())
      }}
      submitLabel={messages.form.save}
      cancelLabel={messages.form.cancel}
      isSubmitting={isSubmitting}
    >
      {loading ? (
        <p className="m-0 text-sm text-mute">{messages.list.loading}</p>
      ) : (
        <UserForm
          mode={mode}
          messages={messages}
          register={register}
          control={control}
          errors={errors}
          roles={roles}
        />
      )}
    </UserPageShell>
  )
}
