import * as Sentry from '@sentry/react';
import {
  ErrorBoundaryFallback,
  errorBoundaryFallbackFunction,
} from 'components/uikit/suspense/ErrorBoundary';
import { Loading } from 'components/uikit/suspense/Loading';

import * as React from 'react';
import {
  ComponentType,
  lazy,
  PropsWithChildren,
  Suspense,
  useCallback,
  useMemo,
  useState,
} from 'react';
import { QueryErrorResetBoundary } from '@tanstack/react-query';

/**
 * Helper which retries to run a given async function if it is failed.
 */
export function retry<T>(
  fn: () => Promise<T>,
  retriesLeft = 2,
  intervalInMillis = 1000,
): Promise<T> {
  return new Promise((resolve, reject) => {
    fn()
      .then(resolve)
      .catch((reason) => {
        if (retriesLeft === 0) {
          reject(reason);
          return;
        }

        setTimeout(() => {
          retry<T>(fn, retriesLeft - 1, intervalInMillis).then(resolve, reject);
        }, intervalInMillis);
      });
  });
}

/**
 * Helper which retries to load a lazy component if it is failed.
 * Wrapped in ErrorBoundary to show an error and reload if something bad happens
 */
export function lazyRetry<T extends ComponentType<any>>(
  factory: () => Promise<{ default: T }>,
): T {
  const RetryWrapper = (props: any) => {
    const [loading, setLoading] = useState(true);
    const resetError = useCallback(() => setLoading(true), []);
    const LazyComponent = useMemo(
      () =>
        lazy(() =>
          factory().catch((e) => {
            setLoading(false);
            return {
              default: function Fallback() {
                return (
                  <ErrorBoundaryFallback
                    resetError={resetError}
                    error={e}
                    componentStack={null}
                    eventId={null}
                  />
                );
              },
            } as any;
          }),
        ),
      [factory, loading],
    );
    return (
      <QuerySuspenseErrorWrapper>
        <LazyComponent {...props} />
      </QuerySuspenseErrorWrapper>
    );
  };

  return RetryWrapper as any;
}

export const QuerySuspenseErrorWrapper: React.FC<
  PropsWithChildren<{ reset?: () => void }>
> = (props) => {
  const result: any = (
    <QueryErrorResetBoundary>
      {({ reset }) => (
        <Sentry.ErrorBoundary
          fallback={errorBoundaryFallbackFunction}
          onReset={() => {
            reset();
            props.reset?.();
          }}
        >
          <Suspense fallback={<Loading loading={true} />}>
            {props.children}
          </Suspense>
        </Sentry.ErrorBoundary>
      )}
    </QueryErrorResetBoundary>
  );
  return result;
};
