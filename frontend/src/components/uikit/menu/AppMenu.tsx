import { Menu, MenuItem, MenuProps, useEventCallback } from '@mui/material';
import * as React from 'react';
import { Key } from 'react';
import clsx from 'clsx';
import styles from './AppMenu.module.scss';
import { anchorTransformOrigin } from './anchorTransformOrigin';
import { MenuDirection } from './MenuDirection';

export type AppMenuItem = {
  text: string;
  onClick: () => void;
  key?: Key | null | undefined;
  icon?: React.ReactNode;
  className?: string;
  /*
   * Some special styles are applied to 'danger' items (e.g. Delete)
   */
  isDanger?: boolean;
};

/*
 * For menu positioning you could experiment on https://mui.com/material-ui/react-popover/#anchor-playground
 */
export type AppMenuProps = MenuProps & {
  menuItems: AppMenuItem[];
  /*
   * You could use this prop instead of `anchorOrigin` & `transformOrigin`.
   * If `direction` is specified it takes the priority over `anchorOrigin` & `transformOrigin`.
   */
  direction?: MenuDirection;

  testId?: string;
};

const classes: MenuProps['classes'] = {
  paper: styles.paper,
  list: styles.list,
};
export const AppMenu: React.FC<AppMenuProps> = (props) => {
  const { menuItems, direction, testId, ...rest } = props;
  const onClose: NonNullable<MenuProps['onClose']> = useEventCallback(
    (ev: any, reason) => {
      ev.stopPropagation();
      props.onClose?.(ev, reason);
    },
  );
  const onClick: NonNullable<MenuProps['onClick']> = useEventCallback((e) => {
    e.stopPropagation();
    props.onClick?.(e);
  });

  return (
    <Menu
      {...rest}
      data-test-id={testId}
      {...(direction !== undefined
        ? anchorTransformOrigin[direction]
        : undefined)}
      classes={classes}
      onClose={onClose}
      onClick={onClick}
    >
      {menuItems.map((menuItem) => (
        <MenuItem
          key={menuItem.key}
          onClick={menuItem.onClick}
          className={clsx(
            menuItem.className,
            styles.menuItem,
            menuItem.isDanger && styles.deleteItem,
          )}
        >
          {menuItem.icon}
          {menuItem.text}
        </MenuItem>
      ))}
    </Menu>
  );
};
