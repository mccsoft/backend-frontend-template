import { combineReducers } from 'redux';
import { GlobalState } from './types';
import { themeSlice } from 'application/redux-store/theme/theme-slice';
import { CombinedState, createAction, Reducer } from '@reduxjs/toolkit';

const combinedReducer: Reducer<CombinedState<GlobalState>> = combineReducers({
  theme: themeSlice.reducer,
});

export const logoutAction = createAction('logout');

export const rootReducer: Reducer<CombinedState<GlobalState>> = (
  state,
  action,
) => {
  if (action.type === logoutAction.type) {
    state = undefined;
  }

  return combinedReducer(state, action);
};
