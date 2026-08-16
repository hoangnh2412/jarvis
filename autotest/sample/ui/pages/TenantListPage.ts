import { Page } from '@jarvis/autotest';

export class TenantListPage extends Page {
  async expectLoaded(): Promise<void> {
    await this.driver.expectVisibleText('Danh sách tenant');
  }
}
