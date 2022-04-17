import React, { FC } from 'react';
import clsx from 'clsx';

import styles from './IconButton.module.scss';

export interface IconButtonProps {
  icon: string;
  disabled?: boolean;
  onClick?: (e: React.MouseEvent<HTMLAnchorElement>) => void;
  className?: string;
  iconClassName?: string;
  id?: string;
  onBlur?: () => void;
  tabIndex?: number;
  testId?: string;
  title?: string;
}

export const IconButton: FC<IconButtonProps> = React.forwardRef<
  HTMLAnchorElement,
  IconButtonProps
>(function IconButton(props, ref) {
  return (
    <a
      ref={ref}
      data-disabled={props.disabled}
      onClick={(e: React.MouseEvent<HTMLAnchorElement>) => {
        e.stopPropagation();
        props.onClick?.(e);
      }}
      className={clsx(styles.iconButton, props.className)}
      id={props.id}
      onBlur={props.onBlur}
      tabIndex={props.tabIndex}
      data-test-id={props.testId}
      title={props.title}
    >
      <img
        className={clsx(styles.image, props.iconClassName)}
        src={props.icon}
      />
    </a>
  );
});
