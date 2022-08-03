import { Menu, MenuItem, MenuProps } from '@mui/material';
import * as React from 'react';
import { Key, useCallback } from 'react';
import clsx from 'clsx';
import styles from './AppMenu.module.scss';

export type AppMenuItem = {
  text: string;
  onClick: () => void;
  key?: Key | null | undefined;
  icon?: React.ReactNode;
  className?: string;
};

/*
 * For menu positioning you could experiment on https://mui.com/material-ui/react-popover/#anchor-playground
 */
export type AppMenuProps = MenuProps & {
  menuItems: AppMenuItem[];
};

const classes: MenuProps['classes'] = {
  paper: styles.paper,
  list: styles.list,
};
export const AppMenu: React.FC<AppMenuProps> = (props) => {
  const { menuItems, ...rest } = props;
  const onClose: NonNullable<MenuProps['onClose']> = useCallback(
    (ev: any, reason) => {
      ev.stopPropagation();
      props.onClose?.(ev, reason);
    },
    [props.onClose],
  );
  const onClick: NonNullable<MenuProps['onClick']> = useCallback(
    (e) => {
      e.stopPropagation();
      props.onClick?.(e);
    },
    [props.onClick],
  );
  return (
    <Menu {...rest} classes={classes} onClose={onClose} onClick={onClick}>
      {menuItems.map((menuItem) => (
        <MenuItem
          key={menuItem.key}
          onClick={menuItem.onClick}
          className={clsx(menuItem.className, styles.menuItem)}
        >
          {menuItem.icon}
          {menuItem.text}
        </MenuItem>
      ))}
    </Menu>
  );
};
