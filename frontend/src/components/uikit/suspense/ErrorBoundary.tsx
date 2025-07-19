import { errorToString, NetworkError } from 'helpers/error-helpers';
import { useTranslation } from 'react-i18next';
import { useEffect, useState } from 'react';
import { Button } from '../buttons/Button';

import styles from './Loading.module.scss';
import { QueryFactory } from 'services/api';
import { isAxiosError } from 'axios';
import {
  useQueryClient,
  useQueryErrorResetBoundary,
} from '@tanstack/react-query';

type ErrorBoundaryFallbackProps = {
  error: unknown;
  componentStack: string | null;
  eventId: string | null;
  resetError(): void;
};
export const errorBoundaryFallbackFunction = (
  props: ErrorBoundaryFallbackProps,
) => <ErrorBoundaryFallback {...props} />;
export const ErrorBoundaryFallback = (props: ErrorBoundaryFallbackProps) => {
  const location = window.location.href;
  const i18n = useTranslation();
  const queryClient = useQueryClient();
  const [initialLocation] = useState(window.location.href);
  useEffect(() => {
    if (initialLocation !== location) {
      props.resetError();
    }
  }, [initialLocation, location]);

  const errorString = errorToString(props.error);
  const versionQuery = QueryFactory.VersionQuery.useVersionQuery({
    throwOnError: false,
  });
  const isServerAvailable = !!versionQuery.data;

  // if error is NetworkError, but server is available, there's a high chance we are trying to request an old chunk of JS or CSS
  // (unfortunately it's impossible to get the error code of Network response and compare it with 404).
  // so we show proper error message and offer to reload the whole page
  const isServerUpdated =
    errorString === NetworkError &&
    !isAxiosError(props.error) &&
    isServerAvailable;
  const queryErrorReset = useQueryErrorResetBoundary();

  return (
    <div className={styles.flexContainer}>
      <div className={styles.flexLoadingData} data-test-id="loading-error">
        <div className={styles.loading}>
          <h1>{errorString}</h1>
          {isServerUpdated ? (
            <>
              <div className={styles.serverUpdated}>
                {i18n.t('suspense.server_updated')}
              </div>
              <Button
                onClick={async () => {
                  document.location.reload();
                }}
                title={i18n.t('suspense.reload')}
              />
            </>
          ) : (
            <div>
              <Button
                onClick={async () => {
                  void queryClient.refetchQueries({
                    predicate: (x) => !!x.state.error,
                  });
                  queryErrorReset.reset();
                  props.resetError();
                }}
                title={i18n.t('suspense.reload')}
              />
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
