import clsx from 'clsx';
import * as React from 'react';
import { useCallback, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import arrowDownIcon from 'assets/icons/arrow-down.svg';

import styles from './StyledAutocomplete.module.scss';
import { Autocomplete } from '@mui/material';
import { Input } from '../Input';
import { AutocompleteProps } from '@mui/material/Autocomplete/Autocomplete';
import { AutocompleteClasses } from '@mui/material/Autocomplete/autocompleteClasses';

export interface CustomOption {
  key: number | string;
  label: string;
  onClick: () => void;
}

const caretDown = <img src={arrowDownIcon} className={styles.expandIcon} />;

export type StyledAutocompleteProps<
  T,
  Multiple extends boolean | undefined = undefined,
  Required extends boolean | undefined = undefined,
  FreeSolo extends boolean | undefined = undefined,
> = Omit<
  AutocompleteProps<T, Multiple, Required, FreeSolo>,
  'disableClearable' | 'renderInput'
> & {
  emptyLabel?: string;
  rootClassName?: string;
  required?: Required;
  testId?: string;
  errorText?: string;
  /*
   * Makes it possible to type right into the input to filter results
   */
  enableSearch?: boolean;
  /*
   * 'normal' - input will have minimal width
   * 'formInput' - input will have the standard width (as all form elements)
   */
  variant?: 'normal' | 'formInput';
};

export function StyledAutocomplete<
  T,
  Multiple extends boolean | undefined = undefined,
  Required extends boolean | undefined = undefined,
  FreeSolo extends boolean | undefined = undefined,
>(
  props: StyledAutocompleteProps<T, Multiple, Required, FreeSolo>,
): JSX.Element {
  const i18next = useTranslation();
  const {
    emptyLabel,
    rootClassName,
    required,
    testId,
    errorText,
    enableSearch,
    ...rest
  } = {
    emptyLabel: i18next.t('uikit.inputs.nothing_selected'),
    ...props,
  };

  const dropdownStyles: Partial<AutocompleteClasses> = useMemo(
    () => ({
      option: styles.optionValue,
      popper: styles.dropdownCallout,
      listbox: styles.listbox,
    }),
    [],
  );

  const options = useMemo(() => {
    if (required || props.multiple) return props.options;
    return props.options.includes(null!)
      ? props.options
      : [null!, ...props.options];
  }, [required, props.options]);

  const getOptionLabel: typeof props['getOptionLabel'] = useMemo(() => {
    if (required) return props.getOptionLabel;
    return (option) => {
      if (option === null || option === undefined) {
        return emptyLabel;
      }

      return props.getOptionLabel?.(option) ?? (option as any).toString();
    };
  }, [required, props.getOptionLabel, emptyLabel]);

  return (
    <div
      className={clsx(styles.rootContainer, rootClassName)}
      style={props.style}
    >
      <Autocomplete
        {...rest}
        options={options}
        renderInput={(params) => {
          const value = props.multiple
            ? (params.InputProps.startAdornment as string)
            : params.inputProps.value;
          return (
            <Input
              containerRef={params.InputProps.ref}
              placeholder={emptyLabel}
              {...params.inputProps}
              value={value}
              size={undefined}
              readOnly={!enableSearch}
              endAdornment={caretDown}
              variant={props.variant}
            />
          );
        }}
        classes={dropdownStyles}
        data-test-id={testId}
        data-error={!!errorText}
        placeholder={emptyLabel}
        getOptionLabel={getOptionLabel}
        disableClearable={props.required}
        value={props.value ?? null!}
      />
      {!!errorText && <div className={styles.errorText}>{errorText}</div>}
    </div>
  );
}
