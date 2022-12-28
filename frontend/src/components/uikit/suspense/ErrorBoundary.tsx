import {
  convertToErrorString,
  NetworkError,
} from 'helpers/form/useErrorHandler';
import { useTranslation } from 'react-i18next';
import { useEffect, useState } from 'react';
import { useLocation } from 'react-router';
import { Button } from '../buttons/Button';
import React from 'react';

import styles from './Loading.module.scss';
import { QueryFactory } from 'services/api';

type ErrorBoundaryFallbackProps = {
  error: Error;
  componentStack: string | null;
  eventId: string | null;
  resetError(): void;
};
export const errorBoundaryFallbackFunction = (
  props: ErrorBoundaryFallbackProps,
) => <ErrorBoundaryFallback {...props} />;
export const ErrorBoundaryFallback = (props: ErrorBoundaryFallbackProps) => {
  const location = useLocation();
  const i18n = useTranslation();
  const [initialLocation] = useState(location);
  useEffect(() => {
    if (initialLocation !== location) {
      props.resetError();
    }
  }, [initialLocation, location]);

  const errorString = convertToErrorString(props.error);
  const versionQuery = QueryFactory.VersionQuery.useVersionQuery();
  const isServerAvailable = !!versionQuery.data;

  // if error is NetworkError, but server is available, there's a high chance we are trying to request an old chunk of JS or CSS
  // (unfortunately it's impossible to get the error code of Network response and compare it with 404).
  // so we show proper error message and offer to reload the whole page
  const isServerUpdated = errorString === NetworkError && isServerAvailable;

  return (
    <div className={styles.flexContainer}>
      <div className={styles.flexLoadingData} data-test-id="loading-error">
        <div className={styles.loading}>
          <h1>{convertToErrorString(props.error)}</h1>
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
