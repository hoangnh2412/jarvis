import type { AuthApiClient } from '../../integrations/sample/auth-api.client';
import type { WhoAmI } from '../../integrations/contracts/models';

export async function probeAnonymousAuth(client: AuthApiClient): Promise<WhoAmI> {
  return client.whoAmI();
}
