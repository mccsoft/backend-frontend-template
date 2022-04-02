export const authCallbackPath = '/auth/openid-callback';
export const scopes = 'offline_access';
export const backendUri = `${window.location.protocol}//${
  window.location.hostname
}${window.location.port ? `:${window.location.port}` : ''}`;
export const redirectUri = `${backendUri}${authCallbackPath}`;
export const clientId = 'web_client';
