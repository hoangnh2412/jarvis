/** Nút nav / tree — không viền focus */
export const navBtnClass =
  'border-0 outline-none shadow-none focus:border-0 focus:outline-none focus:ring-0 focus:shadow-none focus-visible:border-0 focus-visible:outline-none focus-visible:ring-0 focus-visible:shadow-none'

/** Item active trong cây thư mục — chỉ nền, không box ring */
export const treeItemActiveClass =
  'bg-teal-50 font-medium text-teal-800'

export const treeItemIdleClass =
  'text-slate-700 hover:bg-slate-100/90'

export const btnPrimaryClass =
  'pr-btn-primary inline-flex h-10 w-full items-center justify-center gap-2 rounded-xl border-0 px-4 text-sm font-semibold shadow-sm transition-colors disabled:cursor-not-allowed disabled:opacity-60 ' +
  navBtnClass

export const btnOutlinedClass =
  'pr-btn-outlined inline-flex h-10 w-full items-center justify-center gap-2 rounded-xl border px-4 text-sm font-medium shadow-sm transition-colors disabled:cursor-not-allowed disabled:opacity-60 ' +
  navBtnClass

export const btnTextClass =
  'pr-btn-text inline-flex h-9 items-center justify-center gap-1.5 rounded-lg border-0 px-2.5 text-sm font-medium transition-colors disabled:cursor-not-allowed disabled:opacity-60 ' +
  navBtnClass

export const fieldInputClass =
  'box-border h-10 w-full rounded-xl border border-slate-200 bg-white px-3.5 text-sm text-slate-800 shadow-sm outline-none transition placeholder:text-slate-400 hover:border-slate-300 focus:border-teal-600 focus:ring-2 focus:ring-teal-600/20 disabled:cursor-not-allowed disabled:opacity-70'

/** Nút Actions trên từng dòng */
export const actionsBtnClass =
  'inline-flex h-9 shrink-0 items-center justify-center gap-1.5 rounded-lg border border-teal-600 bg-teal-600 px-3 text-sm font-medium text-white shadow-sm transition hover:bg-teal-700 hover:border-teal-700 disabled:cursor-not-allowed disabled:opacity-60 ' +
  navBtnClass
