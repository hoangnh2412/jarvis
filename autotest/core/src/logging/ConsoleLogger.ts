import type { Logger } from './Logger';

export class ConsoleLogger implements Logger {
  constructor(private readonly prefix = '') {}

  info(message: string, context?: Record<string, unknown>): void {
    this.log('INFO', message, context);
  }

  warn(message: string, context?: Record<string, unknown>): void {
    this.log('WARN', message, context);
  }

  error(message: string, context?: Record<string, unknown>): void {
    this.log('ERROR', message, context);
  }

  private log(level: string, message: string, context?: Record<string, unknown>): void {
    const tag = this.prefix ? `[${this.prefix}]` : '';
    const suffix = context ? ` ${JSON.stringify(context)}` : '';
    // eslint-disable-next-line no-console
    console.log(`${tag}[${level}] ${message}${suffix}`);
  }
}
