import * as React from 'react';
import styles from './RadioButton.module.scss';

type Props = React.InputHTMLAttributes<HTMLInputElement> & {
  title?: string;
  value?: string | number;
};

export const RadioButton = React.forwardRef<HTMLInputElement, Props>(
  function RadioButton(props, ref) {
    const { title, ...rest } = props;
    const id =
      props.id ?? props.name != undefined
        ? `uni-radio-${props.name}-${props.value}`
        : undefined;
    return (
      <div className={props.className}>
        <input
          type="radio"
          ref={ref}
          {...rest}
          id={id}
          className={styles.radio}
        />
        <label className={styles.label} htmlFor={id}>
          {title}
        </label>
      </div>
    );
  },
);
