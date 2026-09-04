import { ApiClient } from '@jarvis/autotest';
import type { JarvisEnvelope, WhoAmI } from '../contracts/models';

export class AuthApiClient extends ApiClient {
  async whoAmI(): Promise<WhoAmI> {
    const body = await this.get<JarvisEnvelope<WhoAmI>>('/api/_auth-test/whoami');
    const data = body.data;
    if (!data) {
      throw new Error('whoami không có data');
    }
    return data;
  }
}
