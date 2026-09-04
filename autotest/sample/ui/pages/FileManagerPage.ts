import { Page } from '@jarvis/autotest';

export class FileManagerPage extends Page {
  async expectLoaded(): Promise<void> {
    await this.driver.expectVisibleText('Upload files');
  }
}
