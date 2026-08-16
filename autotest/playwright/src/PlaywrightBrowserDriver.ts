import type { Page } from '@playwright/test';
import type { IBrowserDriver } from '@jarvis/autotest';

export class PlaywrightBrowserDriver implements IBrowserDriver {
  constructor(private readonly page: Page) {}

  async goto(url: string): Promise<void> {
    await this.page.goto(url);
  }

  async click(selector: string): Promise<void> {
    await this.page.locator(selector).click();
  }

  async fill(selector: string, value: string): Promise<void> {
    await this.page.locator(selector).fill(value);
  }

  async clickRole(role: string, name: string): Promise<void> {
    await this.page.getByRole(role as 'button', { name }).click();
  }

  async clickText(text: string): Promise<void> {
    await this.page.getByText(text, { exact: true }).click();
  }

  async waitForUrl(substring: string): Promise<void> {
    await this.page.waitForURL((url) => url.toString().includes(substring));
  }

  async waitForPath(pathname: string): Promise<void> {
    await this.page.waitForURL((url) => {
      const path = url.pathname.replace(/\/$/, '') || '/';
      const expected = pathname.replace(/\/$/, '') || '/';
      return path === expected;
    });
  }

  async expectVisibleText(text: string): Promise<void> {
    await this.page.getByText(text).first().waitFor({ state: 'visible' });
  }

  url(): string {
    return this.page.url();
  }
}

export function createPlaywrightBrowserDriver(page: Page): PlaywrightBrowserDriver {
  return new PlaywrightBrowserDriver(page);
}
