import React from 'react';
import styles from './LoginErrorPage.module.scss';

export const LoginErrorPage: React.FC<{
  error: string;
  error_description: string;
}> = (props) => {
  return (
    <div className={styles.container}>
      <div>
        <div>{`ERROR: ${props.error}.`}</div>
        <div>{props.error_description}</div>
      </div>
    </div>
  );
};
