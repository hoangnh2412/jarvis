import type { ControlElementsProp, FullField } from 'react-querybuilder'
import { PrimeAction } from './PrimeAction'
import { PrimeDragHandle } from './PrimeDragHandle'
import { PrimeNotToggle } from './PrimeNotToggle'
import { PrimeSelector } from './PrimeSelector'
import { PrimeValueEditor } from './valueEditors/PrimeValueEditor'

/** Custom PrimeReact 11 controls (ToggleSwitch / Select API mới). */
export const primeReact11ControlElements: ControlElementsProp<FullField, string> =
  {
    actionElement: PrimeAction,
    valueSelector: PrimeSelector,
    valueEditor: PrimeValueEditor,
    notToggle: PrimeNotToggle,
    dragHandle: PrimeDragHandle,
  }
