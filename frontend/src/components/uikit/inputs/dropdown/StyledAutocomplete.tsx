import clsx from 'clsx';
import * as React from 'react';
import { useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import arrowDownIcon from 'assets/icons/arrow-down.svg';

import styles from './StyledAutocomplete.module.scss';
import { Autocomplete } from '@mui/material';
import { Input } from '../Input';
import { AutocompleteProps } from '@mui/material/Autocomplete/Autocomplete';
import { AutocompleteClasses } from '@mui/material/Autocomplete/autocompleteClasses';

export interface CustomOption {
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
  /*
   * You could add more options to Autocomplete beside the standard `menuItems`.
   * It's useful when for example you have a dropdown with categories and want to add a `Add New Category' item
   * Custom options are added to the top of the list.
   */
  customOptions?: CustomOption[];
};

type OptionType<T> = InternalOptionType | T;
type InternalOptionType =
  | ({ __optionType: 'custom' } & CustomOption)
  | { __optionType: 'not-selected' };
const notSelectedOption: InternalOptionType = { __optionType: 'not-selected' };
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

  const getOptionLabel: typeof props['getOptionLabel'] = useMemo(() => {
    return (option) => {
      if (option === null || option === undefined) return emptyLabel;

      const getDefaultValue = () =>
        props.getOptionLabel?.(option) ?? (option as any).toString();

      if (typeof option !== 'object') return getDefaultValue();

      const internalOption = option as unknown as InternalOptionType;
      if (internalOption.__optionType === 'not-selected') return emptyLabel;
      if (internalOption.__optionType === 'custom') return internalOption.label;

      return getDefaultValue();
    };
  }, [props.getOptionLabel, emptyLabel]);

  // handle equality for CustomOptions
  const isOptionEqualToValue: typeof props['isOptionEqualToValue'] =
    useMemo(() => {
      const original = props.isOptionEqualToValue ?? defaultEqualityFunction;
      return (option1, option2) => {
        if (original(option1, option2)) return true;

        if (
          option1 === null ||
          option1 === undefined ||
          option2 === null ||
          option2 === undefined
        )
          return false;

        if (typeof option1 !== 'object' || typeof option2 !== 'object')
          return false;

        const internalOption1 = option1 as unknown as InternalOptionType;
        const internalOption2 = option2 as unknown as InternalOptionType;
        if (
          internalOption1.__optionType === 'custom' &&
          internalOption2.__optionType === 'custom'
        ) {
          return internalOption1.label == internalOption2.label;
        }
        return false;
      };
    }, [props.getOptionLabel, emptyLabel]);

  const options: T[] = useMemo(() => {
    const result: T[] = [];
    if (!required && !props.multiple) {
      if (!props.options.includes(null!))
        result.push(notSelectedOption as OptionType<T> as any);
    }
    if (props.customOptions) {
      result.push(
        ...props.customOptions.map(
          (x) => ({ __optionType: 'custom', ...x } as OptionType<T> as any),
        ),
      );
    }

    result.push(...props.options);

    // update selected value
    if (props.value) {
      let newSelectedValues: any;
      if (props.multiple) {
        newSelectedValues = result.filter((x) =>
          (props.value as any).includes((z: any) => isOptionEqualToValue(x, z)),
        );
      } else {
        newSelectedValues = result.find((x) =>
          isOptionEqualToValue(x, props.value as any),
        );
      }
      props.onChange?.({} as any, newSelectedValues, 'selectOption');
    }

    return result;
  }, [required, props.options, props.customOptions]);

  // handle CustomOptions selection
  const onChange: typeof props['onChange'] = useMemo(() => {
    if (!props.customOptions || props.customOptions.length === 0)
      return props.onChange;

    return (event, value, reason, details) => {
      if (typeof value === 'object') {
        const internalOption = value as unknown as InternalOptionType;
        if (internalOption.__optionType === 'custom') {
          internalOption.onClick();
          return;
        }
      }
      props.onChange?.(event, value, reason, details);
    };
  }, [props.onChange, props.customOptions]);

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
        isOptionEqualToValue={isOptionEqualToValue}
        onChange={onChange}
        disableClearable={props.required}
        value={props.value ?? null!}
      />
      {!!errorText && <div className={styles.errorText}>{errorText}</div>}
    </div>
  );
}
function defaultEqualityFunction(x: any, y: any) {
  return x == y;
}
