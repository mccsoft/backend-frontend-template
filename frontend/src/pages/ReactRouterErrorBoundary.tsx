import { useRouteError } from 'react-router';

/*
 * This is an ErrorBoundary that should be provided to `createBrowserRouter`.
 * It just rethrows an error so that it will be cought by `QuerySuspenseErrorWrapper`.
 * We could include all `QuerySuspenseErrorWrapper` error-handling logic into `ReactRouterErrorBoundary`,
 * but we'd still need to wrap all routes in <Suspense> and <QueryErrorResetBoundary>.
 */
export const ReactRouterErrorBoundary = () => {
  const error = useRouteError() as Error;
  throw error;
};
