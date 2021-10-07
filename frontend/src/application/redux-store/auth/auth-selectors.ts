import { useCallback } from 'react';
import { GlobalState } from '../types';
import { useAppSelector } from '../root-store';

export function useUserId(): string | undefined {
  return useAppSelector(
    useCallback(
      (x) => (x.auth.type === 'authorized' ? x.auth.userId : undefined),
      [],
    ),
  );
}
export function useIsAuthorized(): boolean {
  return useAppSelector(getIsAuthorized);
}
export const getIsAuthorized = (state: GlobalState) => {
  return state.auth.type === 'authorized';
};
export const getAccessToken = (state: GlobalState) => {
  return state.auth.type === 'authorized' ? state.auth.accessToken : undefined;
};
export const getRefreshToken = (state: GlobalState) => {
  return state.auth.type === 'authorized' ? state.auth.refreshToken : undefined;
};
