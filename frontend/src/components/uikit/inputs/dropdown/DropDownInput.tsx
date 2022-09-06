import * as React from 'react';
import { useMemo } from 'react';
import { StyledAutocomplete } from './StyledAutocomplete';
import { DropDownInputProps, StyledAutocompleteProps } from './types';

export function DropDownInput<
  T,
  Required extends boolean | undefined = undefined,
  UseIdAsValue extends boolean | undefined = undefined,
>(props: DropDownInputProps<T, Required, UseIdAsValue>) {
  const { onValueChanged, ...rest } = props;
  const onChange: StyledAutocompleteProps<
    T,
    false,
    Required,
    false
  >['onChange'] = useMemo(
    () => (e, item) => {
      onValueChanged(item as any);
    },
    [onValueChanged],
  );
  return (
    <StyledAutocomplete<T, false, Required, false>
      {...rest}
      multiple={false}
      onChange={onChange}
    />
  );
}
