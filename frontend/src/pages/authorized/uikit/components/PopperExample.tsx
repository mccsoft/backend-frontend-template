import { Button, ButtonColor } from 'components/uikit/buttons/Button';
import React from 'react';
import {
  bindHover,
  bindPopper,
  usePopupState,
} from 'material-ui-popup-state/hooks';
import { AppPopper } from 'components/uikit/menu/AppPopper';
import { AppTooltip } from 'components/uikit/menu/AppTooltip';

export const PopperExample = () => {
  const popperState = usePopupState({
    popupId: 'popper1',
    variant: 'popper',
  });
  return (
    <div>
      <AppTooltip title={<div>Tooltip contents!</div>} arrow={true}>
        <Button
          color={ButtonColor.Secondary}
          title={'Simple tooltip on hover'}
        ></Button>
      </AppTooltip>
      <Button
        {...bindHover(popperState)}
        color={ButtonColor.Secondary}
        title={'Advanced (e.g. when you need to manually control everything)'}
      ></Button>
      <AppPopper {...bindPopper(popperState)}>blablabla</AppPopper>
    </div>
  );
};
