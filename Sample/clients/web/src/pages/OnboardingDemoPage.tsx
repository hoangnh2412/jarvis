import { useState, useEffect, useCallback } from 'react'
import {
  Laptop,
  UserCheck,
  CheckSquare,
  BookOpen,
  GitBranch,
  FileSignature,
  CheckCircle2,
  ExternalLink,
  Play,
  RotateCcw,
  Layers,
  //Database,
  Eye,
  X,
  Clock,
  AlertCircle,
  Loader2,
} from 'lucide-react'
import { notify } from '@platform/core'
import { getAccessToken } from '../auth'
import { getWorkflowPreviewUrl } from '../constants'

interface WorkflowMapping {
  id: string
  stepId: string
  stepCode: string
  workflowDefinitionId: string
  instanceId?: string | null
  workflowName: string
  description?: string
  workflowStatus?: string
  isActive: boolean
  createdAt: string
}

interface OnboardingStep {
  id: string
  stepCode: string
  title: string
  department: string
  description?: string
  order: number
  status: 'Pending' | 'InProgress' | 'Completed' | string
  assignedTo?: string
  createdAt: string
  updatedAt?: string
  workflowMapping?: WorkflowMapping | null
}

function extractArrayData<T>(raw: unknown): T[] {
  if (Array.isArray(raw)) {
    return raw as T[]
  }
  if (raw && typeof raw === 'object') {
    const obj = raw as Record<string, unknown>
    if (Array.isArray(obj.data)) {
      return obj.data as T[]
    }
    if (Array.isArray(obj.items)) {
      return obj.items as T[]
    }
    if (Array.isArray(obj.Items)) {
      return obj.Items as T[]
    }
  }
  return []
}

const getDepartmentBadge = (dept?: string) => {
  if (!dept) {
    return {
      bg: 'bg-slate-50 text-slate-700 border-slate-200',
      text: 'Chưa gán',
    }
  }
  switch (dept.toLowerCase()) {
    case 'hr':
      return {
        bg: 'bg-blue-50 text-blue-700 border-blue-200',
        text: 'HR',
      }
    case 'it admin':
      return {
        bg: 'bg-purple-50 text-purple-700 border-purple-200',
        text: 'IT Admin',
      }
    case 'nhân sự mới':
    case 'employee':
      return {
        bg: 'bg-emerald-50 text-emerald-700 border-emerald-200',
        text: 'Nhân sự mới',
      }
    default:
      return {
        bg: 'bg-slate-50 text-slate-700 border-slate-200',
        text: dept,
      }
  }
}

const getStatusBadge = (status?: string) => {
  switch (status) {
    case 'Completed':
      return {
        bg: 'bg-emerald-100 text-emerald-800 border-emerald-300',
        label: 'Hoàn thành',
        icon: CheckCircle2,
      }
    case 'InProgress':
      return {
        bg: 'bg-blue-100 text-blue-800 border-blue-300 animate-pulse',
        label: 'Đang chạy',
        icon: Clock,
      }
    case 'Pending':
    default:
      return {
        bg: 'bg-slate-100 text-slate-600 border-slate-200',
        label: 'Chờ thực hiện',
        icon: AlertCircle,
      }
  }
}

const getStepIcon = (stepCode?: string) => {
  switch (stepCode) {
    case 'HR_HANDOVER_DEVICE':
      return Laptop
    case 'IT_CREATE_AD':
      return UserCheck
    case 'IT_CREATE_JIRA':
      return CheckSquare
    case 'IT_CREATE_CONFLUENCE':
      return BookOpen
    case 'IT_CREATE_BITBUCKET':
      return GitBranch
    case 'HR_SIGN_PROBATION_CONTRACT':
      return FileSignature
    case 'EMPLOYEE_CONFIRM_ONBOARD':
      return CheckCircle2
    default:
      return Layers
  }
}

export function OnboardingDemoPage() {
  const [steps, setSteps] = useState<OnboardingStep[]>([])
  const [loading, setLoading] = useState(true)
  const [executingId, setExecutingId] = useState<string | null>(null)
  const [resetting, setResetting] = useState(false)
  const [previewWorkflowId, setPreviewWorkflowId] = useState<string | null>(null)
  const [previewInstanceId, setPreviewInstanceId] = useState<string | null>(null)
  const [previewTitle, setPreviewTitle] = useState<string>('')

  const handleOpenPreview = (
    definitionId: string,
    title: string,
    instanceId?: string | null,
  ) => {
    setPreviewWorkflowId(definitionId)
    setPreviewInstanceId(instanceId || null)
    setPreviewTitle(title)
  }

  const fetchSteps = useCallback(async () => {
    try {
      const token = getAccessToken()
      const headers: Record<string, string> = {}
      if (token) {
        headers['Authorization'] = `Bearer ${token}`
      }
      const response = await fetch('/api/v1/onboarding/steps', { headers })
      if (!response.ok) {
        throw new Error('Không thể tải danh sách bước Onboarding.')
      }
      const json: unknown = await response.json()
      const data = extractArrayData<OnboardingStep>(json)
      setSteps(data)
    } catch (err) {
      notify.error(
        err instanceof Error ? err.message : 'Có lỗi khi tải dữ liệu.',
      )
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    // Fetching is the external synchronization performed by this effect.
    // eslint-disable-next-line react-hooks/set-state-in-effect
    void fetchSteps()
  }, [fetchSteps])

  useEffect(() => {
    if (!previewWorkflowId) return

    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        setPreviewWorkflowId(null)
        setPreviewInstanceId(null)
      }
    }

    window.addEventListener('keydown', handleKeyDown)
    return () => window.removeEventListener('keydown', handleKeyDown)
  }, [previewWorkflowId])

  const handleExecute = async (step: OnboardingStep) => {
    setExecutingId(step.id)
    try {
      const token = getAccessToken()
      const headers: Record<string, string> = {
        'Content-Type': 'application/json',
      }
      if (token) {
        headers['Authorization'] = `Bearer ${token}`
      }
      const response = await fetch(`/api/v1/onboarding/steps/${step.id}/execute`, {
        method: 'POST',
        headers,
      })
      if (!response.ok) {
        throw new Error('Thực thi bước thất bại.')
      }
      notify.success(`Đã hoàn thành bước: ${step.title}`)
      await fetchSteps()
    } catch (err) {
      notify.error(
        err instanceof Error ? err.message : 'Lỗi trong quá trình thực thi.',
      )
    } finally {
      setExecutingId(null)
    }
  }

  const handleReset = async () => {
    setResetting(true)
    try {
      const token = getAccessToken()
      const headers: Record<string, string> = {
        'Content-Type': 'application/json',
      }
      if (token) {
        headers['Authorization'] = `Bearer ${token}`
      }
      const response = await fetch('/api/v1/onboarding/reset', {
        method: 'POST',
        headers,
      })
      if (!response.ok) {
        throw new Error('Đặt lại quy trình thất bại.')
      }
      notify.success('Đã đặt lại quy trình Onboarding về ban đầu.')
      await fetchSteps()
    } catch (err) {
      notify.error(
        err instanceof Error ? err.message : 'Lỗi khi đặt lại quy trình.',
      )
    } finally {
      setResetting(false)
    }
  }

  const safeSteps = Array.isArray(steps) ? steps : []
  const completedCount = safeSteps.filter((s) => s.status === 'Completed').length
  const progressPercent =
    safeSteps.length > 0 ? Math.round((completedCount / safeSteps.length) * 100) : 0
  const activeMapping =
    safeSteps.find(
      (step) =>
        step.status === 'InProgress' ||
        step.workflowMapping?.workflowStatus === 'Running',
    )?.workflowMapping || safeSteps[0]?.workflowMapping
  const activeWorkflowId = activeMapping?.workflowDefinitionId || 'wf-onboarding-process'

  return (
    <div className="space-y-7 pb-12">
      {/* Header Banner */}
      <div className="relative overflow-hidden rounded-[1.75rem] bg-slate-950 p-8 text-white shadow-xl shadow-slate-300/40">
        <div className="absolute -right-16 -top-24 h-64 w-64 rounded-full bg-violet-500/20 blur-3xl" />
        <div className="absolute -bottom-28 left-1/3 h-56 w-56 rounded-full bg-indigo-500/15 blur-3xl" />
        <div className="relative z-10 flex flex-col md:flex-row md:items-center md:justify-between gap-6">
          <div>
            <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-violet-400/10 text-violet-200 text-xs font-semibold mb-3 border border-violet-300/20">
              <Layers className="w-3.5 h-3.5" />
              WORKFLOW LAB · ELSA INTEGRATION
            </div>
            <h1 className="text-2xl md:text-3xl font-bold tracking-tight">
              Onboarding nhân sự
            </h1>
            <p className="mt-2 max-w-xl text-sm text-slate-300">
              Theo dõi tiến trình bàn giao và kích hoạt từng bước trên cùng một workflow.
            </p>
          </div>

          <div className="flex items-center gap-3">
            <button
              type="button"
              onClick={() => void handleReset()}
              disabled={resetting || loading}
               className="inline-flex items-center gap-2 rounded-xl bg-white/10 hover:bg-white/15 px-4 py-2.5 text-sm font-semibold text-white border border-white/15 transition-colors backdrop-blur-sm disabled:opacity-50"
            >
              <RotateCcw className={`w-4 h-4 ${resetting ? 'animate-spin' : ''}`} />
              {resetting ? 'Đang đặt lại…' : 'Đặt lại quy trình'}
            </button>
          </div>
        </div>

        {/* Progress Bar Container */}
        <div className="mt-8 pt-6 border-t border-white/10">
          <div className="flex justify-between items-center text-sm font-medium mb-2">
             <span className="text-slate-300">
              Tiến độ hoàn tất: <strong className="text-white">{completedCount} / {safeSteps.length} bước</strong>
            </span>
             <span className="text-violet-200 font-bold">{progressPercent}%</span>
          </div>
           <div className="w-full bg-white/10 rounded-full h-2.5 overflow-hidden border border-white/10">
            <div
               className="bg-violet-400 h-full rounded-full transition-all duration-500"
              style={{ width: `${progressPercent}%` }}
            />
          </div>
        </div>
      </div>

      {/* Loading state */}
      {loading ? (
        <div className="flex flex-col items-center justify-center p-16 bg-white rounded-2xl border border-slate-200 shadow-sm">
           <Loader2 className="w-8 h-8 animate-spin text-violet-600 mb-3" />
          <p className="text-sm text-slate-500 font-medium">Đang tải dữ liệu quy trình Onboarding…</p>
        </div>
      ) : (
        <>
          {/* Section 1: Step Cards Grid */}
          <div className="space-y-4">
            <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
              <div>
                <h2 className="text-lg font-bold text-slate-900">
                   Các bước onboarding
                </h2>
                <p className="text-xs text-slate-500">
                   {safeSteps.length} bước liên kết với Workflow Definition:{' '}
                   <span className="font-mono font-semibold text-violet-700">{activeWorkflowId}</span>
                </p>
              </div>

              <div className="flex items-center gap-2 self-start sm:self-auto">
                <button
                  type="button"
                   onClick={() => {
                     const sharedName =
                      activeMapping?.workflowName ||
                      'Quy trình Onboarding Nhân sự'
                    handleOpenPreview(
                       activeWorkflowId,
                      sharedName,
                      activeMapping?.instanceId,
                    )
                  }}
                   className="inline-flex items-center gap-2 px-4 py-2 text-xs font-semibold text-white bg-violet-600 hover:bg-violet-700 rounded-xl transition-colors"
                >
                  <Eye className="w-4 h-4" />
                  Xem Preview Workflow
                </button>
                <a
                    href={getWorkflowPreviewUrl(activeWorkflowId, activeMapping?.instanceId)}
                  target="_blank"
                  rel="noreferrer"
                  title="Mở preview Elsa tab độc lập"
                   className="p-2 rounded-xl text-slate-500 hover:text-violet-600 hover:bg-violet-50 border border-slate-200 bg-white transition-colors"
                >
                  <ExternalLink className="w-4 h-4" />
                </a>
              </div>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-5">
               {safeSteps.length === 0 ? (
                 <div className="col-span-full rounded-2xl border border-dashed border-slate-300 bg-white px-6 py-12 text-center">
                   <Layers className="mx-auto mb-3 h-8 w-8 text-slate-300" />
                   <p className="text-sm font-semibold text-slate-700">Chưa có bước onboarding</p>
                   <p className="mt-1 text-xs text-slate-500">API chưa trả về dữ liệu quy trình.</p>
                 </div>
               ) : safeSteps.map((step) => {
                const IconComponent = getStepIcon(step.stepCode)
                const dept = getDepartmentBadge(step.department)
                const status = getStatusBadge(step.status)
                const StatusIcon = status.icon
                const isCurrent = step.status === 'InProgress'
                const isDone = step.status === 'Completed'
                const isExecuting = executingId === step.id

                return (
                  <div
                    key={step.id}
                      className={`relative flex flex-col justify-between rounded-2xl border p-5 transition-all duration-200 bg-white shadow-sm hover:-translate-y-0.5 hover:shadow-lg ${
                      isCurrent
                        ? 'border-blue-400 ring-2 ring-blue-100'
                        : isDone
                        ? 'border-emerald-200 bg-emerald-50/20'
                        : 'border-slate-200'
                    }`}
                  >
                    {/* Header card */}
                    <div>
                      <div className="flex items-start justify-between gap-2 mb-3">
                        <div className="flex items-center gap-2.5">
                          <div
                            className={`w-9 h-9 rounded-xl flex items-center justify-center text-sm font-bold ${
                              isDone
                                ? 'bg-emerald-600 text-white'
                                : isCurrent
                                 ? 'bg-indigo-600 text-white'
                                 : 'bg-slate-100 text-slate-700'
                            }`}
                          >
                            <IconComponent className="w-5 h-5" />
                          </div>
                          <div>
                            <span className="text-[11px] font-bold text-slate-600 uppercase tracking-wider">
                              Bước {step.order}
                            </span>
                            <span className="block text-xs font-mono text-slate-600 font-medium">
                              {step.stepCode}
                            </span>
                          </div>
                        </div>

                        <span
                          className={`inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-medium border ${status.bg}`}
                        >
                          <StatusIcon className="w-3 h-3" />
                          {status.label}
                        </span>
                      </div>

                      {/* Title & Desc */}
                      <h3 className="text-sm font-bold text-slate-900 line-clamp-2 min-h-[40px]">
                        {step.title}
                      </h3>
                      <p className="mt-1 text-xs text-slate-500 line-clamp-2 min-h-[32px]">
                        {step.description || 'Không có mô tả chi tiết.'}
                      </p>

                      {/* Meta Tags */}
                      <div className="mt-3.5 pt-3 border-t border-slate-100 flex flex-wrap gap-2 text-xs">
                        <span
                          className={`inline-flex items-center px-2 py-0.5 rounded-md font-medium border ${dept.bg}`}
                        >
                          {dept.text}
                        </span>
                        {step.assignedTo && (
                          <span className="inline-flex items-center px-2 py-0.5 rounded-md bg-slate-100 text-slate-600 font-medium">
                            👤 {step.assignedTo}
                          </span>
                        )}
                      </div>
                    </div>

                    {/* Actions */}
                    <div className="mt-4 pt-3 border-t border-slate-100 flex items-center gap-2">
                      <button
                        type="button"
                        onClick={() => void handleExecute(step)}
                        disabled={isExecuting || isDone}
                        className={`w-full inline-flex items-center justify-center gap-1.5 py-2 px-3 rounded-xl text-xs font-semibold transition-all ${
                          isDone
                            ? 'bg-slate-100 text-slate-400 cursor-not-allowed'
                            : isCurrent
                             ? 'bg-violet-600 hover:bg-violet-700 text-white shadow-sm'
                             : 'bg-slate-800 hover:bg-slate-900 text-white'
                        }`}
                      >
                        {isExecuting ? (
                          <>
                            <Loader2 className="w-3.5 h-3.5 animate-spin" />
                            Đang chạy…
                          </>
                        ) : isDone ? (
                          <>
                            <CheckCircle2 className="w-3.5 h-3.5" />
                            Đã hoàn thành
                          </>
                        ) : (
                          <>
                            <Play className="w-3.5 h-3.5" />
                            Thực thi bước
                          </>
                        )}
                      </button>
                    </div>
                  </div>
                )
              })}
            </div>
          </div>

          {/* Section 2: Business Workflow Mapping Table */}
          
        </>
      )}

      {/* Elsa Workflow Preview Modal */}
      {previewWorkflowId && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/70 backdrop-blur-sm p-4" role="presentation">
          <div
            className="relative flex flex-col w-full max-w-5xl h-[85vh] bg-white rounded-2xl shadow-2xl overflow-hidden border border-slate-200 animate-in fade-in zoom-in duration-150"
            role="dialog"
            aria-modal="true"
            aria-labelledby="workflow-preview-title"
          >
            {/* Modal Header */}
            <div className="flex items-center justify-between px-6 py-4 border-b border-slate-200 bg-slate-50">
              <div className="flex items-center gap-2.5">
                  <div className="w-8 h-8 rounded-lg bg-violet-600 text-white flex items-center justify-center font-bold">
                  <Layers className="w-4 h-4" />
                </div>
                <div>
                  <div className="flex items-center gap-2">
                     <h3 id="workflow-preview-title" className="text-sm font-bold text-slate-900">
                      Sơ đồ Flowchart Elsa Preview: {previewTitle}
                    </h3>
                    {previewInstanceId ? (
                      <span className="inline-flex items-center gap-1 text-[11px] font-semibold text-purple-700 bg-purple-50 border border-purple-200 px-2 py-0.5 rounded-full">
                        <span className="w-1.5 h-1.5 rounded-full bg-purple-600 animate-pulse"></span>
                        Runtime Instance: {previewInstanceId}
                      </span>
                    ) : (
                      <span className="inline-flex items-center gap-1 text-[11px] font-semibold text-slate-600 bg-slate-100 border border-slate-200 px-2 py-0.5 rounded-full">
                        Workflow Definition
                      </span>
                    )}
                  </div>
                  <span className="text-xs font-mono text-slate-500">
                    /preview/{previewWorkflowId}
                    {previewInstanceId ? `/${previewInstanceId}` : ''}
                  </span>
                </div>
              </div>

              <div className="flex items-center gap-2">
                <a
                  href={getWorkflowPreviewUrl(previewWorkflowId, previewInstanceId)}
                  target="_blank"
                  rel="noreferrer"
                   className="inline-flex items-center gap-1.5 text-xs font-semibold text-violet-700 bg-white hover:bg-violet-50 border border-slate-200 px-3 py-1.5 rounded-lg transition-colors"
                >
                  <ExternalLink className="w-3.5 h-3.5" />
                  Mở tab mới
                </a>
                <button
                  type="button"
                  onClick={() => {
                    setPreviewWorkflowId(null)
                    setPreviewInstanceId(null)
                  }}
                   aria-label="Đóng preview workflow"
                   className="p-1.5 rounded-lg text-slate-400 hover:text-slate-600 hover:bg-slate-200/60 transition-colors"
                >
                  <X className="w-5 h-5" />
                </button>
              </div>
            </div>

            {/* Modal Body: Iframe loading Elsa preview */}
            <div className="flex-1 bg-slate-100 relative">
              <iframe
                src={getWorkflowPreviewUrl(previewWorkflowId, previewInstanceId)}
                title="Elsa Workflow Preview"
                className="w-full h-full border-0"
              />
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
