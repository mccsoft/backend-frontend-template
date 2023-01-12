import { FeatureFlags, sentryDsn } from 'application/constants/env-variables';
import { Loading } from 'components/uikit/suspense/Loading';
import React, { Suspense, useMemo } from 'react';
import { Provider } from 'react-redux';
import { LanguageProvider } from './application/localization/LanguageProvider';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { PersistGate } from 'redux-persist/integration/react';
import { AppRouter } from 'pages/router';
import axios from 'axios';
import { QueryFactory } from './services/api';
import * as Sentry from '@sentry/react';
import { RootStore } from './application/redux-store';
import { sessionAxiosInterceptor } from './helpers/interceptors/inject-session-interceptor';
import { injectLanguageInterceptor } from './helpers/interceptors/inject-language-interceptor';
import {
  addLogoutHandler,
  setupAuthInterceptor,
} from './helpers/interceptors/auth/auth-interceptor';
import { sendRefreshTokenRequest } from './helpers/interceptors/auth/auth-client';
import { logoutAction } from './application/redux-store/root-reducer';
import { backendUri } from './pages/unauthorized/openid/openid-settings';
import { ThemeProvider } from '@mui/material';
import { createTheme } from '@mui/material/styles';
import { ModalProvider } from './components/uikit/modal/useModal';
import { MiniProfiler, miniProfilerInterceptor } from './helpers/MiniProfiler';

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
      <ThemeProvider theme={theme}>
        <QueryClientProvider client={queryClient}>
          <Provider store={RootStore.store}>
            <PersistGate loading={fallback} persistor={RootStore.persistor}>
              <LanguageProvider>
                <ModalProvider>
                  <AppRouter />
                  {FeatureFlags.isMiniProfilerEnabled() && <MiniProfiler />}
                </ModalProvider>
              </LanguageProvider>
            </PersistGate>
          </Provider>
        </QueryClientProvider>
      </ThemeProvider>
    </Suspense>
  );
};
