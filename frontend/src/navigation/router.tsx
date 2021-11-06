import { appVersion } from 'application/constants/env-variables';
import { useIsAuthorized } from 'application/redux-store/auth/auth-selectors';
import { QuerySuspenseErrorWrapper } from 'helpers/retry-helper';
import { RootPage } from 'pages/authorized/RootPage';
import { LoginPage } from 'pages/unauthorized/login/LoginPage';
import React from 'react';
import { BrowserRouter, Route, Routes } from 'react-router-dom';
import { QueryParamProvider } from 'use-query-params';
import { RouteAdapter } from './RouteAdapter';

export const AppRouter = () => {
  const isAuth = useIsAuthorized();

  return (
    <BrowserRouter>
      <QueryParamProvider ReactRouterRoute={RouteAdapter}>
        <QuerySuspenseErrorWrapper>
          {isAuth ? (
            <RootPage />
          ) : (
            <>
              <Routes>
                <Route element={<LoginPage />} />
              </Routes>
              <div>Version: {appVersion()}</div>
            </>
          )}
        </QuerySuspenseErrorWrapper>
      </QueryParamProvider>
    </BrowserRouter>
  );
};
