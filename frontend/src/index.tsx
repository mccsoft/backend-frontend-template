import React from 'react';
import ReactDOM from 'react-dom/client';
import './index.scss';
import { App } from './App';
import reportWebVitals from './reportWebVitals';
import { formatISO } from 'date-fns';
import { OpenIdCallback } from 'helpers/auth/openid/OpenIdCallback';
import { postServerLogOut, setAuthData } from 'helpers/auth/auth-interceptor';
import { backendUri } from 'helpers/auth/openid/openid-settings';
import { Loading } from './components/uikit/suspense/Loading';
import { LoginErrorPage } from 'pages/unauthorized/LoginErrorPage';
import { QueryClientProvider } from '@tanstack/react-query';
import { queryClient } from 'services/api/query-client-helper';
import { UseCookieAuth } from 'helpers/auth/auth-settings';

//to send dates to backend in local timezone (not in UTC)
Date.prototype.toISOString = function () {
  const result = formatISO(this);
  return result;
};

const root = ReactDOM.createRoot(
  document.getElementById('root') as HTMLElement,
);
root.render(
  <React.StrictMode>
    <QueryClientProvider client={queryClient}>
      <OpenIdCallback
        signInRedirectHandler={(user) => {
          if (!UseCookieAuth)
            setAuthData({
              access_token: user.access_token,
              refresh_token: user.refresh_token!,
            });

          window.history.pushState(null, '', backendUri);
        }}
        signOutRedirectHandler={() => {
          postServerLogOut();
        }}
        loading={<Loading loading={true} />}
        error={LoginErrorPage}
      >
        <App />
      </OpenIdCallback>
    </QueryClientProvider>
  </React.StrictMode>,
);

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals();
