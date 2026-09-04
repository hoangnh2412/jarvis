import { useState, type ReactNode } from 'react'
import ConfirmDialog from '../../../../common/ConfirmDialog'
import { AddChartDialog } from '../../components/AddChartDialog'
import { ChartSettingsDialog } from '../../components/ChartSettingsDialog'
import { DashboardCanvas } from '../../components/DashboardCanvas'
import { DashboardHeader } from '../../components/DashboardHeader'
import { useDashboardState } from '../../hooks'
import {
  getDashboardMessages,
  type DashboardLocale,
} from '../../localization'
import type { CanvasChartInstance, ChartSettings } from '../../types'
import {
  resolveDashboardContent,
  type DashboardSlotContent,
} from '../../utils'

export type DashboardPageContentContext = {
  charts: CanvasChartInstance[]
  catalogLoading: boolean
  selectedId: string | null
  openAddDialog: () => void
  saveLayout: () => void
  requestReset: () => void
  reloadCatalog: () => Promise<void>
  DefaultHeader: ReactNode
  DefaultCanvas: ReactNode
  DefaultContent: ReactNode
}

export type DashboardPageProps = {
  locale?: DashboardLocale
  title?: string
  description?: string
  className?: string
  persistLayout?: boolean
  content?: DashboardSlotContent<DashboardPageContentContext>
}

export function DashboardPage({
  locale = 'vi',
  title,
  description,
  className = '',
  persistLayout = true,
  content,
}: DashboardPageProps) {
  const msg = getDashboardMessages(locale)
  const state = useDashboardState({ persistLayout })
  const [addOpen, setAddOpen] = useState(false)
  const [editId, setEditId] = useState<string | null>(null)
  const [resetOpen, setResetOpen] = useState(false)

  const editing = editId
    ? (state.charts.find((c) => c.instanceId === editId) ?? null)
    : null

  const DefaultHeader = (
    <DashboardHeader
      title={title ?? msg.title}
      description={description ?? msg.description}
      chartCount={state.charts.length}
      chartCountLabel={msg.chartCount}
      addLabel={msg.addChart}
      saveLabel={msg.saveLayout}
      resetLabel={msg.resetLayout}
      loading={state.loadingCatalog}
      onAddChart={() => setAddOpen(true)}
      onSave={state.saveLayout}
      onReset={() => setResetOpen(true)}
    />
  )

  const DefaultCanvas = (
    <DashboardCanvas
      charts={state.charts}
      selectedId={state.selectedId}
      onSelect={state.setSelectedId}
      onChangeChart={state.updateChart}
      onChangeCharts={state.updateCharts}
      onEditChart={(id) => setEditId(id)}
      onDeleteChart={state.removeChart}
      emptyTitle={msg.emptyCanvas}
      emptyHint={msg.emptyCanvasHint}
      editLabel={msg.edit}
      deleteLabel={msg.deleteChart}
    />
  )

  const DefaultContent = (
    <div className="flex h-full min-h-0 flex-1 flex-col overflow-hidden">
      {DefaultHeader}
      {DefaultCanvas}
    </div>
  )

  const ctx: DashboardPageContentContext = {
    charts: state.charts,
    catalogLoading: state.loadingCatalog,
    selectedId: state.selectedId,
    openAddDialog: () => setAddOpen(true),
    saveLayout: state.saveLayout,
    requestReset: () => setResetOpen(true),
    reloadCatalog: state.reloadCatalog,
    DefaultHeader,
    DefaultCanvas,
    DefaultContent,
  }

  return (
    <div className={`flex h-full min-h-0 w-full flex-1 flex-col overflow-hidden ${className}`}>
      {resolveDashboardContent(content, ctx, DefaultContent)}

      <AddChartDialog
        visible={addOpen}
        onHide={() => setAddOpen(false)}
        items={state.catalog}
        loading={state.loadingCatalog}
        title={msg.addChartTitle}
        hint={msg.addChartHint}
        addLabel={msg.add}
        cancelLabel={msg.cancel}
        emptyLabel={msg.noCatalog}
        onAdd={(item) => state.addChart(item)}
      />

      <ChartSettingsDialog
        visible={!!editing}
        onHide={() => setEditId(null)}
        instance={editing}
        title={msg.settingsTitle}
        applyLabel={msg.apply}
        cancelLabel={msg.cancel}
        onApply={(settings: ChartSettings) => {
          if (!editId) return
          state.applySettings(editId, settings)
        }}
      />

      <ConfirmDialog
        open={resetOpen}
        onClose={() => setResetOpen(false)}
        onConfirm={() => {
          state.clearCharts()
          setResetOpen(false)
        }}
        title={msg.resetConfirmTitle}
        description={msg.resetConfirmMessage}
        confirmText={msg.delete}
        cancelText={msg.cancel}
      />
    </div>
  )
}
