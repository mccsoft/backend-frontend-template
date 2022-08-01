import * as React from 'react';
import { useMemo } from 'react';
import { AutocompleteProps } from '@mui/material/Autocomplete/Autocomplete';
import { CheckBox } from 'components/uikit/CheckBox';
import {
  StyledAutocomplete,
  StyledAutocompleteProps,
} from './StyledAutocomplete';
import { emptyArray } from '../../table/AppTable';

import styles from './StyledAutocomplete.module.scss';

export function MultiSelectDropDownInput<T, Required extends boolean>(
  props: Omit<StyledAutocompleteProps<T, true, Required, false>, 'onChange'> & {
    onValueChanged: (newValues: T[]) => void;
  },
) {
  const { onValueChanged, ...rest } = props;
  const onChange: AutocompleteProps<T, true, Required, false>['onChange'] =
    useMemo(
      () => (e, item) => {
        onValueChanged(item);
      },
      [onValueChanged],
    );
  return (
    <StyledAutocomplete
      onChange={onChange}
      multiple={true}
      disableCloseOnSelect={true}
      renderOption={(optionProps, option, { selected }) => (
        <li {...optionProps}>
          <CheckBox
            className={styles.multiSelectCheckbox}
            checked={selected}
            title={
              props.getOptionLabel?.(option) ?? (option as unknown as string)
            }
          />
        </li>
      )}
      {...rest}
      value={rest.value ?? emptyArray}
    />
  );
}
