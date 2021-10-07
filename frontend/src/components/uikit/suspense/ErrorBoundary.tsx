import { convertToErrorString } from 'helpers/form/useErrorHandler';
import { useTranslation } from 'react-i18next';
import { useEffect, useState } from 'react';
import { useLocation } from 'react-router';
import { Button } from '../buttons/Button';
import React from 'react';

const styles = require('./Loading.module.scss');

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

  return (
    <div className={styles.flexContainer}>
      <div className={styles.flexLoadingData} data-test-id="loading-error">
        <div className={styles.loading}>
          <h1>{convertToErrorString(props.error)}</h1>
          <div>
            <Button
              onClick={async () => {
                props.resetError();
              }}
              title={i18n.t('suspense.reload')}
            />
          </div>
        </div>
      </div>
    </div>
  );
};
