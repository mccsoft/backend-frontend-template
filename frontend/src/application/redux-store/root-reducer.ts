import { combineReducers } from 'redux';
import { GlobalState } from './types';
import { AuthActions, authSlice } from './auth/auth-reducer';
import { CombinedState, Reducer } from '@reduxjs/toolkit';

const combinedReducer: Reducer<CombinedState<GlobalState>> = combineReducers({
  auth: authSlice.reducer,
});

export const rootReducer: Reducer<CombinedState<GlobalState>> = (
  state,
  action,
) => {
  if (action.type === AuthActions.logoutAction.type) {
    _logoutHandler();
    state = undefined;
  }

  return combinedReducer(state, action);
};
let _logoutHandler = () => {
  /* no action by default */
};

export function addLogoutHandler(handler: () => void) {
  const oldLogoutHandler = _logoutHandler;
  _logoutHandler = () => {
    oldLogoutHandler();
    handler();
  };
}
