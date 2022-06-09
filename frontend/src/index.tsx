import React from 'react';
import ReactDOM from 'react-dom';
import './index.scss';
import { App } from './App';
import reportWebVitals from './reportWebVitals';
import { formatISO } from 'date-fns';
import { OpenIdCallback } from './pages/unauthorized/openid/OpenIdCallback';
import {
  postServerLogOut,
  setAuthData,
} from './helpers/interceptors/auth/auth-interceptor';

//to send dates to backend in local timezone (not in UTC)
Date.prototype.toISOString = function () {
  const result = formatISO(this);
  return result;
};

ReactDOM.render(
  <React.StrictMode>
    <OpenIdCallback
      signInRedirectHandler={(user) => {
        setAuthData({
          access_token: user.access_token,
          refresh_token: user.refresh_token!,
        });
        window.location.href =
          window.location.protocol + '//' + window.location.host;
      }}
      signOutRedirectHandler={() => {
        /*
         * This is only called if REDIRECT is used during sign out.
         * Normally we use popup.
         */
        postServerLogOut();
      }}
    >
      <App />
    </OpenIdCallback>
  </React.StrictMode>,
  document.getElementById('root'),
);

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals();
