import clsx from 'clsx';
import { CSSProperties } from 'react';
import * as React from 'react';

import styles from './Button.module.scss';

export enum ButtonWidth {
  Content = 'content',
  Fullwidth = 'fullwidth',
}

export enum ButtonColor {
  Default = 'default',
  Primary = 'primary',
  Secondary = 'secondary',
  Danger = 'danger',
}

export type ButtonProps = {
  id?: string;
  type?: 'submit' | 'button';
  title?: string;
  color?: ButtonColor;
  width?: ButtonWidth;
  className?: string;
  disabled?: boolean;
  icon?: string;
  iconClassName?: string;
  onClick?: (e: React.MouseEvent<HTMLElement>) => void;
  testId?: string;
} & Pick<React.ButtonHTMLAttributes<HTMLButtonElement>, 'autoFocus' | 'style'>;

export const Button = React.forwardRef<HTMLButtonElement, ButtonProps>(
  function Button(props, ref) {
    const {
      title,
      color,
      width,
      className,
      disabled,
      icon,
      iconClassName,
      ...rest
    } = {
      ...DefaultProps,
      ...props,
    };

    const buttonStyles = [
      className,
      styles.button,
      styles[`${width}-button`],
      styles[`${color}-button`],
    ];
    return (
      <button
        ref={ref}
        type={props.type ?? 'button'}
        data-disabled={disabled}
        {...rest}
        className={clsx(buttonStyles)}
        data-test-id={props.testId}
      >
        {icon && (
          <img className={clsx(styles.icon, iconClassName)} src={icon} />
        )}
        {title}
      </button>
    );
  },
);

const DefaultProps: Partial<ButtonProps> = {
  color: ButtonColor.Default,
  width: ButtonWidth.Content,
};
