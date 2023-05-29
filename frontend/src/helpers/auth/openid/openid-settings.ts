export const signInCallbackPath = '/auth/openid-callback';
export const signOutCallbackPath = '/auth/signout-callback';
export const scopes = 'offline_access';
export const backendUri = `${window.location.protocol}//${
  window.location.hostname
}${window.location.port ? `:${window.location.port}` : ''}`;
export const signInRedirectUri = `${backendUri}${signInCallbackPath}`;
export const signInRedirectUriPopup = `${backendUri}${signInCallbackPath}?popup=1`;
export const signOutRedirectUri = `${backendUri}${signOutCallbackPath}`;
export const signOutRedirectUriPopup = `${backendUri}${signOutCallbackPath}?popup=1`;
export const clientId = 'web_client';
