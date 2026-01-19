import { QueryClient } from '@tanstack/react-query';
import axios from 'axios';
import { QueryFactory } from 'services/api';
import equal from 'fast-deep-equal';
import { addLogoutHandler } from 'helpers/auth/auth-handlers';

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      structuralSharing: (oldData: any, newData) =>
        equal(oldData, newData) ? oldData : newData,
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
configureDefaultQueryOptions();

addLogoutHandler(() => {
  // this is to make `useIsAuthorized` hook rerender, return false, and log the user out
  queryClient.setQueryData(
    QueryFactory.UserQuery.getCurrentUserInfoQueryKey(),
    null,
  );
});

export async function invlidateAuthQuery() {
  await queryClient.invalidateQueries({
    queryKey: QueryFactory.UserQuery.getCurrentUserInfoQueryKey(),
  });
}

function configureDefaultQueryOptions() {
  QueryFactory.UserQuery.setGetCurrentUserInfoDefaultOptions({
    staleTime: 60 * 1000, // 1 minute
  });
}
