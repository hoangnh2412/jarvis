import type { IBrowserDriver } from '../ports/IBrowserDriver';

export abstract class Page {
  constructor(protected readonly driver: IBrowserDriver) {}
}
