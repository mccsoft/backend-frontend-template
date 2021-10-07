import clsx from 'clsx';
import * as React from 'react';
import { ButtonColor } from './Button';

const styles = require('./AppLink.module.scss');

export interface LinkProps {
  color?: ButtonColor;
  className?: string;
  disabled?: boolean;
  icon?: string;
  onClick?: (e: React.MouseEvent<HTMLAnchorElement>) => void;
}

export const AppLink: React.FC<LinkProps> = (props) => {
  const { color, className, disabled, icon, children, ...rest } = {
    ...defaultProps,
    ...props,
  };

  const linkStyles = [className];
  linkStyles.push(styles.link);
  linkStyles.push(styles[`${color}-link`]);

  return (
    <a data-disabled={disabled} {...rest} className={clsx(linkStyles)}>
      {icon && <img className={styles.icon} src={icon} />}
      {children}
    </a>
  );
};

const defaultProps: Partial<LinkProps> = {
  color: ButtonColor.Default,
};
