export interface JarvisEnvelope<T> {
  code?: string;
  data?: T;
}

export interface WhoAmI {
  authenticated: boolean;
  scheme: string | null;
  name: string | null;
}

export interface SettingGroup {
  name?: string;
  displayName?: string;
}
