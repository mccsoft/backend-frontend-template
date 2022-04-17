import * as React from 'react';

import styles from './CheckBox.module.scss';

type Props = React.InputHTMLAttributes<HTMLInputElement> & {
  title?: string;
  testId?: string;
};

export const CheckBox = React.forwardRef<HTMLInputElement, Props>(
  function CheckBox(props, ref) {
    const { title, testId, ...rest } = props;
    const id =
      props.name != undefined ? `uni-check-box-${props.name}` : undefined;
    return (
      <div className={props.className}>
        <input
          ref={ref}
          {...rest}
          className={styles.customCheckbox}
          type="checkbox"
          data-test-id={testId}
        />
        <label htmlFor={id}>{title}</label>
      </div>
    );
  },
);
