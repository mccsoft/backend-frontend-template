import * as React from 'react';
import { useMemo } from 'react';
import { AutocompleteProps } from '@mui/material/Autocomplete/Autocomplete';
import { CheckBox } from 'components/uikit/CheckBox';
import {
  convertPropertyAccessorToFunction,
  StyledAutocomplete,
  StyledAutocompleteProps,
} from './StyledAutocomplete';
import { emptyArray } from '../../table/AppTable';

import styles from './StyledAutocomplete.module.scss';
import { useTranslation } from 'react-i18next';
import clsx from 'clsx';

export function MultiSelectDropDownInput<T, Required extends boolean>(
  props: Omit<StyledAutocompleteProps<T, true, Required, false>, 'onChange'> & {
    onValueChanged: (newValues: ReadonlyArray<T>) => void;
    /*
     * Hides the header with `All/None` buttons and a title
     */
    hideHeader?: boolean;
    /*
     * Text that is put in the Header. Usually a name of a field
     */
    headerTitle?: string;
  },
) {
  const { onValueChanged, hideHeader, headerTitle, ...rest } = props;
  const i18next = useTranslation();

  const onChange: AutocompleteProps<T, true, Required, false>['onChange'] =
    useMemo(
      () => (e, item) => {
        onValueChanged(item);
      },
      [onValueChanged],
    );
  const getOptionLabel = useMemo(
    () =>
      convertPropertyAccessorToFunction<T, true, Required, false>(
        props.getOptionLabel,
      ),
    [props.getOptionLabel],
  );
  const isAllSelected = useMemo(() => {
    return !props.options || props.value?.length === props.options.length;
  }, [props.value?.length, props.options]);

  const isNoneSelected = useMemo(() => {
    return !props.value?.length;
  }, [props.value]);

  const popupHeader = useMemo(() => {
    if (hideHeader) return undefined;
    return (
      <Header
        isNoneSelected={isNoneSelected}
        isAllSelected={isAllSelected}
        selectAllText={i18next.t('uikit.inputs.select_all_button')}
        selectNoneText={i18next.t('uikit.inputs.select_none_button')}
        options={props.options}
        onValueChanged={props.onValueChanged}
        headerTitle={headerTitle}
      />
    );
  }, [hideHeader, props.options, headerTitle, isAllSelected, isNoneSelected]);
  return (
    <StyledAutocomplete
      onChange={onChange}
      multiple={true}
      disableCloseOnSelect={true}
      popupHeader={popupHeader}
      renderOption={(optionProps, option, { selected }) => (
        <li {...optionProps}>
          <CheckBox
            className={styles.multiSelectCheckbox}
            checked={selected}
            title={getOptionLabel(option)}
          />
        </li>
      )}
      {...rest}
      value={rest.value ?? emptyArray}
    />
  );
}

function Header<T>(props: {
  isAllSelected: boolean;
  isNoneSelected: boolean;
  headerTitle?: string;
  selectAllText?: string;
  selectNoneText?: string;
  onValueChanged: (newValues: ReadonlyArray<T>) => void;
  options: ReadonlyArray<T>;
}) {
  return (
    <div
      className={styles.multiselectHeader}
      key="header"
      onMouseDown={(e) => {
        /* otherwise Popper is closed when user clicks within header */
        e.preventDefault();
        e.stopPropagation();
      }}
    >
      <span className={styles.headerText}>{props.headerTitle}</span>
      <div className={styles.linksBlock}>
        <a
          className={clsx(
            styles.link,
            props.isAllSelected && styles.linkDisabled,
          )}
          onClick={() => {
            props.onValueChanged(props.options);
          }}
        >
          {props.selectAllText}
        </a>
        <a
          className={clsx(
            styles.link,
            props.isNoneSelected && styles.linkDisabled,
          )}
          onClick={() => props.onValueChanged([])}
        >
          {props.selectNoneText}
        </a>
      </div>
    </div>
  );
}
