export interface WorkflowContext {
  transId?: string;
  [key: string]: unknown;
}

export interface Workflow<TResult> {
  run(context: WorkflowContext): Promise<TResult>;
}
