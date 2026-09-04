import type { IBrowserDriver } from '@jarvis/autotest';
import { LoginPage } from '../../ui/pages/LoginPage';

export async function loginAs(
  driver: IBrowserDriver,
  webUrl: string,
  email: string,
  password: string,
): Promise<void> {
  const login = new LoginPage(driver);
  await login.submit(webUrl, email, password);
}
