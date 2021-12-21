import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { ThemeState } from './theme-types';

export const themeSlice = createSlice({
  name: 'theme',
  initialState: {
    theme: 'light',
  } as ThemeState,
  reducers: {
    setThemeAction: (
      state,
      action: PayloadAction<{ theme: 'light' | 'dark' }>,
    ) => {
      return {
        theme: action.payload.theme,
      };
    },
  },
});

export const ThemeActions = themeSlice.actions;
