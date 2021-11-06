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
export const RouteAdapter = ({ children }: { children?: any }) => {
  const navigate = useNavigate();
  const location = useLocation();

  const adaptedHistory = React.useMemo(
    () => ({
      replace(location: Location) {
        navigate(location, { replace: true, state: location.state });
      },
      push(location: Location) {
        navigate(location, { replace: false, state: location.state });
      },
    }),
    [navigate],
  );
  return children({ history: adaptedHistory, location });
};
