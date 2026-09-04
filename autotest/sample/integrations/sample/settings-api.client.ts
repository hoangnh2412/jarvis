import { ApiClient } from '@jarvis/autotest';
import type { JarvisEnvelope, SettingGroup } from '../contracts/models';

export class SettingsApiClient extends ApiClient {
  async getGroups(): Promise<SettingGroup[]> {
    const body = await this.get<JarvisEnvelope<SettingGroup[]>>('/api/v1/settings/groups');
    return body.data ?? [];
  }
}
