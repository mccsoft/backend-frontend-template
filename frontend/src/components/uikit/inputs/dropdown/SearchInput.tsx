import { Input, Props } from '../Input';
import { ReactComponent as SearchIcon } from 'assets/icons/search.svg';
import { ReactComponent as CrossIcon } from 'assets/icons/close-button.svg';
import React, { useCallback } from 'react';

import styles from '../Input.module.scss';
import { mergeRefs } from 'react-merge-refs';

export const SearchInput: React.FC<Props> = React.forwardRef<
  HTMLInputElement,
  Props
>(function SearchInput(props, ref) {
  const localRef = React.useRef<HTMLInputElement>();
  const onCrossIconClick = useCallback(() => {
    localRef.current!.value = '';
    props.onChange?.({
      target: localRef.current!,
      currentTarget: localRef.current!,
      type: 'change',
      preventDefault() {},
      stopPropagation() {},
      isDefaultPrevented(): boolean {
        return false;
      },
    } as any);
  }, []);
  return (
    <Input
      ref={mergeRefs([ref, localRef])}
      endAdornment={
        <>
          {!!props.value && (
            <CrossIcon
              className={styles.searchInputCross}
              onClick={onCrossIconClick}
            />
          )}
          <SearchIcon />
        </>
      }
      variant={'normal'}
      {...props}
    />
  );
});
