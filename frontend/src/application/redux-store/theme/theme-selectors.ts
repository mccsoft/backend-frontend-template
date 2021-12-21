import { GlobalState } from '../types';

export const getThemeSelector = (state: GlobalState) => {
  return state.theme;
};
