import React, { FC } from 'react';
import { AppLink } from 'components/uikit/buttons/AppLink';
import { ButtonColor } from './buttons/Button';
import clsx from 'clsx';
import HintIcon from 'assets/icons/i.svg?react';

import styles from './Field.module.scss';
import { AppTooltip } from './menu/AppTooltip';

export type FieldLinkProps = {
  title: string | React.ReactNode;
  disabled?: boolean;
  icon?: string;
  onClick?: (e: React.MouseEvent<HTMLAnchorElement>) => void;
  className?: string;
  testId?: string;
  color?: ButtonColor;
};

export type FieldProps = {
  title: string;
  children?: React.ReactNode;
  className?: string;
  titleClassName?: string;
  linkProps?: FieldLinkProps;
  hint?: string;
  testId?: string;
};

export const Field: FC<FieldProps> = (props) => {
  const { children, className, linkProps, title, titleClassName, testId } =
    props;
  return (
    <div
      className={clsx(className, styles.container)}
      data-test-id={testId}
      data-app-field={title}
    >
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
              testId={linkProps.testId ?? 'field-link'}
              color={ButtonColor.Primary}
              className={clsx(styles.fieldLink, linkProps.className)}
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
