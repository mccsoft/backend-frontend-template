import React, { PropsWithChildren } from 'react';
import styles from './Field.module.scss';

export const FormError: React.FC<PropsWithChildren> = (props) => {
  if (!props.children) return null;
  return <div className={styles.error}>{props.children}</div>;
};
