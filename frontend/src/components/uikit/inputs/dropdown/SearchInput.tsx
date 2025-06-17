import clsx from 'clsx';
import { Input, Props } from '../Input';
import SearchIcon from 'assets/icons/search.svg?react';
import CrossIcon from 'assets/icons/close-button.svg?react';
import React, { ChangeEvent, useCallback } from 'react';

import styles from '../Input.module.scss';
import { mergeRefs } from 'react-merge-refs';

export const SearchInput: React.FC<Props> = React.forwardRef<
  HTMLInputElement,
  Props
>(function SearchInput(props, ref) {
  const localRef = React.useRef<HTMLInputElement>(undefined);
  const onCrossIconClick = useCallback(() => {
    localRef.current!.value = '';

    props.onChange?.(createChangeEventArgs(localRef.current!));

    const keyboardEvent = new window.KeyboardEvent('keydown', {
      code: '\n',
      key: 'Escape',
      bubbles: true,
    });

    // we simulate pressing Escape twice:
    // - first press closes the popup
    // - second press clears the input
    localRef.current?.dispatchEvent(keyboardEvent);
    setTimeout(() => localRef.current?.dispatchEvent(keyboardEvent), 1);
  }, []);
  return (
    <Input
      ref={mergeRefs([ref, localRef])}
      {...props}
      endAdornment={
        props.disabled ? undefined : (
          <>
            {!!props.value && (
              <CrossIcon
                className={styles.crossIcon}
                onClick={onCrossIconClick}
              />
            )}
            <SearchIcon className={styles.searchIcon} />
          </>
        )
      }
      className={clsx(styles.search, props.className)}
    />
  );
});
const createChangeEventArgs = (
  element: HTMLInputElement,
): ChangeEvent<HTMLInputElement> => {
  return {
    target: element,
    currentTarget: element,
    type: 'change',
    preventDefault() {},
    stopPropagation() {},
    isDefaultPrevented(): boolean {
      return false;
    },
  } as any;
};
