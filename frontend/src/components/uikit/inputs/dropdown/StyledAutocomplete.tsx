import clsx from 'clsx';
import * as React from 'react';
import { useCallback, useMemo, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import { ReactComponent as ArrowDownIcon } from 'assets/icons/arrow-down.svg';

import styles from './StyledAutocomplete.module.scss';
import { Autocomplete, Paper, Popper, PopperProps } from '@mui/material';
import { Input } from '../Input';
import { AutocompleteProps } from '@mui/material/Autocomplete/Autocomplete';
import { AutocompleteClasses } from '@mui/material/Autocomplete/autocompleteClasses';
import {
  VirtualizedListboxComponent,
  VirtualizedListboxComponentProps,
} from './VirtualizedListboxAdapter';
import {
  CustomOption,
  PropertyAccessor,
  StyledAutocompleteProps,
} from './types';
import { useTriggerOnClickOutsideElement } from '../../../../helpers/useTriggerOnClickOutsideElement';
import equal from 'fast-deep-equal/react';

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
    idFunction,
    placeholder,
    rootClassName,
    style,
    required,
    testId,
    errorText,
    enableSearch,
    popupHeader,
    popupFooter,
    postfixRenderer,
    customOptions,
    itemSize,
    variant,
    useVirtualization,
    autosizeInputWidth,
    popupWidth,
    maxPopupWidth,
    additionalWidth,
    ...rest
  } = {
    ...props,
    placeholder:
      props.placeholder ?? i18next.t('uikit.inputs.nothing_selected'),
    variant: props.variant ?? 'normal',
    useVirtualization: props.useVirtualization ?? true,
  };

  const classes: Partial<AutocompleteClasses> = useMemo(
    () => ({
      ...props.classes,
      option: clsx(
        styles.optionValue,
        styles.fontConfig,
        variant === 'normal' && styles.optionValueNormal,
        props.classes?.option,
      ),
      popper: clsx(
        styles.dropdownCallout,
        styles.fontConfig,
        props.classes?.popper,
      ),
      listbox: clsx(styles.listbox, props.classes?.listbox),
    }),
    [props.classes, variant],
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
    return getEqualityFunction(props.isOptionEqualToValue, idFunction);
  }, [props.isOptionEqualToValue, idFunction]);

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
      if (props.multiple) {
        const newSelectedValues = props.options.filter((x) =>
          (props.value as any).includes((z: any) => isOptionEqualToValue(x, z)),
        );
        if (!equal(newSelectedValues, props.value)) {
          props.onChange?.({} as any, newSelectedValues as any, 'selectOption');
        }
      } else {
        const newSelectedValue = props.options.find((x) =>
          isOptionEqualToValue(x, props.value as any),
        );
        if (newSelectedValue !== props.value) {
          props.onChange?.(
            {} as any,
            (newSelectedValue ?? null) as any,
            'selectOption',
          );
        }
      }
    }

    return [...result, ...props.options];
  }, [required, props.options, customOptions]);

  const renderOption: NonNullable<
    AutocompleteProps<T, Multiple, Required, FreeSolo>['renderOption']
  > = useCallback(
    (liProps, option, state) => {
      if (
        option &&
        typeof option === 'object' &&
        (option as any)['__type'] === 'custom-option'
      ) {
        const customOption = option as unknown as CustomOption;
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
        <li
          {...liProps}
          className={clsx(liProps.className, styles.liWithPrefix)}
        >
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

  const listboxProps: VirtualizedListboxComponentProps = useMemo(
    () => ({ itemSize: itemSize ?? variant === 'formInput' ? 40 : 32 }),
    [itemSize, variant],
  );
  const componentProps = useMemo(() => {
    const popperProps: Partial<PopperAutocompleteProps> &
      PopperAutocompleteAdditionalProps = {
      // When Autocomplete is placed in a Popup (e.g. Tooltip),
      // we should not close the outer Popup when clicked on DropDown items.
      // So we need to disablePortal.
      disablePortal: true,
      popupWidth: popupWidth,
      maxPopupWidth: maxPopupWidth,
      additionalWidth: additionalWidth,
      useVirtualization: useVirtualization,
      options: () => options.map(getOptionLabel),
    };

    return {
      paper: {
        footer: popupFooter,
        header: popupHeader,
        onClickOutside: onClickOutsidePaper,
      } as PaperComponentProps,
      popper: popperProps as any,
    };
  }, [
    additionalWidth,
    getOptionLabel,
    maxPopupWidth,
    onClickOutsidePaper,
    options,
    popupFooter,
    popupHeader,
    popupWidth,
    useVirtualization,
  ]);
  return (
    <div
      className={clsx(styles.rootContainer, rootClassName)}
      style={style}
      data-variant={variant}
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
              style={
                autosizeInputWidth && value
                  ? { width: `calc(${(value as string).length}ch + 40px)` }
                  : undefined
              }
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
              variant={variant}
            />
          );
        }}
        componentsProps={componentProps as any}
        ListboxProps={useVirtualization ? listboxProps : undefined}
        ListboxComponent={
          useVirtualization ? (VirtualizedListboxComponent as any) : undefined
        }
        PaperComponent={PaperComponentWithHeaderFooter}
        PopperComponent={PopperComponentForAutocomplete as any}
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

type PopperAutocompleteAdditionalProps = {
  /*
   * Could be used to specify popupWidth.
   * Makes sense to use when `useVirtualization` is true, otherwise popupWidth will be equal to Input width
   * (if `useVirtualization` is false, autosize is enabled by default)
   */
  popupWidth: number | 'autosize' | undefined;

  /*
   * Specifies maximum width of the popup.
   * Default to '450px'
   */
  maxPopupWidth: number | undefined;

  /*
   * Makes sense to use when `useVirtualization` is true and `popupWidth` is auto.
   * Specifies the width that is added to automatically calculated item width.
   * Could be used for paddings and/or postfix.
   * Defaults to '40px'
   */
  additionalWidth: string | number | undefined;

  /*
   * If true, uses react-window to render elements in drop-down list
   * True by default.
   */
  useVirtualization: boolean | undefined;

  options: () => string[];
};

type PopperAutocompleteProps = PopperProps & PopperAutocompleteAdditionalProps;

function getWidth(
  popupWidth: 'autosize' | number | undefined,
  useVirtualization: undefined | boolean,
  options: string[] | undefined,
  additionalWidth: string | number,
): string | number | undefined {
  // if popupWidth is specified and not 'autosize' - return it as is
  if (popupWidth && popupWidth !== 'autosize') return popupWidth;

  // if popupWidth is not defined and virtualization is enabled - return `undefined`, popup will have the width of an input
  if (!popupWidth && useVirtualization) return undefined;

  // if we are here, it means we need to handle autosize

  if (!useVirtualization) {
    // if no virtualization, then autosize is easy
    return 'auto !important';
  }

  // here we need to calculate the width for the VirtualizedList
  const maxOptionLength = options
    ?.map((x) => x.length)
    .reduce(
      (previousValue, currentValue) =>
        currentValue > previousValue ? currentValue : previousValue,
      0,
    );

  return `calc(${maxOptionLength}ch + ${additionalWidth})`;
}

const PopperComponentForAutocomplete = React.memo(
  function PopperComponentForAutocomplete(props: PopperAutocompleteProps) {
    const {
      popupWidth,
      maxPopupWidth,
      additionalWidth,
      useVirtualization,
      options,
      ...rest
    } = {
      ...props,
      maxPopupWidth: props.maxPopupWidth ?? '450px',
      additionalWidth: props.additionalWidth ?? '40px',
    };

    const width = getWidth(
      popupWidth,
      useVirtualization,
      options(),
      additionalWidth,
    );

    return (
      <Popper
        {...rest}
        // we override the style to make Popper width bigger than Input
        style={{
          minWidth: props?.style?.width,
          maxWidth: maxPopupWidth,
          width: width,
        }}
      />
    );
  },
);

type PaperComponentProps = {
  header?: any;
  footer?: any;
  onClickOutside?: () => void;
};

const PaperComponentWithHeaderFooter = React.memo(
  function PaperComponentWithHeaderFooter(props: PaperComponentProps & any) {
    const { onClickOutside, header, footer, children, ...rest } = props;
    const ref = useRef<HTMLDivElement>(null);
    useTriggerOnClickOutsideElement(
      ref,
      props.onClickOutside!,
      !!props.onClickOutside,
    );

    return (
      <Paper {...rest} ref={ref}>
        {header}
        {children}
        {footer}
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
