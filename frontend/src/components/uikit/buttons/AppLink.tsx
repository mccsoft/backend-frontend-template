import clsx from 'clsx';
import * as React from 'react';
import { ButtonColor } from './Button';
import { Link } from 'react-router-dom';
import { PropsWithChildren } from 'react';

import styles from './AppLink.module.scss';

export type LinkProps = {
  color?: ButtonColor;
  className?: string;
  disabled?: boolean;
  icon?: string;
  testId?: string;
} & (
  | {
      onClick?: (e: React.MouseEvent<HTMLAnchorElement>) => void;
      to?: undefined;
    }
  | {
      to: string;
    }
);

export const AppLink: React.FC<PropsWithChildren<LinkProps>> = (props) => {
  const { color, className, disabled, icon, children, testId, ...rest } = {
    ...defaultProps,
    ...props,
  };

  return (
    <Link
      data-disabled={disabled}
      {...rest}
      to={props.to ?? ''}
      className={clsx(className, styles.link, styles[`${color}-link`])}
      data-test-id={testId}
    >
      {icon && <img className={styles.icon} src={icon} />}
      {children}
    </Link>
  );
};

const defaultProps: Partial<LinkProps> = {
  color: ButtonColor.Default,
};
