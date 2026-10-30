import type { HttpTransportResponse, IHttpTransport } from '../ports/IHttpTransport';

export type HttpMethod = 'GET' | 'HEAD' | 'POST' | 'PUT' | 'PATCH' | 'DELETE';

export interface RequestOptions {
  params?: Record<string, string | number | boolean | undefined>;
  body?: unknown;
  headers?: Record<string, string>;
}

/** Phản hồi HTTP không ném lỗi — dùng khi cần assert status (404, 403…). */
export interface RawApiResponse<TBody = unknown> {
  ok: boolean;
  status: number;
  url: string;
  text: string;
  body: TBody;
}

export class ApiClientError extends Error {
  constructor(
    public readonly status: number,
    public readonly body: string,
    public readonly url: string,
    public readonly method?: HttpMethod,
  ) {
    const preview = body.length > 500 ? `${body.slice(0, 500)}…` : body;
    const verb = method ? `${method} ` : '';
    super(`API ${verb}${url} trả về HTTP ${status}: ${preview}`);
    this.name = 'ApiClientError';
  }
}

function normalizeBaseUrl(baseUrl: string): string {
  const trimmed = baseUrl.trim();
  if (!trimmed) {
    throw new Error('Thiếu cấu hình baseUrl');
  }
  try {
    new URL(trimmed);
  } catch {
    throw new Error(`baseUrl không hợp lệ: ${trimmed}`);
  }
  return trimmed.replace(/\/+$/, '');
}

export class ApiClient {
  protected readonly transport: IHttpTransport;
  protected readonly baseUrl: string;
  protected readonly authHeaders: Record<string, string>;

  constructor(transport: IHttpTransport, baseUrl: string, authHeaders: Record<string, string> = {}) {
    this.transport = transport;
    this.baseUrl = normalizeBaseUrl(baseUrl);
    this.authHeaders = { ...authHeaders };
  }

  /** Gọi API và trả raw status + body (JSON parse best-effort). Không ném khi HTTP lỗi. */
  async requestRaw<TBody = unknown>(
    method: HttpMethod,
    path: string,
    options: RequestOptions = {},
  ): Promise<RawApiResponse<TBody>> {
    const response = await this.dispatch(method, path, options);
    const url = response.url();
    const text = await response.text();
    let body: TBody;
    if (!text) {
      body = null as TBody;
    } else {
      try {
        body = JSON.parse(text) as TBody;
      } catch {
        body = text as TBody;
      }
    }
    return {
      ok: response.ok(),
      status: response.status(),
      url,
      text,
      body,
    };
  }

  protected async get<T>(path: string, options: RequestOptions = {}): Promise<T> {
    return this.parseJson<T>(await this.dispatch('GET', path, options));
  }

  protected async head(path: string, options: RequestOptions = {}): Promise<void> {
    await this.parseJson<void>(await this.dispatch('HEAD', path, options));
  }

  protected async post<T>(path: string, options: RequestOptions = {}): Promise<T> {
    return this.parseJson<T>(await this.dispatch('POST', path, options));
  }

  protected async put<T>(path: string, options: RequestOptions = {}): Promise<T> {
    return this.parseJson<T>(await this.dispatch('PUT', path, options));
  }

  protected async patch<T>(path: string, options: RequestOptions = {}): Promise<T> {
    return this.parseJson<T>(await this.dispatch('PATCH', path, options));
  }

  protected async delete<T>(path: string, options: RequestOptions = {}): Promise<T> {
    return this.parseJson<T>(await this.dispatch('DELETE', path, options));
  }

  protected buildUrl(path: string, params?: RequestOptions['params']): string {
    const normalizedPath = path.startsWith('/') ? path : `/${path}`;
    const url = new URL(`${this.baseUrl}${normalizedPath}`);
    if (params) {
      for (const [key, value] of Object.entries(params)) {
        if (value === undefined) {
          continue;
        }
        url.searchParams.set(key, String(value));
      }
    }
    return url.toString();
  }

  private buildHeaders(method: HttpMethod, extra?: Record<string, string>): Record<string, string> {
    const headers: Record<string, string> = { ...this.authHeaders, ...extra };
    if (method !== 'GET' && method !== 'HEAD' && !headers['Content-Type'] && !headers['content-type']) {
      headers['Content-Type'] = 'application/json';
    }
    return headers;
  }

  private async dispatch(
    method: HttpMethod,
    path: string,
    options: RequestOptions,
  ): Promise<HttpTransportResponse> {
    const url = this.buildUrl(path, options.params);
    const headers = this.buildHeaders(method, options.headers);
    const hasBody = options.body !== undefined;
    const transportOpts = hasBody ? { headers, data: options.body } : { headers };

    switch (method) {
      case 'GET':
        return this.transport.get(url, transportOpts);
      case 'HEAD':
        return this.transport.head(url, transportOpts);
      case 'POST':
        return this.transport.post(url, transportOpts);
      case 'PUT':
        return this.transport.put(url, transportOpts);
      case 'PATCH':
        return this.transport.patch(url, transportOpts);
      case 'DELETE':
        return this.transport.delete(url, transportOpts);
      default: {
        const exhaustive: never = method;
        throw new Error(`HTTP method không hỗ trợ: ${exhaustive}`);
      }
    }
  }

  private async parseJson<T>(response: HttpTransportResponse): Promise<T> {
    const url = response.url();
    const text = await response.text();
    if (!response.ok()) {
      throw new ApiClientError(response.status(), text, url);
    }
    if (!text) {
      return null as T;
    }
    try {
      return JSON.parse(text) as T;
    } catch {
      throw new ApiClientError(
        response.status(),
        `Response không phải JSON: ${text.slice(0, 200)}`,
        url,
      );
    }
  }
}
