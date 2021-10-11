import React from 'react';
import ReactDOM from 'react-dom';
import 'index.scss';
import { App } from './App';
import reportWebVitals from './reportWebVitals';
import { formatISO } from 'date-fns';

//to send dates to backend in local timezone (not in UTC)
Date.prototype.toISOString = function () {
  const result = formatISO(this);
  return result;
};

ReactDOM.render(
  <React.StrictMode>
    <App />
  </React.StrictMode>,
  document.getElementById('root'),
);

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals();
