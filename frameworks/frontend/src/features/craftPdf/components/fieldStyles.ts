/** Compact inputs for dense editor chrome (not auth forms) */
export const editorFieldClass =
  'box-border h-8 w-full min-w-0 max-w-full rounded-md border border-slate-200 !bg-white px-2 text-[12px] !text-slate-800 outline-none transition-[border-color,box-shadow] ' +
  'placeholder:!text-slate-400 hover:border-slate-300 hover:!bg-white hover:!text-slate-800 ' +
  'focus:!border-teal-600 focus:!bg-white focus:!text-slate-800 focus:ring-2 focus:ring-teal-600/15 ' +
  'focus-visible:!bg-white focus-visible:!text-slate-800 ' +
  '[&:focus]:!bg-white [&:focus]:!text-slate-800 ' +
  'disabled:cursor-not-allowed disabled:!bg-slate-50 disabled:opacity-70'

export const editorBtnGhostClass =
  'inline-flex h-8 items-center justify-center gap-1.5 rounded-md border-0 px-2 text-[12px] font-medium text-slate-600 transition-colors hover:bg-slate-100 hover:text-slate-900 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-slate-400/30 disabled:cursor-not-allowed disabled:opacity-60'

export const editorBtnOutlineClass =
  'inline-flex h-8 items-center justify-center gap-1.5 rounded-md border border-slate-200 bg-white px-3 text-[12px] font-medium text-slate-700 shadow-sm transition-colors hover:border-slate-300 hover:bg-slate-50 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-slate-400/30 disabled:cursor-not-allowed disabled:opacity-60'

export const editorBtnPrimaryClass =
  'pr-btn-primary inline-flex h-8 items-center justify-center gap-1.5 rounded-md border-0 px-3 text-[12px] font-semibold shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-600/30 disabled:cursor-not-allowed disabled:opacity-60'

export const fieldInputClass =
  'box-border h-11 w-full rounded-xl border border-slate-200 bg-white px-3.5 text-sm text-slate-800 shadow-sm outline-none transition-[border-color,box-shadow] placeholder:text-slate-400 hover:border-slate-300 focus:border-teal-600 focus:ring-2 focus:ring-teal-600/20 disabled:cursor-not-allowed disabled:bg-slate-50 disabled:opacity-70'

export const fieldInputInvalidClass = `${fieldInputClass} !border-red-500 focus:!border-red-500 focus:!ring-red-500/20`

export const btnPrimaryClass =
  'pr-btn-primary inline-flex h-11 items-center justify-center gap-2 rounded-xl border-0 px-4 text-sm font-semibold shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-600/30 disabled:cursor-not-allowed disabled:opacity-60'

export const btnOutlinedClass =
  'pr-btn-outlined inline-flex h-10 items-center justify-center gap-2 rounded-xl border px-4 text-sm font-medium shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-slate-400/30 disabled:cursor-not-allowed disabled:opacity-60'

export const btnTextClass =
  'pr-btn-text inline-flex h-9 items-center justify-center gap-1.5 rounded-lg border-0 px-2.5 text-sm font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-slate-400/30 disabled:cursor-not-allowed disabled:opacity-60'
