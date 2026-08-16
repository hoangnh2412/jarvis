import type { HarnessDependencies } from '@jarvis/autotest';
import { AuthApiClient } from '../integrations/sample/auth-api.client';
import { SettingsApiClient } from '../integrations/sample/settings-api.client';

export type SampleClients = {
  auth: AuthApiClient;
  settings: SettingsApiClient;
};

export function createSampleClients(deps: HarnessDependencies): SampleClients {
  return {
    auth: new AuthApiClient(deps.transport, deps.baseUrl, deps.authHeaders),
    settings: new SettingsApiClient(deps.transport, deps.baseUrl, deps.authHeaders),
  };
}
