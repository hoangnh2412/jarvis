export interface HttpTransportResponse {
  ok(): boolean;
  status(): number;
  url(): string;
  text(): Promise<string>;
}

export interface HttpTransportOptions {
  headers?: Record<string, string>;
  data?: unknown;
}

export interface IHttpTransport {
  get(url: string, options?: HttpTransportOptions): Promise<HttpTransportResponse>;
  post(url: string, options?: HttpTransportOptions): Promise<HttpTransportResponse>;
}
