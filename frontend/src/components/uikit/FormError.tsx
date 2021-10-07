import React from 'react';
const styles = require('./Field.module.scss');

export const FormError: React.FC = (props) => {
  if (!props.children) return null;
  return <div className={styles.error}>{props.children}</div>;
};
