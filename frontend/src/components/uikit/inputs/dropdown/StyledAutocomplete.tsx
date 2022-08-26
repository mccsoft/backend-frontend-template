import clsx from 'clsx';
import * as React from 'react';
import { useCallback, useMemo, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import { ReactComponent as ArrowDownIcon } from 'assets/icons/arrow-down.svg';

import styles from './StyledAutocomplete.module.scss';
import {
  Autocomplete,
  AutocompleteFreeSoloValueMapping,
  Paper,
} from '@mui/material';
import { Input } from '../Input';
import {
  AutocompleteProps,
  AutocompleteRenderOptionState,
} from '@mui/material/Autocomplete/Autocomplete';
import { AutocompleteClasses } from '@mui/material/Autocomplete/autocompleteClasses';
import { VirtualizedListboxComponent } from './VirtualizedListboxAdapter';
import {
  CustomOption,
  PropertyAccessor,
  StyledAutocompleteProps,
} from './types';
import { useTriggerOnClickOutsideElement } from '../../../../helpers/useTriggerOnClickOutsideElement';

const caretDown = <ArrowDownIcon className={styles.expandIcon} />;

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
    onChange,
    placeholder,
    rootClassName,
    required,
    testId,
    errorText,
    enableSearch,
    popupHeader,
    popupFooter,
    postfixRenderer,
    customOptions,
    ...rest
  } = {
    ...props,
    placeholder:
      props.placeholder ?? i18next.t('uikit.inputs.nothing_selected'),
  };

  const classes: Partial<AutocompleteClasses> = useMemo(
    () => ({
      ...props.classes,
      option: clsx(
        styles.optionValue,
        props.variant === 'normal' && styles.optionValueNormal,
        props.classes?.option,
      ),
      popper: clsx(styles.dropdownCallout, props.classes?.popper),
      listbox: clsx(styles.listbox, props.classes?.listbox),
    }),
    [props.classes],
  );

  const getOptionLabel: NonNullable<
    AutocompleteProps<T, Multiple, Required, FreeSolo>['getOptionLabel']
  > = useMemo(() => {
    const baseFunction = convertPropertyAccessorToFunction(
      props.getOptionLabel,
    ) as any;
    return (option) =>
      option === null || option === undefined
        ? placeholder
        : baseFunction(option);
  }, [props.getOptionLabel, placeholder]);

  const isOptionEqualToValue: AutocompleteProps<
    T,
    Multiple,
    Required,
    FreeSolo
  >['isOptionEqualToValue'] = useMemo(() => {
    return getEqualityFunction(props.isOptionEqualToValue, props.idFunction);
  }, [props.isOptionEqualToValue, props.idFunction]);

  const options: T[] = useMemo(() => {
    const result: T[] = [];
    if (!required && !props.multiple) {
      if (!props.options.includes(null!)) result.push(null!);
    }

    if (customOptions) {
      result.push(
        ...(customOptions.map((x) => ({
          ...x,
          __type: 'custom-option',
        })) as any),
      );
    }

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

    return [...result, ...props.options];
  }, [required, props.options, customOptions]);

  const renderOption: AutocompleteProps<
    T,
    Multiple,
    Required,
    FreeSolo
  >['renderOption'] = useCallback(
    (
      liProps: React.HTMLAttributes<HTMLLIElement>,
      option: T | CustomOption | AutocompleteFreeSoloValueMapping<FreeSolo>,
      state: AutocompleteRenderOptionState,
    ) => {
      if (
        option &&
        typeof option === 'object' &&
        (option as any)['__type'] === 'custom-option'
      ) {
        const customOption = option as CustomOption;
        return (
          <li
            {...liProps}
            onClick={(e) => {
              e.preventDefault();
              customOption.onClick();
            }}
          >
            {customOption.label}
          </li>
        );
      }

      if (props.renderOption)
        return props.renderOption(liProps, option as any, state);
      return (
        <li {...liProps}>
          {getOptionLabel(option as any)}
          {postfixRenderer?.(option as any)}
        </li>
      );
    },
    [props.renderOption, postfixRenderer],
  );

  const closeAutocomplete = useRef<React.FocusEventHandler>();
  const onClickOutsidePaper = useCallback((e: MouseEvent) => {
    closeAutocomplete.current?.(e as any);
  }, []);
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
            ? (params.InputProps.startAdornment as string) ?? ''
            : params.inputProps.value;
          closeAutocomplete.current = params.inputProps.onBlur;
          return (
            <Input
              containerRef={params.InputProps.ref}
              placeholder={placeholder}
              {...params.inputProps}
              className={clsx(
                params.inputProps.className,
                styles.nonEditableInput,
              )}
              /* If `onBlur` has it's default value
               * the DropDown will close when input loses the focus.
               * We need to prevent that, since there might be inputs inside the DropDown */
              onBlur={undefined}
              value={value}
              size={undefined}
              readOnly={!enableSearch}
              endAdornment={caretDown}
              variant={props.variant}
            />
          );
        }}
        componentsProps={{
          paper: {
            footer: popupFooter,
            header: popupHeader,
            onClickOutside: onClickOutsidePaper,
          } as PaperComponentProps as any,
        }}
        ListboxProps={
          {
            itemSize: 40,
          } as any
        }
        ListboxComponent={VirtualizedListboxComponent as any}
        PaperComponent={PaperComponentWithHeaderFooter}
        classes={classes}
        data-test-id={testId}
        data-error={!!errorText}
        placeholder={placeholder}
        getOptionLabel={getOptionLabel}
        renderOption={renderOption}
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

type PaperComponentProps = {
  header?: any;
  footer?: any;
  onClickOutside?: () => void;
};

const PaperComponentWithHeaderFooter = React.memo(
  function PaperComponentWithHeaderFooter(props: PaperComponentProps & any) {
    const ref = useRef<HTMLDivElement>(null);
    useTriggerOnClickOutsideElement(
      ref,
      props.onClickOutside!,
      !!props.onClickOutside,
    );

    return (
      <Paper {...props} ref={ref}>
        {props.header}
        {props.children}
        {props.footer}
      </Paper>
    );
  },
);

function convertPropertyAccessorToEqualityFunction<T>(
  key: keyof T,
): (option1: T, option2: T) => boolean {
  const propertyAccessorFunction = convertPropertyPathToFunction(key);
  return (option1, option2) =>
    propertyAccessorFunction(option1) == propertyAccessorFunction(option2);
}

export function convertPropertyAccessorToFunction<
  T,
  Multiple extends boolean | undefined,
  Required extends boolean | undefined,
  FreeSolo extends boolean | undefined,
>(getOptionLabel?: PropertyAccessor<T>): (option: T) => string {
  return getOptionLabel
    ? typeof getOptionLabel !== 'function'
      ? convertPropertyPathToFunction(getOptionLabel)
      : (getOptionLabel as any)
    : (option1: any) => option1 as any;
}

export function convertPropertyPathToFunction<T>(
  key: keyof T,
): (option: any) => string {
  return (option: T) =>
    option === null || option === undefined || typeof option !== 'object'
      ? (option as any)
      : option[key];
}

function getEqualityFunction<T>(
  isOptionEqualToValue: StyledAutocompleteProps<
    T,
    false,
    false,
    false
  >['isOptionEqualToValue'],
  idFunction: StyledAutocompleteProps<T, false, false, false>['idFunction'],
) {
  if (isOptionEqualToValue) return isOptionEqualToValue;
  if (!idFunction) return defaultEqualityFunction;

  if (typeof idFunction !== 'function') {
    return convertPropertyAccessorToEqualityFunction(idFunction);
  }

  return (option1: T, option2: T) => idFunction(option1) == idFunction(option2);
}
