import * as React from 'react';
import { useNavigate, useLocation, To } from 'react-router-dom';

type Location = To & {
  state: any;
};

/**
 * This is the main thing you need to use to adapt the react-router v6
 * API to what use-query-params expects.
 *
 * Pass this as the `ReactRouterRoute` prop to QueryParamProvider.
 */
export const RouteAdapter: React.FC<{ children?: React.ReactNode }> = ({
  children,
}) => {
  const navigate = useNavigate();
  const location = useLocation();

  const adaptedHistory = React.useMemo(
    () => ({
      replace({ search, state }: Location) {
        navigate(
          { search: search as string },
          {
            replace: true,
            state: state,
          },
        );
      },
      push({ search, state }: Location) {
        navigate(
          { search: search as string },
          { replace: false, state: state },
        );
      },
    }),
    [navigate],
  );
  // eslint-disable-next-line @typescript-eslint/ban-ts-comment
  // @ts-ignore
  return children({ history: adaptedHistory, location });
};
