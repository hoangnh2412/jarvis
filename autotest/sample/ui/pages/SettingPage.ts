import { Page } from '@jarvis/autotest';

export class SettingPage extends Page {
  async expectLoaded(): Promise<void> {
    await this.driver.expectVisibleText('Cấu hình hệ thống');
  }
}
