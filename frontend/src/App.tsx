import { sentryDsn } from 'application/constants/env-variables';
import { Loading } from 'components/uikit/suspense/Loading';
import React, { useMemo } from 'react';
import { Provider } from 'react-redux';
import { LanguageProvider } from './application/localization/LanguageProvider';
import { QueryClient, QueryClientProvider } from 'react-query';
import { PersistGate } from 'redux-persist/integration/react';
import { AppRouter } from 'navigation/router';
import axios from 'axios';
import { QueryFactory } from './services/api';
import * as Sentry from '@sentry/react';
import { Integrations } from '@sentry/tracing';
import { Suspense } from 'react';
import { RootStore } from './application/redux-store';
import { sessionAxiosInterceptor } from './helpers/interceptors/inject-session-interceptor';
import { injectLanguageInterceptor } from './helpers/interceptors/inject-language-interceptor';
import {
  addLogoutHandler,
  setupAuthInterceptor,
} from './helpers/interceptors/auth/auth-interceptor';
import { sendRefreshTokenRequest } from './helpers/interceptors/auth/auth-client';
import { logoutAction } from './application/redux-store/root-reducer';
import { OpenIdCallback } from './pages/unauthorized/openid/OpenIdCallback';

QueryFactory.setAxiosFactory(() => axios);

setupAuthInterceptor(
  axios,
  async (authData) => {
    const result = await sendRefreshTokenRequest(authData.refresh_token);
    return result;
  },
  () => {
    RootStore.store.dispatch(logoutAction);
  },
);
axios.interceptors.request.use(injectLanguageInterceptor);
axios.interceptors.request.use(sessionAxiosInterceptor);

if (import.meta.env.PROD) {
  Sentry.init({
    dsn: sentryDsn(),
    integrations: [new Integrations.BrowserTracing()],
    tracesSampleRate: 1.0,
  });
}

export const App = () => {
  const queryClient = useMemo(() => {
    const queryClient = new QueryClient({
      defaultOptions: {
        queries: {
          refetchOnWindowFocus: false,
          useErrorBoundary: true,
          suspense: false,
        },
      },
    });
    addLogoutHandler(() => {
      queryClient.clear();
    });
    return queryClient;
  }, []);

  const fallback = useMemo(() => {
    return <Loading loading={true} />;
  }, []);

  return (
    <Suspense fallback={fallback}>
      <QueryClientProvider client={queryClient}>
        <Provider store={RootStore.store}>
          <PersistGate loading={fallback} persistor={RootStore.persistor}>
            <LanguageProvider>
              <AppRouter />
            </LanguageProvider>
          </PersistGate>
        </Provider>
      </QueryClientProvider>
    </Suspense>
  );
};
