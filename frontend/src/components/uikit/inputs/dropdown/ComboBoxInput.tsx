import * as React from 'react';
import { useMemo } from 'react';
import { AutocompleteProps } from '@mui/material/Autocomplete/Autocomplete';
import { StyledAutocomplete } from './StyledAutocomplete';
import { StyledAutocompleteProps } from './types';

export function ComboBoxInput<T, Required extends boolean>(
  props: Omit<StyledAutocompleteProps<T, false, Required, true>, 'onChange'> & {
    onValueChanged: (value: T | null | string) => void;
  },
) {
  const { onValueChanged, ...rest } = props;
  const onChange: NonNullable<
    AutocompleteProps<T, false, Required, true>['onChange']
  > = useMemo(
    () => (e, item) => {
      onValueChanged(item);
    },
    [onValueChanged],
  );
  return (
    <StyledAutocomplete<T, false, Required, true>
      {...rest}
      freeSolo={true}
      onChange={onChange}
    />
  );
}
