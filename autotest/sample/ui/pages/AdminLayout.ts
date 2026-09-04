import { Page } from '@jarvis/autotest';

export class AdminLayout extends Page {
  async openTenants(): Promise<void> {
    await this.driver.clickText('Tenant');
    await this.driver.waitForUrl('/tenants');
  }

  async openFiles(): Promise<void> {
    await this.driver.clickText('Files');
    await this.driver.waitForUrl('/files');
  }

  async openSettings(): Promise<void> {
    await this.driver.clickText('Cài đặt');
    await this.driver.waitForUrl('/settings');
  }
}
