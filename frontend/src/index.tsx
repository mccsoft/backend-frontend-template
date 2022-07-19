import React from 'react';
import ReactDOM from 'react-dom/client';
import './index.scss';
import { App } from './App';
import reportWebVitals from './reportWebVitals';
import { formatISO } from 'date-fns';
import { OpenIdCallback } from './pages/unauthorized/openid/OpenIdCallback';
import {
  postServerLogOut,
  setAuthData,
} from './helpers/interceptors/auth/auth-interceptor';
import { backendUri } from './pages/unauthorized/openid/openid-settings';
import { Loading } from './components/uikit/suspense/Loading';

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
    <OpenIdCallback
      signInRedirectHandler={(user) => {
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
    >
      <App />
    </OpenIdCallback>
  </React.StrictMode>,
);

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals();
