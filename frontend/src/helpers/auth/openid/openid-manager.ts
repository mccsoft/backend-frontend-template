import { User, UserManager, UserManagerSettings } from 'oidc-client-ts';
import {
  backendUri,
  clientId,
  signInRedirectUri,
  scopes,
  signOutRedirectUri,
  signOutRedirectUriPopup,
} from './openid-settings';
import Logger from 'js-logger';
import { UseCookieAuth } from '../auth-settings';
import { invlidateAuthQuery } from 'services/api/query-client-helper';

export type SignInRedirectHandler = (user: User) => void;
export type SignOutRedirectHandler = () => void;

function getClientSettings(): UserManagerSettings {
  return {
    authority: backendUri,
    client_id: clientId,
    redirect_uri: signInRedirectUri,
    popup_redirect_uri: signInRedirectUri,
    post_logout_redirect_uri: signOutRedirectUri,
    popup_post_logout_redirect_uri: signOutRedirectUriPopup,
    response_type: 'code',
    filterProtocolClaims: true,
    loadUserInfo: false,
    scope: scopes,
    automaticSilentRenew: false,
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
    if (UseCookieAuth) {
      await invlidateAuthQuery();
    }
    return user;
  } catch (e) {
    Logger.error('Error during external authentication', e);
  }
}
export async function openExternalLoginRedirect(provider: string) {
  try {
    const user = await getManager().signinRedirect({
      extraQueryParams: { provider: provider },
    } as any);
    return user;
  } catch (e) {
    Logger.error('Error during external authentication', e);
  }
}

export async function redirectToLoginPage() {
  try {
    Logger.info('Redirecting to login page');
    await getManager().signinRedirect();
  } catch (e) {
    Logger.error('Error during redirect to authentication', e);
  }
}

export function handleAuthenticationSignInCallback(
  successCallback: (user: User) => void,
  errorCallback?: (e: unknown) => void,
) {
  if (window.location.search.includes('popup')) {
    completeAuthorizationPopup().catch((e) => Logger.error(e));
  } else {
    completeAuthorizationRedirect()
      .then((user) => {
        Logger.info('Logged in successfully');
        successCallback(user);
      })
      .catch((e) => {
        Logger.error('Error in completeAuthorizationRedirect', e);
        errorCallback?.(e);
      });
  }
  return true;
}

export async function completeAuthorizationPopup() {
  const user = getManager().signinPopupCallback(window.location.href);
  return user;
}

export async function completeAuthorizationRedirect() {
  const user = getManager().signinRedirectCallback(window.location.href);
  return user;
}

export function handleAuthenticationSignOutCallback(
  signOutCallback: () => void,
) {
  if (window.location.search.includes('popup')) {
    getManager()
      .signoutPopupCallback()
      .catch((e) => console.error(e));
  } else {
    getManager()
      .signoutRedirectCallback()
      .then((user) => {
        Logger.info('Signed out successfully');
        signOutCallback();
      })
      .catch((e) => {
        Logger.error('Error in handleAuthenticationLogoutCallback', e);
      });
  }
}

export async function signOutRedirect() {
  await getManager().signoutRedirect();
}

export async function signOutPopup() {
  await getManager().signoutPopup({
    extraQueryParams: { popup: true },
  });
}
