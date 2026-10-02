import { useRef, type ChangeEvent } from 'react'
import { notify } from '../../../../common/Toaster'
import type { CraftDocBuilderController } from '../../types'
import { getCraftDocMessages, type CraftDocLocale } from '../../localization'
import { CraftDocSplitLayout } from '../CraftDocSplitLayout'
import { DocumentDataForm } from '../DocumentDataForm'
import { DocumentPreviewPane } from '../DocumentPreviewPane'
import { TemplateDesignDrawer } from '../TemplateDesignDrawer'
import { TemplatePickerDialog } from '../TemplatePickerDialog'

export type DocxTemplateBuilderProps = {
  locale?: CraftDocLocale
  controller: CraftDocBuilderController
  className?: string
}

export function DocxTemplateBuilder({
  locale = 'vi',
  controller,
  className = '',
}: DocxTemplateBuilderProps) {
  const messages = getCraftDocMessages(locale)
  const fileInputRef = useRef<HTMLInputElement>(null)

  const handleUploadClick = () => {
    fileInputRef.current?.click()
  }

  const handleFileChange = async (event: ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0]
    event.target.value = ''
    if (!file) return
    if (!file.name.toLowerCase().endsWith('.docx')) {
      notify.error(messages.form.invalidDocx)
      return
    }
    await controller.onUploadFile(file)
  }

  return (
    <div className={`craft-doc-builder h-full min-h-0 w-full overflow-hidden ${className}`}>
      <input
        ref={fileInputRef}
        type="file"
        accept=".docx,application/vnd.openxmlformats-officedocument.wordprocessingml.document"
        className="hidden"
        onChange={handleFileChange}
      />

      <CraftDocSplitLayout
        locale={locale}
        form={
          <DocumentDataForm
            locale={locale}
            template={controller.activeTemplate}
            saving={controller.saving}
            onFieldChange={controller.setFieldValue}
            onFieldValuesChange={controller.setFieldValues}
            onUpload={handleUploadClick}
            onCreateTemplate={() => void controller.onCreateTemplate()}
            onOpenDesign={() => controller.setDesignDrawerOpen(true)}
            onOpenTemplatePicker={() => controller.setTemplatePickerOpen(true)}
            onSave={() => void controller.onSave()}
            onExport={() => void controller.onExport()}
          />
        }
        preview={
          <DocumentPreviewPane
            locale={locale}
            template={controller.activeTemplate}
            fields={controller.fields}
            previewBlob={controller.previewBlob}
            status={controller.status}
            preview={controller.preview}
            onChangeTemplate={() => controller.setTemplatePickerOpen(true)}
          />
        }
      />

      <TemplateDesignDrawer
        locale={locale}
        open={controller.designDrawerOpen}
        onClose={() => controller.setDesignDrawerOpen(false)}
        template={controller.activeTemplate}
        design={controller.design}
      />

      <TemplatePickerDialog
        open={controller.templatePickerOpen}
        onClose={() => controller.setTemplatePickerOpen(false)}
        templates={controller.templates}
        activeTemplateId={controller.activeTemplateId}
        onSelect={controller.selectTemplate}
        onUpload={handleUploadClick}
        onCreateBlank={() => void controller.onCreateTemplate()}
      />
    </div>
  )
}
