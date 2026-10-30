export interface HttpTransportResponse {
  ok(): boolean;
  status(): number;
  url(): string;
  text(): Promise<string>;
  /** Response headers (lowercase keys) — optional để transport mỏng vẫn implement được. */
  headers?(): Record<string, string>;
}

export interface HttpTransportOptions {
  headers?: Record<string, string>;
  data?: unknown;
}

export interface IHttpTransport {
  get(url: string, options?: HttpTransportOptions): Promise<HttpTransportResponse>;
  head(url: string, options?: HttpTransportOptions): Promise<HttpTransportResponse>;
  post(url: string, options?: HttpTransportOptions): Promise<HttpTransportResponse>;
  put(url: string, options?: HttpTransportOptions): Promise<HttpTransportResponse>;
  patch(url: string, options?: HttpTransportOptions): Promise<HttpTransportResponse>;
  delete(url: string, options?: HttpTransportOptions): Promise<HttpTransportResponse>;
}
