/**
 * Inject tw-animate-css–compatible enter utilities + premium chart presets.
 * Class names match `tw-animate-css` where possible; premium classes are kit-only.
 */
const STYLE_ID = 'fe-ui-kit-tw-animate-bridge-v2'

const TW_ANIMATE_BRIDGE_CSS = `
@property --tw-animation-delay { syntax: "*"; inherits: false; initial-value: 0s; }
@property --tw-animation-duration { syntax: "*"; inherits: false; }
@property --tw-enter-blur { syntax: "*"; inherits: false; initial-value: 0; }
@property --tw-enter-opacity { syntax: "*"; inherits: false; initial-value: 1; }
@property --tw-enter-rotate { syntax: "*"; inherits: false; initial-value: 0; }
@property --tw-enter-scale { syntax: "*"; inherits: false; initial-value: 1; }
@property --tw-enter-translate-x { syntax: "*"; inherits: false; initial-value: 0; }
@property --tw-enter-translate-y { syntax: "*"; inherits: false; initial-value: 0; }

@keyframes tw-enter {
  from {
    opacity: var(--tw-enter-opacity, 1);
    transform: translate3d(
        var(--tw-enter-translate-x, 0),
        var(--tw-enter-translate-y, 0),
        0
      )
      scale3d(
        var(--tw-enter-scale, 1),
        var(--tw-enter-scale, 1),
        var(--tw-enter-scale, 1)
      )
      rotate(var(--tw-enter-rotate, 0));
    filter: blur(var(--tw-enter-blur, 0));
  }
}

.animate-in {
  animation-name: tw-enter;
  animation-duration: var(--tw-animation-duration, var(--tw-duration, 0.55s));
  animation-timing-function: cubic-bezier(0.16, 1, 0.3, 1);
  animation-delay: var(--tw-animation-delay, 0s);
  animation-iteration-count: 1;
  animation-direction: normal;
  animation-fill-mode: both;
  will-change: transform, opacity, filter;
  backface-visibility: hidden;
}

.animate-in.ease-out-quart {
  animation-timing-function: cubic-bezier(0.25, 1, 0.5, 1);
}
.animate-in.ease-out-expo {
  animation-timing-function: cubic-bezier(0.16, 1, 0.3, 1);
}
.animate-in.ease-out-back {
  animation-timing-function: cubic-bezier(0.34, 1.4, 0.64, 1);
}

.animate-in.duration-300 {
  --tw-animation-duration: 300ms;
  --tw-duration: 300ms;
  animation-duration: 300ms;
}
.animate-in.duration-500 {
  --tw-animation-duration: 500ms;
  --tw-duration: 500ms;
  animation-duration: 500ms;
}
.animate-in.duration-600 {
  --tw-animation-duration: 600ms;
  --tw-duration: 600ms;
  animation-duration: 600ms;
}
.animate-in.duration-700 {
  --tw-animation-duration: 700ms;
  --tw-duration: 700ms;
  animation-duration: 700ms;
}
.animate-in.duration-800 {
  --tw-animation-duration: 800ms;
  --tw-duration: 800ms;
  animation-duration: 800ms;
}
.animate-in.duration-1000 {
  --tw-animation-duration: 1s;
  --tw-duration: 1s;
  animation-duration: 1s;
}

.fade-in { --tw-enter-opacity: 0; }
.zoom-in { --tw-enter-scale: 0; }
.zoom-in-95 { --tw-enter-scale: 0.95; }
.zoom-in-90 { --tw-enter-scale: 0.9; }
.zoom-in-80 { --tw-enter-scale: 0.8; }
.zoom-in-50 { --tw-enter-scale: 0.5; }
.blur-in { --tw-enter-blur: 20px; }
.blur-in-sm { --tw-enter-blur: 8px; }
.blur-in-md { --tw-enter-blur: 14px; }
.spin-in { --tw-enter-rotate: 30deg; }
.spin-in-12 { --tw-enter-rotate: 12deg; }
.spin-in-6 { --tw-enter-rotate: 6deg; }

.slide-in-from-top { --tw-enter-translate-y: -100%; }
.slide-in-from-top-2 { --tw-enter-translate-y: -0.5rem; }
.slide-in-from-top-4 { --tw-enter-translate-y: -1rem; }
.slide-in-from-top-8 { --tw-enter-translate-y: -2rem; }
.slide-in-from-top-12 { --tw-enter-translate-y: -3rem; }
.slide-in-from-bottom { --tw-enter-translate-y: 100%; }
.slide-in-from-bottom-2 { --tw-enter-translate-y: 0.5rem; }
.slide-in-from-bottom-4 { --tw-enter-translate-y: 1rem; }
.slide-in-from-bottom-8 { --tw-enter-translate-y: 2rem; }
.slide-in-from-bottom-12 { --tw-enter-translate-y: 3rem; }
.slide-in-from-left { --tw-enter-translate-x: -100%; }
.slide-in-from-left-2 { --tw-enter-translate-x: -0.5rem; }
.slide-in-from-left-4 { --tw-enter-translate-x: -1rem; }
.slide-in-from-left-8 { --tw-enter-translate-x: -2rem; }
.slide-in-from-left-12 { --tw-enter-translate-x: -3rem; }
.slide-in-from-right { --tw-enter-translate-x: 100%; }
.slide-in-from-right-2 { --tw-enter-translate-x: 0.5rem; }
.slide-in-from-right-4 { --tw-enter-translate-x: 1rem; }
.slide-in-from-right-8 { --tw-enter-translate-x: 2rem; }
.slide-in-from-right-12 { --tw-enter-translate-x: 3rem; }

/* Premium multi-step presets */
@keyframes dashboard-soft-rise {
  0% {
    opacity: 0;
    transform: translate3d(0, 18px, 0) scale(0.96);
    filter: blur(6px);
  }
  100% {
    opacity: 1;
    transform: translate3d(0, 0, 0) scale(1);
    filter: blur(0);
  }
}
@keyframes dashboard-soft-drop {
  0% {
    opacity: 0;
    transform: translate3d(0, -18px, 0) scale(0.96);
    filter: blur(6px);
  }
  100% {
    opacity: 1;
    transform: translate3d(0, 0, 0) scale(1);
    filter: blur(0);
  }
}
@keyframes dashboard-bounce-in {
  0% {
    opacity: 0;
    transform: translate3d(0, 16px, 0) scale(0.92);
  }
  55% {
    opacity: 1;
    transform: translate3d(0, -4px, 0) scale(1.03);
  }
  75% {
    transform: translate3d(0, 2px, 0) scale(0.99);
  }
  100% {
    opacity: 1;
    transform: translate3d(0, 0, 0) scale(1);
  }
}
@keyframes dashboard-elastic-pop {
  0% {
    opacity: 0;
    transform: scale(0.85);
  }
  50% {
    opacity: 1;
    transform: scale(1.06);
  }
  72% {
    transform: scale(0.97);
  }
  100% {
    opacity: 1;
    transform: scale(1);
  }
}
@keyframes dashboard-float-in {
  0% {
    opacity: 0;
    transform: translate3d(0, 28px, 0) scale(0.94);
    filter: blur(10px);
  }
  60% {
    opacity: 1;
    filter: blur(0);
  }
  100% {
    opacity: 1;
    transform: translate3d(0, 0, 0) scale(1);
    filter: blur(0);
  }
}
@keyframes dashboard-reveal-wipe {
  0% {
    opacity: 0;
    clip-path: inset(0 0 100% 0);
    transform: translate3d(0, 8px, 0);
  }
  100% {
    opacity: 1;
    clip-path: inset(0 0 0 0);
    transform: translate3d(0, 0, 0);
  }
}

.dashboard-anim-soft-rise,
.dashboard-anim-soft-drop,
.dashboard-anim-bounce-in,
.dashboard-anim-elastic-pop,
.dashboard-anim-float-in,
.dashboard-anim-reveal-wipe {
  will-change: transform, opacity, filter;
  backface-visibility: hidden;
  animation-fill-mode: both;
}

.dashboard-anim-soft-rise {
  animation: dashboard-soft-rise 0.7s cubic-bezier(0.16, 1, 0.3, 1) both;
}
.dashboard-anim-soft-drop {
  animation: dashboard-soft-drop 0.7s cubic-bezier(0.16, 1, 0.3, 1) both;
}
.dashboard-anim-bounce-in {
  animation: dashboard-bounce-in 0.75s cubic-bezier(0.34, 1.4, 0.64, 1) both;
}
.dashboard-anim-elastic-pop {
  animation: dashboard-elastic-pop 0.7s cubic-bezier(0.34, 1.56, 0.64, 1) both;
}
.dashboard-anim-float-in {
  animation: dashboard-float-in 0.85s cubic-bezier(0.16, 1, 0.3, 1) both;
}
.dashboard-anim-reveal-wipe {
  animation: dashboard-reveal-wipe 0.75s cubic-bezier(0.25, 1, 0.5, 1) both;
}
`

export function ensureTwAnimateStyles(): void {
  if (typeof document === 'undefined') return
  // Drop older bridge if present
  document.getElementById('fe-ui-kit-tw-animate-bridge-v1')?.remove()
  const existing = document.getElementById(STYLE_ID)
  if (existing) {
    existing.textContent = TW_ANIMATE_BRIDGE_CSS
    return
  }
  const style = document.createElement('style')
  style.id = STYLE_ID
  style.textContent = TW_ANIMATE_BRIDGE_CSS
  document.head.appendChild(style)
}
