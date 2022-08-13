import React, { FC } from 'react';
import { AppLink } from 'components/uikit/buttons/AppLink';
import { ButtonColor } from './buttons/Button';
import clsx from 'clsx';
import { ReactComponent as HintIcon } from 'assets/icons/i.svg';

import styles from './Field.module.scss';
import { AppTooltip } from './menu/AppTooltip';

export interface FieldProps {
  title: string;
  children?: React.ReactNode;
  className?: string;
  titleClassName?: string;
  linkProps?: {
    title: string | React.ReactNode;
    disabled?: boolean;
    icon?: string;
    onClick?: (e: React.MouseEvent<HTMLAnchorElement>) => void;
    className?: string;
  };
  hint?: string;
}

export const Field: FC<FieldProps> = (props) => {
  const { children, className, linkProps, title, titleClassName } = props;
  return (
    <div className={clsx(className, styles.container)}>
      <div className={styles.titleContainer}>
        <div className={styles.titleWithHint}>
          <div className={clsx(styles.title, titleClassName)}>{title}</div>
          {props.hint ? (
            <AppTooltip title={props.hint} arrow={false}>
              <div className={styles.hint}>
                <HintIcon />
              </div>
            </AppTooltip>
          ) : null}
        </div>
        {!!linkProps &&
          (typeof linkProps.title === 'string' ? (
            <AppLink
              {...linkProps}
              color={ButtonColor.Primary}
              className={clsx([styles.fieldLink, linkProps.className])}
            >
              {linkProps.title}
            </AppLink>
          ) : (
            linkProps.title
          ))}
      </div>
      <div className={styles.field}>{children}</div>
    </div>
  );
};
