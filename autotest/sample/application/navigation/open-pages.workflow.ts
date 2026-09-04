import type { IBrowserDriver } from '@jarvis/autotest';
import { AdminLayout } from '../../ui/pages/AdminLayout';
import { FileManagerPage } from '../../ui/pages/FileManagerPage';
import { SettingPage } from '../../ui/pages/SettingPage';
import { TenantListPage } from '../../ui/pages/TenantListPage';

export async function openSettings(driver: IBrowserDriver): Promise<void> {
  const layout = new AdminLayout(driver);
  await layout.openSettings();
  await new SettingPage(driver).expectLoaded();
}

export async function openTenants(driver: IBrowserDriver): Promise<void> {
  const layout = new AdminLayout(driver);
  await layout.openTenants();
  await new TenantListPage(driver).expectLoaded();
}

export async function openFiles(driver: IBrowserDriver): Promise<void> {
  const layout = new AdminLayout(driver);
  await layout.openFiles();
  await new FileManagerPage(driver).expectLoaded();
}
