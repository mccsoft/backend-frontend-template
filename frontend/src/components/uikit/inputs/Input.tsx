import { ReactComponent as EyeHide } from 'assets/auth/eye-hide.svg';
import { ReactComponent as EyeShow } from 'assets/auth/eye-show.svg';
import clsx from 'clsx';
import React, { KeyboardEventHandler, Ref, useState } from 'react';

import styles from './Input.module.scss';

export type Props = React.InputHTMLAttributes<HTMLInputElement> & {
  containerRef?: Ref<HTMLDivElement>;
  onEnterPressed?: (text: string) => void;
  className?: string;
  errorText?: string;
  helperText?: string;
  type?: string;
  badge?: React.ReactNode;
  testId?: string;
};

export const Input = React.forwardRef<HTMLInputElement, Props>(function Input(
  props,
  ref,
) {
  const {
    containerRef,
    onClick,
    style,
    className,
    helperText,
    errorText,
    onEnterPressed,
    type,
    badge,
    ...rest
  } = props;
  const isError = !!errorText;

  const onKeyDown: KeyboardEventHandler<HTMLInputElement> = (e) => {
    if (e.key === 'Enter') {
      if (onEnterPressed) {
        onEnterPressed(e.currentTarget.value);
        e.preventDefault();
        e.stopPropagation();
      }
    }
  };
  const [showPassword, setShowPassword] = useState<boolean | undefined>(
    type === 'password' ? false : undefined,
  );

  return (
    <div
      ref={containerRef}
      onClick={onClick}
      style={style}
      className={styles.inputContainer}
    >
      <input
        ref={ref}
        className={clsx(styles.input, className)}
        data-error={isError}
        onKeyDown={onEnterPressed ? onKeyDown : undefined}
        type={showPassword ? 'input' : type}
        data-test-id={props.testId}
        {...rest}
      />
      {showPassword !== undefined && (
        <div
          className={clsx(styles.passwordEye, 'passwordEye')}
          onClick={() => {
            setShowPassword((oldValue) => !oldValue);
          }}
        >
          {showPassword ? <EyeHide /> : <EyeShow />}
        </div>
      )}
      {badge}
      {!!(helperText || errorText) && (
        <div data-error={isError} className={styles.helperText}>
          {helperText || errorText}
        </div>
      )}
    </div>
  );
});
