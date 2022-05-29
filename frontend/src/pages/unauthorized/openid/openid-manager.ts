import { User, UserManager, UserManagerSettings } from 'oidc-client-ts';
import {
  authCallbackPath,
  backendUri,
  clientId,
  redirectUri,
  scopes,
} from './openid-settings';
import Logger from 'js-logger';

export type SuccessfulRedirectHandler = (user: User) => void;
let _successfulRedirectHandler: SuccessfulRedirectHandler;
export function setSuccessfulRedirectHandler(
  handler: SuccessfulRedirectHandler,
) {
  _successfulRedirectHandler = handler;
}

function getClientSettings(): UserManagerSettings {
  return {
    authority: backendUri,
    client_id: clientId,
    redirect_uri: redirectUri,
    post_logout_redirect_uri: backendUri,
    response_type: 'code',
    filterProtocolClaims: true,
    loadUserInfo: false,
    scope: scopes,
    extraTokenParams: { scope: scopes },
  };
}
let manager: UserManager | undefined;
function getManager() {
  if (!manager) {
    manager = new UserManager(getClientSettings());
  }
  return manager;
}

export async function openExternalLoginPopup(provider: string) {
  try {
    const user = await getManager().signinPopup({
      extraQueryParams: { provider: provider, popup: true },
    } as any);
    return user;
  } catch (e) {
    console.error('Error during external authentication', e);
  }
}

export async function redirectToLoginPage() {
  try {
    const result = await getManager().signinRedirect();
    console.log(result);
  } catch (e) {
    console.error('Error during redirect to authentication', e);
  }
}

export function handleAuthenticationSignInCallback() {
  const url = window.location.pathname;
  const isOpenIdCallback = url.startsWith(authCallbackPath);
  if (isOpenIdCallback) {
    if (window.location.search.includes('popup')) {
      completeAuthorizationPopup().catch((e) => console.error(e));
    } else {
      completeAuthorizationRedirect()
        .then(_successfulRedirectHandler)
        .catch((e) => console.error(e));

      Logger.info('Logged in successfully');
    }
    return true;
  }
  return false;
}

export async function completeAuthorizationPopup() {
  const user = new UserManager({
    redirect_uri: redirectUri,
    client_id: clientId,
    authority: backendUri,
  }).signinPopupCallback(window.location.href);
  return user;
}

export async function completeAuthorizationRedirect() {
  const user = new UserManager({
    redirect_uri: redirectUri,
    client_id: clientId,
    authority: backendUri,
  }).signinRedirectCallback(window.location.href);
  return user;
}
