import { ReactComponent as EyeHide } from 'assets/auth/eye-hide.svg';
import { ReactComponent as EyeShow } from 'assets/auth/eye-show.svg';
import clsx from 'clsx';
import React, { KeyboardEventHandler, Ref, useMemo, useState } from 'react';

import styles from './Input.module.scss';

export type Props = React.InputHTMLAttributes<HTMLInputElement> & {
  containerRef?: Ref<HTMLDivElement>;
  onEnterPressed?: (text: string) => void;
  className?: string;
  errorText?: string;
  helperText?: string;
  type?: string;
  badge?: React.ReactNode;
  /*
   * endAdornment is an element that is placed on the right of the Input (visually inside the input)
   */
  endAdornment?: React.ReactNode;
  endAdornmentClassname?: string;
  testId?: string;
  /*
   * 'normal' - input will have minimal width
   * 'formInput' - input will have the standard width (as all form elements)
   */
  variant?: 'normal' | 'formInput';
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
    endAdornment: endAdornmentProps,
    endAdornmentClassname,
    variant,
    testId,
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

  const endAdornment = useMemo(() => {
    if (showPassword === undefined) {
      if (!props.endAdornment) return undefined;
      return { element: props.endAdornment, onClick: props.onClick };
    }
    return {
      element: showPassword ? <EyeHide /> : <EyeShow />,
      onClick: () => setShowPassword((v) => !v),
    };
  }, [props.endAdornment, showPassword]);

  return (
    <div
      ref={containerRef}
      onClick={onClick}
      style={style}
      className={styles.inputContainer}
    >
      <input
        ref={ref}
        className={clsx(
          styles.input,
          endAdornment ? styles.inputWithAdornment : null,
          variant === 'normal' ? styles.nonFormInput : null,
          className,
        )}
        data-error={isError}
        onKeyDown={onEnterPressed ? onKeyDown : undefined}
        type={showPassword ? 'input' : type}
        data-test-id={testId}
        {...rest}
      />
      {endAdornment ? (
        <div
          className={clsx(styles.passwordEye, endAdornmentClassname)}
          onClick={endAdornment.onClick}
          onFocus={props.onFocus}
          onMouseDown={props.onMouseDown}
        >
          {endAdornment.element}
        </div>
      ) : null}
      {badge}
      {!!(helperText || errorText) && (
        <div data-error={isError} className={styles.helperText}>
          {helperText || errorText}
        </div>
      )}
    </div>
  );
});
