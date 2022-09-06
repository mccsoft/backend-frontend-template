import * as React from 'react';
import { useMemo } from 'react';
import { AutocompleteProps } from '@mui/material/Autocomplete/Autocomplete';
import { StyledAutocomplete } from './StyledAutocomplete';
import { StyledAutocompleteProps } from './types';

export function ComboBoxInput<
  T,
  Required extends boolean | undefined = undefined,
>(
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
      filterOptions={props.enableSearch ? undefined : noFilter}
      {...rest}
      multiple={false}
      freeSolo={true}
      onChange={onChange}
    />
  );
}
function noFilter(option: any) {
  return option;
}
