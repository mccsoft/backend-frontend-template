import React, { useState } from 'react';
import { AppPopper } from './AppPopper';
import type { PopperProps, TooltipProps } from '@mui/material';
import {
  bindHover,
  bindPopper,
  usePopupState,
} from 'material-ui-popup-state/hooks';
import styles from './AppTooltip.module.scss';

type AppTooltipProps = {
  arrow?: boolean;
  /**
   * Tooltip reference element.
   */
  children: React.ReactElement<any, any>;
  title: TooltipProps['title'];
} & Omit<React.ComponentProps<typeof AppPopper>, 'open' | 'title'>;

/*
 * Styling of MUI Tooltip is quite hard, as we need to override all it default styles.
 * ToDo: switch to unstyled version of MUI Tooltip once it's done
 *  (could track the progress here https://github.com/mui/material-ui/issues/27170)
 * Now let's use a basic version which mimics the Tooltip API (i.e. has `children` and `title`)
 */
export const AppTooltip: React.FC<AppTooltipProps> = (props) => {
  const { children, arrow, title, ...rest } = props;
  const [popupId] = useState(() => new Date().getTime().toString());
  const popupState = usePopupState({
    variant: 'popper',
    popupId: popupId,
  });

  return (
    <>
      {React.cloneElement(children, {
        ...children.props,
        ...bindHover(popupState),
      })}
      {title && popupState.isOpen && (
        <AppPopper
          delay={500}
          {...rest}
          {...bindPopper(popupState)}
          noArrow={!arrow}
          className={styles.popper}
          rootClassName={styles.root}
          onClick={onClickHandler}
        >
          {title}
        </AppPopper>
      )}
    </>
  );
};
const onClickHandler: NonNullable<PopperProps['onClick']> = (e) =>
  e.stopPropagation();
