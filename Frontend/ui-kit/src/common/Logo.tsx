import { memo, useId, useState } from 'react'

type LogoProps = {
  collapsed?: boolean
  showText?: boolean
  size?: 'sm' | 'md' | 'lg' | 'xl'
  className?: string
  /** Tên brand cạnh logo */
  title?: string
  /** Dòng phụ dưới title */
  subtitle?: string
}

const CIRCUIT_PATHS = [
  { d: 'M68 68 L50 68 L50 46 L38 46', dur: 2.4, del: 0 },
  { d: 'M72 62 L72 42 L88 42', dur: 2.0, del: 0.3 },
  { d: 'M132 68 L150 68 L150 46 L162 46', dur: 2.2, del: 0.15 },
  { d: 'M128 62 L128 42 L112 42', dur: 2.5, del: 0.5 },
  { d: 'M68 132 L50 132 L50 154 L38 154', dur: 2.3, del: 0.2 },
  { d: 'M72 138 L72 158 L88 158', dur: 2.6, del: 0.6 },
  { d: 'M132 132 L150 132 L150 154 L162 154', dur: 2.4, del: 0.35 },
  { d: 'M128 138 L128 158 L112 158', dur: 2.0, del: 0.8 },
]

const NODES = [
  { cx: 38, cy: 46 },
  { cx: 88, cy: 42 },
  { cx: 162, cy: 46 },
  { cx: 112, cy: 42 },
  { cx: 38, cy: 154 },
  { cx: 88, cy: 158 },
  { cx: 162, cy: 154 },
  { cx: 112, cy: 158 },
  { cx: 50, cy: 68 },
  { cx: 150, cy: 68 },
  { cx: 50, cy: 132 },
  { cx: 150, cy: 132 },
]

const PACKETS = [
  {
    path: 'M68 68 L50 68 L50 46 L38 46',
    color: '#38BDF8',
    del: 0,
    dur: 2.4,
  },
  {
    path: 'M132 68 L150 68 L150 46 L162 46',
    color: '#A78BFA',
    del: 0.4,
    dur: 2.2,
  },
  {
    path: 'M68 132 L50 132 L50 154 L38 154',
    color: '#34D399',
    del: 0.8,
    dur: 2.6,
  },
  {
    path: 'M132 132 L150 132 L150 154 L162 154',
    color: '#38BDF8',
    del: 1.1,
    dur: 2.8,
  },
]

const PINS = [74, 82, 90, 98, 106, 114, 122]

const pixelSize = {
  sm: { expanded: 56, collapsed: 48 },
  md: { expanded: 80, collapsed: 70 },
  lg: { expanded: 100, collapsed: 90 },
  xl: { expanded: 120, collapsed: 110 },
} as const

const LogoMark = memo(function LogoMark({
  collapsed,
  size,
}: {
  collapsed: boolean
  size: 'sm' | 'md' | 'lg' | 'xl'
}) {
  const uid = useId().replace(/:/g, '')
  const [hovered, setHovered] = useState(false)
  const px = collapsed ? pixelSize[size].collapsed : pixelSize[size].expanded

  return (
    <svg
      width={px}
      height={px}
      viewBox="0 0 200 200"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
      role="img"
      aria-label="Logo"
      onMouseEnter={() => setHovered(true)}
      onMouseLeave={() => setHovered(false)}
      style={{
        cursor: 'pointer',
        flexShrink: 0,
        filter: hovered
          ? 'drop-shadow(0 2px 10px rgba(37,99,235,0.55))'
          : 'drop-shadow(0 1px 4px rgba(37,99,235,0.25))',
        transition: 'filter 0.3s ease',
      }}
    >
      <defs>
        <radialGradient id={`gw-${uid}`} cx="50%" cy="50%" r="50%">
          <stop offset="0%" stopColor="rgba(37,99,235,0.45)" />
          <stop offset="100%" stopColor="rgba(37,99,235,0)" />
        </radialGradient>
        <linearGradient id={`cg-${uid}`} x1="0" y1="0" x2="1" y2="1">
          <stop offset="0%" stopColor="#0F2444" />
          <stop offset="100%" stopColor="#071224" />
        </linearGradient>
        <linearGradient id={`lg-${uid}`} x1="0" y1="0" x2="0" y2="1">
          <stop offset="0%" stopColor="#2563EB" />
          <stop offset="100%" stopColor="#1E40AF" />
        </linearGradient>
        <filter id={`cs-${uid}`} x="-25%" y="-25%" width="150%" height="150%">
          <feDropShadow
            dx="0"
            dy="1"
            stdDeviation="4"
            floodColor="#1D4ED8"
            floodOpacity="0.4"
          />
        </filter>
        <filter id={`gb-${uid}`} x="-50%" y="-50%" width="200%" height="200%">
          <feGaussianBlur stdDeviation="9" />
        </filter>
      </defs>

      <circle
        cx="100"
        cy="100"
        r="46"
        fill={`url(#gw-${uid})`}
        filter={`url(#gb-${uid})`}
      >
        <animate
          attributeName="opacity"
          values="0.5;1;0.5"
          dur="3s"
          repeatCount="indefinite"
        />
      </circle>

      {CIRCUIT_PATHS.map((p, i) => (
        <g key={i}>
          <path
            d={p.d}
            stroke="#1E3A5F"
            strokeWidth="1.5"
            strokeLinecap="round"
            fill="none"
            opacity="0.5"
          />
          <path
            d={p.d}
            stroke="#1D4ED8"
            strokeWidth="1.5"
            strokeLinecap="round"
            fill="none"
            strokeDasharray="36 200"
          >
            <animate
              attributeName="stroke-dashoffset"
              from="236"
              to="-236"
              dur={`${p.dur}s`}
              begin={`${p.del}s`}
              repeatCount="indefinite"
              calcMode="linear"
            />
            <animate
              attributeName="opacity"
              values="0;1;1;0"
              keyTimes="0;0.08;0.88;1"
              dur={`${p.dur}s`}
              begin={`${p.del}s`}
              repeatCount="indefinite"
            />
          </path>
        </g>
      ))}

      {NODES.map((n, i) => (
        <circle key={i} cx={n.cx} cy={n.cy} r="3" fill="#3B82F6">
          <animate
            attributeName="opacity"
            values="0.3;1;0.3"
            dur={`${1.4 + (i % 4) * 0.35}s`}
            begin={`${(i * 0.19) % 1.6}s`}
            repeatCount="indefinite"
          />
        </circle>
      ))}

      {PACKETS.map((pk, i) => (
        <circle key={i} r="3" fill={pk.color} opacity="0">
          <animateMotion
            path={pk.path}
            dur={`${pk.dur}s`}
            begin={`${pk.del}s`}
            repeatCount="indefinite"
            calcMode="linear"
          />
          <animate
            attributeName="opacity"
            values="0;1;1;0"
            keyTimes="0;0.07;0.9;1"
            dur={`${pk.dur}s`}
            begin={`${pk.del}s`}
            repeatCount="indefinite"
          />
          <animate
            attributeName="r"
            values="2;3.5;2"
            dur={`${pk.dur * 0.5}s`}
            begin={`${pk.del}s`}
            repeatCount="indefinite"
          />
        </circle>
      ))}

      <g
        filter={`url(#cs-${uid})`}
        style={{
          transformOrigin: '100px 100px',
          transform: hovered ? 'scale(1.08)' : 'scale(1)',
          transition: 'transform 0.35s cubic-bezier(0.34,1.56,0.64,1)',
        }}
      >
        <rect
          x="62"
          y="62"
          width="76"
          height="76"
          rx="10"
          fill={`url(#cg-${uid})`}
          stroke="#1E3A5F"
          strokeWidth="1.5"
        />

        {PINS.map((x) => (
          <g key={`v${x}`}>
            <rect x={x} y="56" width="3" height="7" rx="1" fill="#1D4ED8" />
            <rect x={x} y="137" width="3" height="7" rx="1" fill="#1D4ED8" />
          </g>
        ))}
        {PINS.map((y) => (
          <g key={`h${y}`}>
            <rect x="56" y={y} width="7" height="3" rx="1" fill="#1D4ED8" />
            <rect x="137" y={y} width="7" height="3" rx="1" fill="#1D4ED8" />
          </g>
        ))}

        <rect
          x="74"
          y="74"
          width="52"
          height="52"
          rx="6"
          fill="#0D2240"
          stroke="#2563EB"
          strokeWidth="1.5"
        />

        <g transform="translate(100,100)">
          <path
            d="M-9 -4 L-9 -11 A9 9 0 0 1 9 -11 L9 -4"
            stroke="#93C5FD"
            strokeWidth="3"
            strokeLinecap="round"
            fill="none"
          />
          <rect
            x="-12"
            y="-4"
            width="24"
            height="18"
            rx="3"
            fill={`url(#lg-${uid})`}
          />
          <circle cx="0" cy="5" r="3.5" fill="#DBEAFE" opacity="0.9" />
          <rect
            x="-1.5"
            y="6"
            width="3"
            height="5"
            rx="1"
            fill="#DBEAFE"
            opacity="0.9"
          />
        </g>
      </g>

      <circle
        cx="100"
        cy="100"
        r="58"
        stroke="#60A5FA"
        strokeWidth="0.8"
        fill="none"
        opacity="0"
      >
        <animate
          attributeName="r"
          values="58;75;58"
          dur="3.5s"
          repeatCount="indefinite"
        />
        <animate
          attributeName="opacity"
          values="0;0.4;0"
          dur="3.5s"
          repeatCount="indefinite"
        />
      </circle>
    </svg>
  )
})

export default function Logo({
  collapsed = false,
  showText = true,
  size = 'md',
  className = '',
  title = '',
  subtitle = '',
}: LogoProps) {
  const visibleText = showText && !collapsed

  return (
    <div className={['flex min-w-0 items-center gap-2.5', className].join(' ')}>
      <LogoMark collapsed={collapsed} size={size} />
      {visibleText && (
        <div className="min-w-0">
          <p className="truncate text-sm font-semibold leading-tight text-slate-900">
            {title}
          </p>
          {subtitle && (
            <p className="truncate text-xs text-slate-500">{subtitle}</p>
          )}
        </div>
      )}
    </div>
  )
}

export type { LogoProps }
