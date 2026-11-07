import type { FormFieldPropertiesLabels } from '../components/FormFieldProperties'
import type { DynamicFormMessages } from '../localization'

export function getFormFieldPropertiesLabels(
  builder: DynamicFormMessages['builder'],
): FormFieldPropertiesLabels {
  return {
    title: builder.properties,
    fieldLabel: builder.fieldLabel,
    fieldKey: builder.fieldKey,
    fieldRequired: builder.fieldRequired,
    fieldPlaceholder: builder.fieldPlaceholder,
    fieldHelp: builder.fieldHelp,
    fieldDefault: builder.fieldDefault,
    fieldDefaultNone: builder.fieldDefaultNone,
    fieldOptions: builder.fieldOptions,
    optionSetup: builder.optionSetup,
    optionValue: builder.optionValue,
    addOption: builder.addOption,
    removeOption: builder.removeOption,
    quickEdit: builder.quickEdit,
    quickEditHint: builder.quickEditHint,
    sortAlpha: builder.sortAlpha,
    sortAlphaHint: builder.sortAlphaHint,
    remindEmpty: builder.remindEmpty,
    remindEmptyHint: builder.remindEmptyHint,
    removeField: builder.removeField,
    close: builder.closeProperties,
  }
}

export function getBuilderGridClass(
  paletteCollapsed: boolean,
  propsOpen: boolean,
): string {
  const cols = paletteCollapsed
    ? propsOpen
      ? 'lg:grid-cols-[3.5rem_minmax(0,1fr)_18rem]'
      : 'lg:grid-cols-[3.5rem_minmax(0,1fr)]'
    : propsOpen
      ? 'lg:grid-cols-[15rem_minmax(0,1fr)_18rem]'
      : 'lg:grid-cols-[15rem_minmax(0,1fr)]'

  return [
    'kit-df-builder-grid grid min-h-0 flex-1 gap-3',
    paletteCollapsed ? 'is-palette-collapsed' : '',
    propsOpen ? 'is-props-open' : '',
    cols,
  ]
    .filter(Boolean)
    .join(' ')
}
