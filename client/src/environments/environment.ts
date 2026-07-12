// Production defaults. The app calls the API under the same origin at /api
// (e.g. behind a reverse proxy). Override apiBaseUrl for other deployments.
export const environment = {
  production: true,
  apiBaseUrl: '/api',
};
