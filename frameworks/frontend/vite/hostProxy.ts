type ViteProxyOptions = Record<string, unknown>

/** Ba tiền tố ASP.NET. Host thêm route riêng cạnh bảng này, không truyền vào hàm. */
export function platformProxy(proxyUrl: string): Record<string, ViteProxyOptions> {
  return {
    '/_blazor': { target: proxyUrl, changeOrigin: true, secure: false, ws: true },
    '/_framework': { target: proxyUrl, changeOrigin: true, secure: false },
    '/_content': { target: proxyUrl, changeOrigin: true, secure: false },
  }
}
