import { QuerySuspenseErrorWrapper } from 'helpers/retry-helper';
import { RootPage } from 'pages/authorized/RootPage';
import { LoginPage } from 'pages/unauthorized/LoginPage';
import React from 'react';
import { BrowserRouter, Route, Routes } from 'react-router-dom';
import { QueryParamProvider } from 'use-query-params';
import { RouteAdapter } from 'helpers/RouteAdapter';
import { useIsAuthorized } from 'helpers/interceptors/auth/auth-interceptor';

export const AppRouter = () => {
  const isAuth = useIsAuthorized();

  return (
    <BrowserRouter>
      <QueryParamProvider ReactRouterRoute={RouteAdapter}>
        <QuerySuspenseErrorWrapper>
          {!!isAuth ? (
            <RootPage />
          ) : (
            <Routes>
              <Route path={'*'} element={<LoginPage />} />
            </Routes>
          )}
        </QuerySuspenseErrorWrapper>
      </QueryParamProvider>
    </BrowserRouter>
  );
};
