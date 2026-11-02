type FilterFlag = 'unknown' | 'on' | 'off'

let flag: FilterFlag = 'unknown'
let loading: Promise<boolean> | null = null

export function isOrgStructureFilterEnabled(): boolean {
  return flag === 'on'
}

export function beginOrgStructureFilterLoad(load: () => Promise<boolean>): Promise<boolean> {
  if (flag !== 'unknown') return Promise.resolve(flag === 'on')
  loading ??= load()
    .then((enabled) => {
      flag = enabled ? 'on' : 'off'
      return enabled
    })
    .catch(() => {
      flag = 'off'
      return false
    })
  return loading
}
