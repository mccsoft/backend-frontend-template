import { appVersion } from 'application/constants/env-variables';
import { Routes } from 'application/constants/routes';
import { useIsAuthorized } from 'application/redux-store/auth/auth-selectors';
import { QuerySuspenseErrorWrapper } from 'helpers/retry-helper';
import { RootPage } from 'pages/authorized/RootPage';
import { LoginPage } from 'pages/unauthorized/login/LoginPage';
import React from 'react';
import { BrowserRouter, Route, Switch } from 'react-router-dom';
import { QueryParamProvider } from 'use-query-params';

export const AppRouter = () => {
  const isAuth = useIsAuthorized();

  return (
    <BrowserRouter>
      <QueryParamProvider ReactRouterRoute={Route}>
        <QuerySuspenseErrorWrapper>
          {isAuth ? (
            <RootPage />
          ) : (
            <>
              <Switch>
                <Route path={Routes.Unauthorized.Login} component={LoginPage} />
                <Route component={LoginPage} />
              </Switch>
              <div>Version: {appVersion()}</div>
            </>
          )}
        </QuerySuspenseErrorWrapper>
      </QueryParamProvider>
    </BrowserRouter>
  );
};
