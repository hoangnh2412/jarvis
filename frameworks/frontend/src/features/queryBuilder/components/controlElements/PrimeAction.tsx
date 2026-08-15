import type { ReactNode } from 'react'
import { FolderPlus, Plus, X } from 'lucide-react'
import { Button } from 'primereact/button'
import type { ActionProps } from 'react-querybuilder'
import {
  btnAddGroupClass,
  btnAddRuleClass,
  btnClass,
  btnDangerClass,
} from './styles'

export function PrimeAction({
  className,
  handleOnClick,
  label,
  title,
  disabled,
  disabledTranslation,
  testID,
}: ActionProps) {
  const isRemove = testID === 'remove-rule' || testID === 'remove-group'
  const isAddRule = testID === 'add-rule'
  const isAddGroup = testID === 'add-group'

  const resolvedLabel =
    disabledTranslation && disabled ? disabledTranslation.label : label

  let content: ReactNode = resolvedLabel
  let btnCls = btnClass

  if (isRemove) {
    btnCls = btnDangerClass
    content = <X className="size-4" aria-hidden />
  } else if (isAddRule) {
    btnCls = btnAddRuleClass
    content = (
      <>
        <Plus className="size-3.5" aria-hidden />
        <span>Rule</span>
      </>
    )
  } else if (isAddGroup) {
    btnCls = btnAddGroupClass
    content = (
      <>
        <FolderPlus className="size-3.5" aria-hidden />
        <span>Group</span>
      </>
    )
  }

  return (
    <Button
      type="button"
      unstyled
      title={disabledTranslation && disabled ? disabledTranslation.title : title}
      disabled={disabled && !disabledTranslation}
      data-testid={testID}
      aria-label={typeof resolvedLabel === 'string' ? resolvedLabel : title}
      className={[btnCls, className].filter(Boolean).join(' ')}
      onClick={(e: { preventDefault: () => void }) => {
        e.preventDefault()
        handleOnClick(e as never)
      }}
    >
      {content}
    </Button>
  )
}
