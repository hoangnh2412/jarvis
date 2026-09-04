import type { IBrowserDriver } from '../ports/IBrowserDriver';

export class BrowserManager {
  constructor(private readonly driver: IBrowserDriver) {}

  goto(url: string): Promise<void> {
    return this.driver.goto(url);
  }

  url(): string {
    return this.driver.url();
  }
}
