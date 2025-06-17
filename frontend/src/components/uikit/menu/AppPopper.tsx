import { Popper as PopperUnstyled } from '@mui/base/Popper/Popper';
import clsx from 'clsx';
import { useTriggerOnClickOutsideElement } from 'helpers/useTriggerOnClickOutsideElement';
import React, { MouseEventHandler, useEffect, useRef, useState } from 'react';
import styles from './AppPopper.module.scss';
import { useTransitionClass } from './useTransitionClass';
import { PopperProps } from '@mui/base/Popper/Popper.types';
// import type { PopperUnstyledOwnProps } from '@mui/base/PopperUnstyled/PopperUnstyled';

/*
 * Poppers could be used as a Tooltips, where you can't wrap an `anchor` into a Tooltip
 * and want to provide an `anchorEl` as a prop instead.
 * Otherwise (if you can wrap you anchor element into another component), please use <AppTooltip>
 */
export const AppPopper: React.FC<
  React.PropsWithChildren<
    PopperProps & {
      noArrow?: boolean;
      delay?: number;
      disableTransition?: boolean;
      // className that is applied to a root element of a popper
      rootClassName?: string;
      // true by default
      hideOnClickOutside?: boolean;
      onClose?: () => void;

      /*
       * This should not be used, but could be passed to Popper if used with {...bindMenu()}
       * We need to define it here to be able to destructure to NOT pass to underlying PopperUnstyled
       */
      anchorPosition?: any;
    }
  >
> = (props) => {
  const {
    children,
    noArrow,
    delay,
    disableTransition,
    rootClassName,
    className,
    hideOnClickOutside = true,
    onClose,
    anchorPosition,
    ...rest
  } = props;

  const [isOpenDelayed, setIsOpenDelayed] = useState(false);
  useEffect(
    function delayOpening() {
      if (!delay) return;

      if (!props.open) {
        setIsOpenDelayed(false);
      } else {
        const timerId = setTimeout(() => {
          const resolvedAnchorEl = resolveAnchorEl(props.anchorEl);
          if (resolvedAnchorEl) {
            const box = resolvedAnchorEl.getBoundingClientRect();

            if (
              box.top === 0 &&
              box.left === 0 &&
              box.right === 0 &&
              box.bottom === 0
            ) {
              onClose?.();
              return;
            }
          }

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
  const isOpenRef = useRef(isOpen);
  isOpenRef.current = isOpen;

  const popper = (
    <PopperUnstyled
      {...rest}
      open={isOpen}
      className={clsx(styles.root, rootClassName)}
    >
      {isOpen
        ? (childProps) => (
            <PopperContent
              disableTransition={disableTransition}
              className={className}
              noArrow={noArrow}
              onClose={onClose}
              hideOnClickOutside={hideOnClickOutside}
            >
              {typeof children === 'function' ? children(childProps) : children}
            </PopperContent>
          )
        : null}
    </PopperUnstyled>
  );

  return popper;
};
const PopperContent: React.FC<
  React.PropsWithChildren<{
    disableTransition: boolean | undefined;
    className: string | undefined;
    noArrow: boolean | undefined;
    hideOnClickOutside: boolean | undefined;
    onClose: undefined | (() => void);
  }>
> = ({
  className,
  disableTransition,
  noArrow,
  onClose,
  hideOnClickOutside,
  children,
}) => {
  const arrowRef = useRef<HTMLDivElement>(null);
  const transitionClass = useTransitionClass(
    disableTransition ? undefined : styles.popperEnterActive,
  );
  const ref = useRef<HTMLDivElement>(null!);
  useTriggerOnClickOutsideElement(
    ref,
    onClose!,
    !!onClose && !!hideOnClickOutside,
  );

  return (
    <div
      ref={ref}
      className={clsx(styles.popper, className, !noArrow && transitionClass)}
      // prevent propagation of onMouseDown so that useTriggerOnClickOutside doesn't catch clicks inside the Popper
      onMouseDown={stopPropagation}
    >
      {!noArrow && (
        <div className={styles.arrow} ref={arrowRef} data-popper-arrow></div>
      )}
      <div className={styles.content}>{children}</div>
    </div>
  );
};
const stopPropagation: MouseEventHandler<any> = (e) => {
  e.stopPropagation();
};
function resolveAnchorEl(
  anchorEl: React.ComponentProps<typeof PopperUnstyled>['anchorEl'],
) {
  return typeof anchorEl === 'function' ? anchorEl() : anchorEl;
}
