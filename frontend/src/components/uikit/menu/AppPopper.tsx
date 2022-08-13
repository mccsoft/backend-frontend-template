import { useTransitionClass } from './useTransitionClass';
import PopperUnstyled from '@mui/base/PopperUnstyled';
import clsx from 'clsx';
import React, { useEffect, useRef, useState } from 'react';
import styles from './AppPopper.module.scss';

/*
 * Poppers could be used as a Tooltips, where you can't wrap an `anchor` into a Tooltip
 * and want to provide an `anchorEl` as a prop instead.
 * Otherwise (if you can wrap you anchor element into another component), please use <AppTooltip>
 */
export const AppPopper: React.FC<
  React.ComponentProps<typeof PopperUnstyled> & {
    noArrow?: boolean;
    delay?: number;
    disableTransition?: boolean;
    // className that is applied to a root element of a popper
    rootClassName?: string;
  }
> = (props) => {
  const {
    children,
    noArrow,
    delay,
    disableTransition,
    rootClassName,
    className,
    ...rest
  } = props;
  const arrowRef = useRef<HTMLDivElement>(null);
  const transitionClass = useTransitionClass(
    disableTransition ? undefined : styles.popperEnterActive,
  );

  const [isOpenDelayed, setIsOpenDelayed] = useState(false);
  useEffect(
    function delayOpening() {
      if (!delay) return;

      if (!props.open) {
        setIsOpenDelayed(false);
      } else {
        const timerId = setTimeout(() => {
          setIsOpenDelayed(true);
        }, delay ?? 0);
        return () => {
          clearTimeout(timerId);
        };
      }
    },
    [props.open],
  );
  const isOpen = delay ? isOpenDelayed : props.open;

  return (
    <PopperUnstyled
      {...rest}
      open={isOpen}
      className={clsx(styles.root, rootClassName)}
    >
      {(childProps) => (
        <div
          className={clsx(
            styles.popper,
            className,
            !noArrow && transitionClass,
          )}
        >
          {props.open && !noArrow && (
            <div
              className={styles.arrow}
              ref={arrowRef}
              data-popper-arrow
            ></div>
          )}
          <div className={styles.content}>
            {typeof children === 'function' ? children(childProps) : children}
          </div>
        </div>
      )}
    </PopperUnstyled>
  );
};
