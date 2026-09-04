import type { SettingsApiClient } from '../../integrations/sample/settings-api.client';
import type { SettingGroup } from '../../integrations/contracts/models';

export async function listSettingGroups(client: SettingsApiClient): Promise<SettingGroup[]> {
  return client.getGroups();
}
