import React, { useRef, useState } from 'react';
import { AppMenu, AppMenuProps } from './AppMenu';
import styles from './DotMenu.module.scss';
import { ReactComponent as Dots } from 'assets/icons/dots.svg';

export type DotMenuProps = Omit<AppMenuProps, 'anchorEl' | 'open'>;

export const DotMenu: React.FC<DotMenuProps> = React.memo(function DotMenu(
  props,
) {
  const menuAnchorRef = useRef<HTMLElement>(null);
  const [isMenuShown, setIsMenuShown] = useState(false);
  return (
    <div>
      <div
        ref={menuAnchorRef as any}
        className={styles.dotMenu}
        onClick={() => {
          setIsMenuShown(true);
        }}
      >
        <Dots className={styles.dotMenuImage} />
      </div>
      <AppMenu
        {...props}
        anchorEl={menuAnchorRef.current}
        open={isMenuShown}
        onClose={() => {
          setIsMenuShown(false);
        }}
      />
    </div>
  );
});
