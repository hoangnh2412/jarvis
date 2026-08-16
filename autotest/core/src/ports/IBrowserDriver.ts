export interface IBrowserDriver {
  goto(url: string): Promise<void>;
  click(selector: string): Promise<void>;
  fill(selector: string, value: string): Promise<void>;
  clickRole(role: string, name: string): Promise<void>;
  clickText(text: string): Promise<void>;
  waitForUrl(substring: string): Promise<void>;
  waitForPath(pathname: string): Promise<void>;
  expectVisibleText(text: string): Promise<void>;
  url(): string;
}
