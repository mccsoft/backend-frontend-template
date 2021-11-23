import { sentryDsn } from 'application/constants/env-variables';
import { Loading } from 'components/uikit/suspense/Loading';
import React, { useMemo } from 'react';
import { Provider } from 'react-redux';
import { LanguageProvider } from './application/localization/LanguageProvider';
import { QueryClient, QueryClientProvider } from 'react-query';
import { addLogoutHandler } from './application/redux-store/root-reducer';
import { PersistGate } from 'redux-persist/integration/react';
import { AppRouter } from 'navigation/router';
import axios from 'axios';
import * as Interceptors from './helpers/auth-interceptors';
import { setupRefreshTokenInterceptor } from './helpers/auth-interceptors';
import { QueryFactory } from './services/api';
import * as Sentry from '@sentry/react';
import { Integrations } from '@sentry/tracing';
import { Suspense } from 'react';
import { RootStore } from './application/redux-store';

QueryFactory.setAxiosFactory(() => axios);
axios.interceptors.request.use(
  Interceptors.injectTokenInterceptor(RootStore.store.getState),
);
setupRefreshTokenInterceptor(
  RootStore.store.getState,
  RootStore.store.dispatch,
);

if (process.env.NODE_ENV === 'production') {
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
