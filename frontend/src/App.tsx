import { FeatureFlags, sentryDsn } from 'application/constants/env-variables';
import { Loading } from 'components/uikit/suspense/Loading';
import React, { Suspense, useMemo } from 'react';
import { Provider } from 'react-redux';
import { LanguageProvider } from './application/localization/LanguageProvider';
import { PersistGate } from 'redux-persist/integration/react';
import { anonymousRoutes, authorizedRoutes } from 'pages/router';
import axios from 'axios';
import { QueryFactory } from './services/api';
import * as Sentry from '@sentry/react';
import { RootStore } from './application/redux-store';
import { sessionAxiosInterceptor } from './helpers/interceptors/inject-session-interceptor';
import { injectLanguageInterceptor } from './helpers/interceptors/inject-language-interceptor';
import {
  setupAuthInterceptor,
  useIsAuthorized,
} from 'helpers/auth/auth-interceptor';
import { sendRefreshTokenRequest } from 'helpers/auth/auth-client';
import { logoutAction } from './application/redux-store/root-reducer';
import { backendUri } from 'helpers/auth/openid/openid-settings';
import { ThemeProvider } from '@mui/material';
import { createTheme } from '@mui/material/styles';
import { ModalProvider } from './components/uikit/modal/useModal';
import { MiniProfiler, miniProfilerInterceptor } from './helpers/MiniProfiler';
import { QuerySuspenseErrorWrapper } from 'helpers/retry-helper';
import { RouterProvider } from 'react-router-dom';
import { blobResponseErrorInterceptor } from 'helpers/interceptors/blob-error-interceptor';
import { addLogoutHandler } from 'helpers/auth/auth-handlers';

QueryFactory.setAxiosFactory(() => axios);

setupAuthInterceptor(axios, async (authData) => {
  const result = await sendRefreshTokenRequest(authData.refresh_token);
  return result;
});
addLogoutHandler(() => {
  RootStore.store.dispatch(logoutAction);
  window.history.pushState(null, '', backendUri);
});
axios.interceptors.request.use(injectLanguageInterceptor);
axios.interceptors.request.use(sessionAxiosInterceptor);
axios.interceptors.response.use(null, blobResponseErrorInterceptor);
if (FeatureFlags.isMiniProfilerEnabled()) {
  axios.interceptors.response.use(miniProfilerInterceptor, undefined);
}

if (sentryDsn()) {
  Sentry.init({
    dsn: sentryDsn(),
    // integrations: [new Integrations.BrowserTracing()],
    // tracesSampleRate: 1.0,
  });
}

const theme = createTheme();

export const App = () => {
  const fallback = useMemo(() => {
    return <Loading loading={true} />;
  }, []);

  const isAuth = useIsAuthorized();
  if (isAuth === 'loading') return <Loading loading={true} />;

  return (
    <Suspense fallback={fallback}>
      <ThemeProvider theme={theme}>
        <Provider store={RootStore.store}>
          <PersistGate loading={fallback} persistor={RootStore.persistor}>
            <LanguageProvider>
              <ModalProvider>
                <QuerySuspenseErrorWrapper>
                  <RouterProvider
                    router={!!isAuth ? authorizedRoutes() : anonymousRoutes()}
                  />
                </QuerySuspenseErrorWrapper>

                {FeatureFlags.isMiniProfilerEnabled() && <MiniProfiler />}
              </ModalProvider>
            </LanguageProvider>
          </PersistGate>
        </Provider>
      </ThemeProvider>
    </Suspense>
  );
};
