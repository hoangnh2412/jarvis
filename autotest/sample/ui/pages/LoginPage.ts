import { Page } from '@jarvis/autotest';

export class LoginPage extends Page {
  async submit(webUrl: string, email: string, password: string): Promise<void> {
    const origin = webUrl.replace(/\/$/, '');
    await this.driver.goto(`${origin}/login`);
    await this.driver.fill('#login-email', email);
    await this.driver.fill('#login-password', password);
    await this.driver.clickRole('button', 'Đăng nhập');
    await this.driver.waitForPath('/');
  }
}
