// Development: the dev server proxies /api to the .NET API (see proxy.conf.json),
// so requests are same-origin from the browser's perspective — no CORS needed.
export const environment = {
  production: false,
  apiBaseUrl: '/api',
};
