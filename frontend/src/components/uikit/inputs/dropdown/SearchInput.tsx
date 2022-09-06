import { Input, Props } from '../Input';
import { ReactComponent as SearchIcon } from 'assets/icons/search.svg';
import React from 'react';

export const SearchInput: React.FC<Props> = React.forwardRef<
  HTMLInputElement,
  Props
>(function SearchInput(props, ref) {
  return (
    <Input
      ref={ref}
      endAdornment={<SearchIcon />}
      variant={'normal'}
      {...props}
    />
  );
});
