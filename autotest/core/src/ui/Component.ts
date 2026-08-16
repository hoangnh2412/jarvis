import type { IBrowserDriver } from '../ports/IBrowserDriver';

export abstract class Component {
  constructor(protected readonly driver: IBrowserDriver) {}
}
