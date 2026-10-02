import { useCallback, useEffect, useState } from 'react'
import { notify } from '../../../common/Toaster'
import { getErrorMessage } from '../../../lib/getErrorMessage'
import { callGetDynamicForm } from '../services'
import type { DynamicFormDefinition } from '../types'

/**
 * Hook dùng chung: load 1 form theo id (Builder / host fill).
 * Logic UI riêng (save, fields, submit…) để ở page hoặc host.
 */
export function useDynamicForm(formId: string, errorFallback: string) {
  const [form, setForm] = useState<DynamicFormDefinition | null>(null)
  const [loading, setLoading] = useState(true)

  const reload = useCallback(async () => {
    setLoading(true)
    try {
      const res = await callGetDynamicForm(formId)
      setForm(res.data)
    } catch (error) {
      notify.error(getErrorMessage(error, errorFallback))
    } finally {
      setLoading(false)
    }
  }, [formId, errorFallback])

  useEffect(() => {
    void reload()
  }, [reload])

  return { form, setForm, loading, reload }
}
