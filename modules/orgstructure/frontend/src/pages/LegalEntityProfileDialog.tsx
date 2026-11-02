import { FeatureDialog } from '@platform/core'
import { LegalEntityForm } from '../components/LegalEntityForm'
import { btnSecondaryClass } from '../components/fieldStyles'
import { legalEntityToCompanyForm } from '../legalEntityForm'
import type { LegalEntity } from '../services/orgStructure'

export type LegalEntityProfileDialogProps = {
  entity: LegalEntity
  onClose: () => void
}

export function LegalEntityProfileDialog({ entity, onClose }: LegalEntityProfileDialogProps) {
  const initial = legalEntityToCompanyForm(entity)
  const subtitle = [entity.code, entity.taxId].filter(Boolean).join(' · ')

  return (
    <FeatureDialog
      open
      onClose={onClose}
      title={entity.name}
      description={subtitle ? `Pháp nhân · ${subtitle}` : 'Pháp nhân'}
      size="2xl"
      footer={<button type="button" className={btnSecondaryClass} onClick={onClose}>Đóng</button>}
    >
      <LegalEntityForm
        formId="legal-entity-profile-view"
        initialValues={initial}
        readOnly
        isEdit
        onSubmit={() => undefined}
      />
    </FeatureDialog>
  )
}
