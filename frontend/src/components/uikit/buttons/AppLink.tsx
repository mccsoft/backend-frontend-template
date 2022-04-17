import clsx from 'clsx';
import * as React from 'react';
import { ButtonColor } from './Button';
import { Link } from 'react-router-dom';
import { PropsWithChildren } from 'react';

const styles = require('./AppLink.module.scss');

export type LinkProps = {
  color?: ButtonColor;
  className?: string;
  disabled?: boolean;
  icon?: string;
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
  const { color, className, disabled, icon, children, ...rest } = {
    ...defaultProps,
    ...props,
  };

  const linkStyles = [className];
  linkStyles.push(styles.link);
  linkStyles.push(styles[`${color}-link`]);

  return (
    <Link
      data-disabled={disabled}
      {...rest}
      to={props.to ?? ''}
      className={clsx(linkStyles)}
    >
      {icon && <img className={styles.icon} src={icon} />}
      {children}
    </Link>
  );
};

const defaultProps: Partial<LinkProps> = {
  color: ButtonColor.Default,
};
