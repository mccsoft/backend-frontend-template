import clsx from 'clsx';
import { errorToString } from 'helpers/error-helpers';
import React from 'react';
import styles from './Loading.module.scss';

import animationData from 'assets/animations/9629-loading.json';
import { Lottie } from '../../animations/Lottie';

type Props = {
  loading: boolean;
  title?: string;
  testId?: string;
  children?: React.ReactNode;
  className?: string;
  flex?: boolean;
  error?: any;
  wrapperClassName?: string;
  centerContent?: boolean;
};

export const Loading: React.FC<Props> = (props) => {
  const loadingStyles = [props.className];
  loadingStyles.push(styles.loadingContainer);

  return (
    <div
      className={clsx(
        props.flex ? styles.flexContainer : styles.container,
        props.centerContent !== false ? styles.centerContent : null,
        props.wrapperClassName,
      )}
      data-test-id={props.testId}
    >
      <div
        data-loaded={props.loading}
        className={props.flex ? styles.flexLoadingData : styles.loadingData}
      >
        {props.error ? (
          <h1 className={styles.loading} data-test-id="loading-error">
            {errorToString(props.error)}
          </h1>
        ) : (
          props.children
        )}
      </div>
      {props.loading && !props.error && (
        <div className={clsx(loadingStyles)} data-test-id="loading">
          <div className={styles.loading}>
            <div className={styles.row}>
              <Lottie animationData={animationData} className={styles.lottie} />
              {props.title && (
                <div className={styles.title}> {props.title}</div>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
