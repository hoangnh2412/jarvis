/**
 * Ensure GridStack CSS is present (host may forget to import it).
 * Critical for absolute item layout, resize handles, and animate transitions.
 */
const STYLE_ID = 'fe-ui-kit-gridstack-css-v2'

const GRIDSTACK_CSS = `
.grid-stack{position:relative}
.grid-stack-placeholder>.placeholder-content{background-color:rgba(0,0,0,.1);margin:0;position:absolute;width:auto;z-index:0!important}
.grid-stack>.grid-stack-item{position:absolute;padding:0;top:0;width:var(--gs-column-width);height:var(--gs-cell-height)}
.grid-stack>.grid-stack-item>.grid-stack-item-content{margin:0;position:absolute;width:auto;overflow-x:hidden;overflow-y:auto}
.grid-stack>.grid-stack-item.size-to-content:not(.size-to-content-max)>.grid-stack-item-content{overflow-y:hidden}
.grid-stack:not(.grid-stack-rtl)>.grid-stack-item{left:0}
.grid-stack.grid-stack-rtl>.grid-stack-item{right:0}
.grid-stack>.grid-stack-item>.grid-stack-item-content,
.grid-stack>.grid-stack-placeholder>.placeholder-content{
  top:var(--gs-item-margin-top);right:var(--gs-item-margin-right);
  bottom:var(--gs-item-margin-bottom);left:var(--gs-item-margin-left)
}
.grid-stack-item>.ui-resizable-handle{position:absolute;font-size:.1px;display:block;-ms-touch-action:none;touch-action:none;user-select:none;z-index:100}
.grid-stack-item.ui-resizable-disabled>.ui-resizable-handle,
.grid-stack-item.ui-resizable-autohide>.ui-resizable-handle{display:none}
.grid-stack-item>.ui-resizable-ne,
.grid-stack-item>.ui-resizable-nw,
.grid-stack-item>.ui-resizable-se,
.grid-stack-item>.ui-resizable-sw{
  background-image:none;background:transparent;z-index:101
}
.grid-stack-item>.ui-resizable-ne{transform:none}
.grid-stack-item>.ui-resizable-sw{transform:none}
.grid-stack-item>.ui-resizable-nw{transform:none}
.grid-stack-item>.ui-resizable-se{transform:none}
.grid-stack-item>.ui-resizable-nw{cursor:nw-resize;width:20px;height:20px;top:var(--gs-item-margin-top);left:var(--gs-item-margin-left)}
.grid-stack-item>.ui-resizable-n{cursor:n-resize;height:10px;top:var(--gs-item-margin-top);left:25px;right:25px}
.grid-stack-item>.ui-resizable-ne{cursor:ne-resize;width:20px;height:20px;top:var(--gs-item-margin-top);right:var(--gs-item-margin-right)}
.grid-stack-item>.ui-resizable-e{cursor:e-resize;width:12px;top:15px;bottom:15px;right:var(--gs-item-margin-right)}
.grid-stack-item>.ui-resizable-se{cursor:se-resize;width:20px;height:20px;bottom:var(--gs-item-margin-bottom);right:var(--gs-item-margin-right)}
.grid-stack-item>.ui-resizable-s{cursor:s-resize;height:12px;left:25px;bottom:var(--gs-item-margin-bottom);right:25px}
.grid-stack-item>.ui-resizable-sw{cursor:sw-resize;width:20px;height:20px;bottom:var(--gs-item-margin-bottom);left:var(--gs-item-margin-left)}
.grid-stack-item>.ui-resizable-w{cursor:w-resize;width:12px;top:15px;bottom:15px;left:var(--gs-item-margin-left)}
.grid-stack-item.ui-draggable-dragging>.ui-resizable-handle{display:none!important}
.grid-stack-item.ui-draggable-dragging{will-change:left,right,top}
.grid-stack-item.ui-resizable-resizing{will-change:width,height}
.ui-draggable-dragging,.ui-resizable-resizing{z-index:10000}
.ui-draggable-dragging>.grid-stack-item-content,
.ui-resizable-resizing>.grid-stack-item-content{box-shadow:0 12px 28px rgba(15,23,42,.18);opacity:.92}
.grid-stack-animate,.grid-stack-animate .grid-stack-item{
  transition:left .25s ease,right .25s ease,top .25s ease,height .25s ease,width .25s ease
}
.grid-stack-animate .grid-stack-item.ui-draggable-dragging,
.grid-stack-animate .grid-stack-item.ui-resizable-resizing,
.grid-stack-animate .grid-stack-item.grid-stack-placeholder{
  transition:left 0s,right 0s,top 0s,height 0s,width 0s
}
.grid-stack>.grid-stack-item[gs-y="0"]{top:0}
.grid-stack:not(.grid-stack-rtl)>.grid-stack-item[gs-x="0"]{left:0%}
.grid-stack.grid-stack-rtl>.grid-stack-item[gs-x="0"]{right:0%}
`

export function ensureGridstackStyles(): void {
  if (typeof document === 'undefined') return
  const existing = document.getElementById(STYLE_ID)
  if (existing) {
    existing.textContent = GRIDSTACK_CSS
    return
  }
  // Remove older injected versions if any
  document.getElementById('fe-ui-kit-gridstack-css')?.remove()
  const style = document.createElement('style')
  style.id = STYLE_ID
  style.textContent = GRIDSTACK_CSS
  document.head.appendChild(style)
}
