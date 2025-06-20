import { matchQuery, QueryClient } from '@tanstack/react-query';
import axios from 'axios';
import { addLogoutHandler } from './auth/auth-interceptor';
import { QueryFactory } from 'services/api';

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      throwOnError: true,
      retry(failureCount, error) {
        if (failureCount >= 3) return false;
        if (axios.isAxiosError(error) && error.response?.status === 401) {
          return false;
        }
        return true;
      },
    },
  },
});

addLogoutHandler(() => {
  // this is to make `useIsAuthorized` hook rerender, return false, and log the user out
  queryClient.setQueryData(
    QueryFactory.UserQuery.getCurrentUserInfoQueryKey(),
    null,
  );
});
