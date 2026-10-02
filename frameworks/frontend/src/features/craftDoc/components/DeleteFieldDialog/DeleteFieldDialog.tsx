import ConfirmDialog from '../../../../common/ConfirmDialog'

export type DeleteFieldDialogProps = {
  open: boolean
  fieldLabel?: string
  onClose: () => void
  onConfirm: () => void
}

export function DeleteFieldDialog({
  open,
  fieldLabel,
  onClose,
  onConfirm,
}: DeleteFieldDialogProps) {
  return (
    <ConfirmDialog
      open={open}
      onClose={onClose}
      onConfirm={onConfirm}
      title="Delete field?"
      description={
        fieldLabel
          ? `Remove "${fieldLabel}" from this template? The placeholder will be deleted from the document.`
          : 'Remove this field from the template?'
      }
      confirmText="Delete Field"
      cancelText="Cancel"
    />
  )
}
