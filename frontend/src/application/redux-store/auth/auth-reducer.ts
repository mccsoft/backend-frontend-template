import { AuthState, TokenData } from './auth-types';
import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { FetchLoginResponse } from 'services/auth-client';

export const authSlice = createSlice({
  name: 'auth',
  initialState: {
    type: 'unauthorized',
  } as AuthState,
  reducers: {
    loginAction: (state, action: PayloadAction<FetchLoginResponse>) => {
      const payload = action.payload;
      const tokenData: TokenData = {
        accessToken: payload.access_token,
        refreshToken: payload.refresh_token,
        expiresIn: payload.expires_in,
        acquiredAt: new Date(),
        userId: payload.claims.id,
      };
      return {
        type: 'authorized',
        ...tokenData,
      };
    },
    logoutAction: (_state, _action: PayloadAction<void>) => {
      return {
        type: 'unauthorized',
      };
    },
  },
});

export const AuthActions = authSlice.actions;
