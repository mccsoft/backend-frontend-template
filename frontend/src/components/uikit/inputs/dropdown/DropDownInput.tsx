import * as React from 'react';
import { useMemo } from 'react';
import { AutocompleteProps } from '@mui/material/Autocomplete/Autocomplete';
import {
  StyledAutocomplete,
  StyledAutocompleteProps,
} from './StyledAutocomplete';

export function DropDownInput<T, Required extends boolean>(
  props: Omit<
    StyledAutocompleteProps<T, false, Required, false>,
    'onChange'
  > & {
    onValueChanged: Required extends true
      ? (newSelectedOption: T) => void
      : (newSelectedOption: T | null) => void;
  },
) {
  const { onValueChanged, ...rest } = props;
  const onChange: AutocompleteProps<T, false, Required, false>['onChange'] =
    useMemo(
      () => (e, item) => {
        onValueChanged(item!);
      },
      [onValueChanged],
    );
  return <StyledAutocomplete {...rest} onChange={onChange} />;
}
