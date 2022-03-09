import { UserManager, UserManagerSettings } from 'oidc-client-ts';

export const authCallbackPath = '/auth/openid-callback';
const scopes = 'offline_access';

function getClientSettings(): UserManagerSettings {
  const backendUri = `${window.location.protocol}//${window.location.hostname}:${window.location.port}`;

  return {
    authority: backendUri,
    client_id: 'web_client',
    redirect_uri: `${backendUri}${authCallbackPath}`,
    post_logout_redirect_uri: backendUri,
    response_type: 'code',
    filterProtocolClaims: true,
    loadUserInfo: false,
    scope: scopes,
    extraTokenParams: { scope: scopes },
  };
}
const manager = new UserManager(getClientSettings());
function getManager() {
  return manager;
}

export async function openExternalLoginPopup(provider: string) {
  try {
    const user = await getManager().signinPopup({
      extraQueryParams: { provider: provider },
    } as any);
    return user;
  } catch (e) {}
}
export async function completeAuthorization() {
  const user = await getManager().signinPopupCallback(window.location.href);
  return user;
}
