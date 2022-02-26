import { Log, UserManager, UserManagerSettings } from 'oidc-client';

export const authCallbackPath = '/auth/openid-callback';

function getClientSettings(): UserManagerSettings {
  const backendUri = `${window.location.protocol}//${window.location.hostname}:${window.location.port}`;
  Log.level = 4;
  Log.logger = console;
  return {
    authority: backendUri,
    client_id: 'web_client',
    redirect_uri: `${backendUri}${authCallbackPath}`,
    post_logout_redirect_uri: backendUri,
    response_type: 'code',
    scope: 'openid offline_access',
    filterProtocolClaims: true,
    loadUserInfo: true,
  };
}
const manager = new UserManager(getClientSettings());
function getManager() {
  return manager;
}

export async function openExternalLoginPopup(provider: string) {
  console.log('tt', getManager().settings);
  const user = await getManager().signinPopup({
    scope: 'offline_access',
    extraQueryParams: { provider: provider, scope: 'offline_access' },
  });
  console.log('iiiiiiiii', user);
  return user;
}
export async function completeAuthorization() {
  console.log('yyy');
  console.log('tt', getManager().settings);
  const user = await getManager().signinPopupCallback(window.location.href);
}
