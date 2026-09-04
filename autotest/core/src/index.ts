export { type TestContext, createTestContext } from './core/TestContext';
export { type Configuration, loadConfiguration } from './core/Configuration';
export { createTestContextWithDefaults } from './core/createTestContext';

export { type Workflow, type WorkflowContext } from './workflow/Workflow';

export { ApiClient, ApiClientError, type RequestOptions } from './api/ApiClient';

export { type AuthStrategy } from './authentication/AuthStrategy';
export { ApiKeyAuth } from './authentication/ApiKeyAuth';
export { type Logger } from './logging/Logger';
export { ConsoleLogger } from './logging/ConsoleLogger';

export {
  createRunId,
  getRunId,
  getReportDir,
  ensureReportDir,
  writeRunMeta,
  getReportsRoot,
  type RunMeta,
} from './reporting/RunContext';

export { pollUntil, delay, type PollOptions } from './retry/pollUntil';

export { type IHttpTransport, type HttpTransportOptions, type HttpTransportResponse } from './ports/IHttpTransport';
export { type IBrowserDriver } from './ports/IBrowserDriver';

export { createTestHarness, type HarnessDependencies } from './composition/createTestHarness';

export { Page } from './ui/Page';
export { Component } from './ui/Component';
export { BrowserManager } from './ui/BrowserManager';

export { newUniqueId } from './test-data/IdGenerator';
