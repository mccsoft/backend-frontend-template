import { useTransitionClass } from './useTransitionClass';
import { Popper } from '@mui/material';
import clsx from 'clsx';
import React, { useRef } from 'react';
import styles from './AppPopper.module.scss';

export const AppPopper: React.FC<
  React.PropsWithChildren<
    React.ComponentProps<typeof Popper> & { noArrow?: boolean }
  >
> = (props) => {
  const { children, noArrow, ...rest } = props;
  const arrowRef = useRef<HTMLDivElement>(null);
  const transitionClass = useTransitionClass(styles.popperEnterActive);
  return (
    <Popper {...rest} className={styles.root}>
      <div className={clsx(styles.popper, !noArrow && transitionClass)}>
        {props.open && !noArrow && (
          <div className={styles.arrow} ref={arrowRef} data-popper-arrow></div>
        )}
        <div className={styles.content}>{props.children}</div>
      </div>
    </Popper>
  );
};
