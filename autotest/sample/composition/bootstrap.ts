import { createTestHarness, type IBrowserDriver, type IHttpTransport } from '@jarvis/autotest';
import type { SampleEnv } from '../config/env';
import { createSampleClients, type SampleClients } from './client-registry';
import { probeAnonymousAuth } from '../application/auth/probe-auth.workflow';
import { listSettingGroups } from '../application/settings/list-groups.workflow';
import { loginAs } from '../application/authentication/login.workflow';
import { openFiles, openSettings, openTenants } from '../application/navigation/open-pages.workflow';

export type SampleApp = {
  api: SampleClients;
  probeWhoAmI: () => ReturnType<typeof probeAnonymousAuth>;
  listSettingGroups: () => ReturnType<typeof listSettingGroups>;
  loginAs: (email: string, password: string) => Promise<void>;
  openSettings: () => Promise<void>;
  openTenants: () => Promise<void>;
  openFiles: () => Promise<void>;
};

export function bootstrap(
  transport: IHttpTransport,
  env: SampleEnv,
  driver?: IBrowserDriver,
): SampleApp {
  const headers = env.apiKey ? { [env.apiKeyHeader]: env.apiKey } : {};
  const api = createTestHarness(
    { transport, baseUrl: env.baseUrl, authHeaders: headers },
    createSampleClients,
  );

  return {
    api,
    probeWhoAmI: () => probeAnonymousAuth(api.auth),
    listSettingGroups: () => listSettingGroups(api.settings),
    loginAs: async (email, password) => {
      if (!driver) {
        throw new Error('loginAs cần IBrowserDriver (UI)');
      }
      await loginAs(driver, env.webUrl, email, password);
    },
    openSettings: async () => {
      if (!driver) {
        throw new Error('openSettings cần IBrowserDriver (UI)');
      }
      await openSettings(driver);
    },
    openTenants: async () => {
      if (!driver) {
        throw new Error('openTenants cần IBrowserDriver (UI)');
      }
      await openTenants(driver);
    },
    openFiles: async () => {
      if (!driver) {
        throw new Error('openFiles cần IBrowserDriver (UI)');
      }
      await openFiles(driver);
    },
  };
}
